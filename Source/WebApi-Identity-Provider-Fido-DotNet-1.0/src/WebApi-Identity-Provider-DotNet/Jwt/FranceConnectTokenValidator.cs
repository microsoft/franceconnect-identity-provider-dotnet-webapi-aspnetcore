//
// The MIT License (MIT)
// Copyright (c) 2016 Microsoft France
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// You may obtain a copy of the License at https://opensource.org/licenses/MIT
//

using IdentityModel;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Hosting;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApi_Identity_Provider_DotNet.Configuration;

namespace WebApi_Identity_Provider_DotNet.Jwt
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class FranceConnectTokenValidator : ITokenValidator
    {
        private readonly ILogger _logger;
        private readonly IdentityServerOptions _options;
        private readonly IdentityServerContext _context;
        private readonly ITokenHandleStore _tokenHandles;
        private readonly ICustomTokenValidator _customValidator;
        private readonly IClientStore _clients;
        private readonly IEnumerable<IValidationKeysStore> _keys;

        public FranceConnectTokenValidator(IdentityServerOptions options, IdentityServerContext context, IClientStore clients, ITokenHandleStore tokenHandles, ICustomTokenValidator customValidator, IEnumerable<IValidationKeysStore> keys, ILogger<TokenValidator> logger)
        {
            _options = options;
            _context = context;
            _clients = clients;
            _tokenHandles = tokenHandles;
            _customValidator = customValidator;
            _keys = keys;
            _logger = logger;
        }

        public virtual async Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string clientId = null, bool validateLifetime = true)
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

            var client = await _clients.FindClientByIdAsync(clientId);
            if (client == null)
            {
                _logger.LogError("Unknown or diabled client.");
                return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
            }

            var keys = new List<SymmetricSecurityKey> { new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Clients.FranceConnectSecret)) };
            var result = await ValidateJwtAsync(token, clientId, keys, validateLifetime);

            result.Client = client;

            if (result.IsError)
            {
                _logger.LogError("Error validating JWT");
                return result;
            }

            var customResult = await _customValidator.ValidateIdentityTokenAsync(result);

            if (customResult.IsError)
            {
                _logger.LogError("Custom validator failed: " + (customResult.Error ?? "unknown"));
                return customResult;
            }

            _logger.LogInformation("Token validation succeded");
            return customResult;
        }

        public virtual async Task<TokenValidationResult> ValidateAccessTokenAsync(string token, string expectedScope = null)
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
                    string.Format(Constants.AccessTokenAudience, _context.GetIssuerUri().EnsureTrailingSlash()),
                    new List<SymmetricSecurityKey> { new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Clients.FranceConnectSecret)) });
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

            if (!string.IsNullOrWhiteSpace(expectedScope))
            {
                var scope = result.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Scope && c.Value == expectedScope);
                if (scope == null)
                {
                    _logger.LogError(string.Format("Checking for expected scope {0} failed", expectedScope));
                    return Invalid(OidcConstants.ProtectedResourceErrors.InsufficientScope);
                }
            }

            var customResult = await _customValidator.ValidateAccessTokenAsync(result);

            if (customResult.IsError)
            {
                _logger.LogError("Custom validator failed: " + (customResult.Error ?? "unknown"));
                return customResult;
            }

            _logger.LogInformation("Token validation Succeded");
            return customResult;
        }

        private async Task<TokenValidationResult> ValidateJwtAsync(string jwt, string audience, IEnumerable<SymmetricSecurityKey> symmetricKeys, bool validateLifetime = true)
        {
            var handler = new JwtSecurityTokenHandler();
            handler.InboundClaimTypeMap.Clear();


            var parameters = new TokenValidationParameters
            {
                ValidIssuer = _context.GetIssuerUri(),
                IssuerSigningKeys = symmetricKeys,
                ValidateLifetime = validateLifetime,
                ValidAudience = audience
            };

            try
            {
                SecurityToken jwtToken;
                var id = handler.ValidateToken(jwt, parameters, out jwtToken);

                // load the client that belongs to the client_id claim
                Client client = null;
                var clientId = id.FindFirst(JwtClaimTypes.ClientId);
                if (clientId != null)
                {
                    client = await _clients.FindClientByIdAsync(clientId.Value);
                    if (client == null)
                    {
                        throw new InvalidOperationException("Client does not exist anymore.");
                    }
                }

                return new TokenValidationResult
                {
                    IsError = false,

                    Claims = id.Claims,
                    Client = client,
                    Jwt = jwt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("JWT token validation error", ex);
                return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
            }
        }

        private async Task<TokenValidationResult> ValidateReferenceAccessTokenAsync(string tokenHandle)
        {
            var token = await _tokenHandles.GetAsync(tokenHandle);

            if (token == null)
            {
                _logger.LogError("Token handle not found");
                return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
            }

            if (token.Type != OidcConstants.TokenTypes.AccessToken)
            {
                _logger.LogError("Token handle does not resolve to an access token - but instead to: " + token.Type);

                await _tokenHandles.RemoveAsync(tokenHandle);
                return Invalid(OidcConstants.ProtectedResourceErrors.InvalidToken);
            }

            if (DateTimeOffset.UtcNow >= token.CreationTime.AddSeconds(token.Lifetime))
            {
                _logger.LogError("Token expired.");

                await _tokenHandles.RemoveAsync(tokenHandle);
                return Invalid(OidcConstants.ProtectedResourceErrors.ExpiredToken);
            }

            return new TokenValidationResult
            {
                IsError = false,

                Client = token.Client,
                Claims = ReferenceTokenToClaims(token),
                ReferenceToken = token,
                ReferenceTokenId = tokenHandle
            };
        }

        private IEnumerable<Claim> ReferenceTokenToClaims(Token token)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Audience, token.Audience),
                new Claim(JwtClaimTypes.Issuer, token.Issuer),
                new Claim(JwtClaimTypes.NotBefore, token.CreationTime.ToEpochTime().ToString()),
                new Claim(JwtClaimTypes.Expiration, token.CreationTime.AddSeconds(token.Lifetime).ToEpochTime().ToString())
            };

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
                _logger.LogError("Malformed JWT token", ex);
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

    }

    public static class StringExtensions
    {
        public static string EnsureTrailingSlash(this string url)
        {
            if (!url.EndsWith("/"))
            {
                return url + "/";
            }

            return url;
        }
    }
}
