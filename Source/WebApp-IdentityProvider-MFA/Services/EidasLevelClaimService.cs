// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using IdentityModel;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebApp_IdentityProvider_MFA.Data;

namespace WebApp_IdentityProvider_MFA.Services
{
    /// <summary>
    /// This class is responsible for determining which claims to include in the tokens. It uses the default IdentityServerImplementation, and adds eidasLevel in the acr claim of the token, as requir
    /// </summary>
    public class EidasLevelClaimService : DefaultClaimsService
    {

        protected UserManager<ApplicationUser> _userManager;

        public EidasLevelClaimService(IProfileService profile,
        UserManager<ApplicationUser> userManager, ILogger<DefaultClaimsService> logger) : base(profile, logger)
        {
            _userManager = userManager;
        }
        public override async Task<IEnumerable<Claim>> GetIdentityTokenClaimsAsync(ClaimsPrincipal subject, ResourceValidationResult resources, bool includeAllIdentityClaims, ValidatedRequest request)
        {
            var claims = new List<Claim>(await base.GetIdentityTokenClaimsAsync(subject, resources, includeAllIdentityClaims, request));

            var user = await _userManager.GetUserAsync(subject);
            var eidasLevel = user.TwoFactorEnabled ? "eidas2" : "eidas1"; // This is for demonstration purposes only.

            claims.Add(new Claim(JwtClaimTypes.AuthenticationContextClassReference, eidasLevel));
            return claims;
        }
    }
}
