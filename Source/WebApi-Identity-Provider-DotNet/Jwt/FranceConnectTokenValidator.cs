// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE-IdentityServer4-Templates.md in the project root for license information.

// This file was changed for the needs of the projects. These modifications are subject to 
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using IdentityModel;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApi_Identity_Provider_DotNet.Configuration;

namespace WebApi_Identity_Provider_DotNet.Jwt
{

    public class FranceConnectTokenValidator : ITokenValidator
    {
        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;
        private readonly IHttpContextAccessor _context;
        private readonly IReferenceTokenStore _referenceTokenStore;
        private readonly ICustomTokenValidator _customValidator;
        private readonly IClientStore _clients;
        private readonly IProfileService _profile;
        private readonly IdentityInMemoryConfiguration _identityConfig;

        public FranceConnectTokenValidator(IdentityServerOptions options, IHttpContextAccessor context, IClientStore clients, IProfileService profile, IReferenceTokenStore referenceTokenStore, IRefreshTokenStore refreshTokenStore, ICustomTokenValidator customValidator, IdentityInMemoryConfiguration identityConfig,ILogger<ITokenValidator> logger)
        {
            _options = options;
            _context = context;
            _clients = clients;
            _profile = profile;
            _referenceTokenStore = referenceTokenStore;
            _customValidator = customValidator;
            _identityConfig = identityConfig;
            _logger = logger; 
        }

        public async Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string clientId = null, bool validateLifetime = true)
        {
            _logger.LogTrace("Start identity token validation");

            if (token.Length > _options.InputLengthRestrictions.Jwt)
            {
                _logger.LogError("JWT too long");
                return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                clientId = GetClientIdFromJwt(token);

                if (string.IsNullOrWhiteSpace(clientId))
                {
                    _logger.LogError("No clientId supplied, can't find id in identity token.");
                    return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
                }
            }

            var client = await _clients.FindEnabledClientByIdAsync(clientId);
            if (client == null)
            {
                _logger.LogError("Unknown or disabled client: {clientId}.", clientId);
                return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
            }

            var keys = new List<SymmetricSecurityKey> { new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_identityConfig.FranceConnectSecret)) };
            var result = await ValidateJwtAsync(token, keys, audience: clientId, validateLifetime: validateLifetime);

            result.Client = client;

            if (result.IsError)
            {
                LogError("Error validating JWT");
                return result;
            }

            var customResult = await _customValidator.ValidateIdentityTokenAsync(result);

            if (customResult.IsError)
            {
                LogError("Custom validator failed: " + (customResult.Error ?? "unknown"));
                return customResult;
            }

            LogSuccess();
            return customResult;
        }

        public async Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null)
        {
            _logger.LogTrace("Start access token validation");
            TokenValidationResult result;

            if (token.Contains("."))
            {
                if (token.Length > _options.InputLengthRestrictions.Jwt)
                {
                    _logger.LogError("JWT too long");

                    return new TokenValidationResult
                    {
                        IsError = true,
                        Error = OidcConstants.ProtectedResourceErrors.InvalidToken,
                        ErrorDescription = "Token too long"
                    };
                }

                result = await ValidateJwtAsync(
                    token,
                    new List<SymmetricSecurityKey> { new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_identityConfig.FranceConnectSecret)) });
            }
            else
            {
                if (token.Length > _options.InputLengthRestrictions.TokenHandle)
                {
                    _logger.LogError("token handle too long");

                    return new TokenValidationResult
                    {
                        IsError = true,
                        Error = OidcConstants.ProtectedResourceErrors.InvalidToken,
                        ErrorDescription = "Token too long"
                    };
                }

                result = await ValidateReferenceAccessTokenAsync(token);
            }

            if (result.IsError)
            {
                return result;
            }

            // make sure client is still active (if client_id claim is present)
            var clientClaim = result.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.ClientId);
            if (clientClaim != null)
            {
                var client = await _clients.FindEnabledClientByIdAsync(clientClaim.Value);
                if (client == null)
                {
                    result.IsError = true;
                    result.Error = OidcConstants.ProtectedResourceErrors.InvalidToken;
                    result.Claims = null;

                    return result;
                }
            }

            // make sure user is still active (if sub claim is present)
            var subClaim = result.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Subject);
            if (subClaim != null)
            {
                var principal = Principal.Create("tokenvalidator", result.Claims.ToArray());

                if (!string.IsNullOrWhiteSpace(result.ReferenceTokenId))
                {
                    principal.Identities.First().AddClaim(new Claim(JwtClaimTypes.ReferenceTokenId, result.ReferenceTokenId));
                }

                var isActiveCtx = new IsActiveContext(principal, result.Client, IdentityServerConstants.ProfileIsActiveCallers.AccessTokenValidation);
                await _profile.IsActiveAsync(isActiveCtx);

                if (isActiveCtx.IsActive == false)
                {
                    _logger.LogError("User marked as not active: {subject}", subClaim.Value);

                    result.IsError = true;
                    result.Error = OidcConstants.ProtectedResourceErrors.InvalidToken;
                    result.Claims = null;

                    return result;
                }
            }

            // check expected scope(s)
            if (!string.IsNullOrWhiteSpace(expectedScope))
            {
                var scope = result.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Scope && c.Value == expectedScope);
                if (scope == null)
                {
                    LogError($"Checking for expected scope {expectedScope} failed");
                    return Invalid(OidcConstants.ProtectedResourceErrors.InsufficientScope);
                }
            }

            _logger.LogDebug("Calling into custom token validator: {type}", _customValidator.GetType().FullName);
            var customResult = await _customValidator.ValidateAccessTokenAsync(result);

            if (customResult.IsError)
            {
                LogError("Custom validator failed: " + (customResult.Error ?? "unknown"));
                return customResult;
            }

            LogSuccess();
            return customResult;
        }

        private async Task<TokenValidationResult> ValidateJwtAsync(string jwt, IEnumerable<SymmetricSecurityKey> symmetricKeys, bool validateLifetime = true, string audience = null)
        {
            var handler = new JwtSecurityTokenHandler();
            handler.InboundClaimTypeMap.Clear();

            var parameters = new TokenValidationParameters
            {
                ValidIssuer = _context.HttpContext.GetIdentityServerIssuerUri(),
                IssuerSigningKeys = symmetricKeys,
                ValidateLifetime = validateLifetime
            };

            if (!string.IsNullOrWhiteSpace(audience))
            {
                parameters.ValidAudience = audience;
            }
            else
            {
                parameters.ValidateAudience = false;
            }

            try
            {
                var id = handler.ValidateToken(jwt, parameters, out var securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;

                // if no audience is specified, we make at least sure that it is an access token
                if (string.IsNullOrWhiteSpace(audience))
                {
                    if (!string.IsNullOrWhiteSpace(_options.AccessTokenJwtType))
                    {
                        var type = jwtSecurityToken.Header.Typ;
                        if (!string.Equals(type, _options.AccessTokenJwtType))
                        {
                            return new TokenValidationResult
                            {
                                IsError = true,
                                Error = "invalid JWT token type"
                            };
                        }

                    }
                }

                // load the client that belongs to the client_id claim
                Client client = null;
                var clientId = id.FindFirst(JwtClaimTypes.ClientId);
                if (clientId != null)
                {
                    client = await _clients.FindEnabledClientByIdAsync(clientId.Value);
                    if (client == null)
                    {
                        throw new InvalidOperationException("Client does not exist anymore.");
                    }
                }

                var claims = id.Claims.ToList();

                // check the scope format (array vs space delimited string)
                var scopes = claims.Where(c => c.Type == JwtClaimTypes.Scope).ToArray();
                if (scopes.Any())
                {
                    foreach (var scope in scopes)
                    {
                        if (scope.Value.Contains(" "))
                        {
                            claims.Remove(scope);

                            var values = scope.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            foreach (var value in values)
                            {
                                claims.Add(new Claim(JwtClaimTypes.Scope, value));
                            }
                        }
                    }
                }

                return new TokenValidationResult
                {
                    IsError = false,

                    Claims = claims,
                    Client = client,
                    Jwt = jwt
                };
            }
            catch (SecurityTokenExpiredException expiredException)
            {
                _logger.LogInformation(expiredException, "JWT token validation error: {exception}", expiredException.Message);
                return Invalid(OidcConstants.ProtectedResourceErrors.ExpiredToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JWT token validation error: {exception}", ex.Message);
                return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
            }
        }

        private async Task<TokenValidationResult> ValidateReferenceAccessTokenAsync(string tokenHandle)
        {
            var token = await _referenceTokenStore.GetReferenceTokenAsync(tokenHandle);

            if (token == null)
            {
                LogError("Invalid reference token.");
                return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
            }

            if (DateTimeOffset.UtcNow >= token.CreationTime.AddSeconds(token.Lifetime))
            {
                LogError("Token expired.");

                await _referenceTokenStore.RemoveReferenceTokenAsync(tokenHandle);
                return Invalid(OidcConstants.ProtectedResourceErrors.ExpiredToken);
            }

            // load the client that is defined in the token
            Client client = null;
            if (token.ClientId != null)
            {
                client = await _clients.FindEnabledClientByIdAsync(token.ClientId);
            }

            if (client == null)
            {
                LogError($"Client deleted or disabled: {token.ClientId}");
                return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
            }

            return new TokenValidationResult
            {
                IsError = false,

                Client = client,
                Claims = ReferenceTokenToClaims(token),
                ReferenceToken = token,
                ReferenceTokenId = tokenHandle
            };
        }

        private IEnumerable<Claim> ReferenceTokenToClaims(Token token)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Issuer, token.Issuer),
                new Claim(JwtClaimTypes.NotBefore, new DateTimeOffset(token.CreationTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtClaimTypes.Expiration, new DateTimeOffset(token.CreationTime).AddSeconds(token.Lifetime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            foreach (var aud in token.Audiences)
            {
                claims.Add(new Claim(JwtClaimTypes.Audience, aud));
            }

            claims.AddRange(token.Claims);
            return claims;
        }

        private string GetClientIdFromJwt(string token)
        {
            try
            {
                var jwt = new JwtSecurityToken(token);
                var clientId = jwt.Audiences.FirstOrDefault();

                return clientId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Malformed JWT token: {exception}", ex.Message);
                return null;
            }
        }

        private TokenValidationResult Invalid(string error)
        {
            return new TokenValidationResult
            {
                IsError = true,
                Error = error
            };
        }

        private void LogError(string message)
        {
            _logger.LogError(message + "\n{@logMessage}");
        }

        private void LogSuccess()
        {
            _logger.LogDebug("Token validation success\n{@logMessage}");
        }
    }
}