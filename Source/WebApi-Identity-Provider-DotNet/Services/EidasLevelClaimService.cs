// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using IdentityModel;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApi_Identity_Provider_DotNet.Services
{
    /// <summary>
    /// This class is responsible for determining which claims to include in the tokens. It uses the default IdentityServerImplementation, and adds eidasLevel in the acr claim of the token, as requir
    /// </summary>
    public class EidasLevelClaimService : DefaultClaimsService
    {
        public EidasLevelClaimService(IProfileService profile, ILogger<DefaultClaimsService> logger) : base(profile, logger)
        {
        }
        public override async Task<IEnumerable<Claim>> GetIdentityTokenClaimsAsync(ClaimsPrincipal subject, ResourceValidationResult resources, bool includeAllIdentityClaims, ValidatedRequest request)
        {
            var claims = new List<Claim>(await base.GetIdentityTokenClaimsAsync(subject, resources, includeAllIdentityClaims, request));

            claims.Add(new Claim(JwtClaimTypes.AuthenticationContextClassReference, "eidas1"));
            return claims;
        }
    }
}
