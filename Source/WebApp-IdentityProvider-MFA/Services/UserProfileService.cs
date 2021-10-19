// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebApp_IdentityProvider_MFA.Data;

namespace WebApp_IdentityProvider_MFA.Services
{
    /// <summary>
    /// This service obtains, transform, and provides the user data that will be sent to an OpenIDConnect Client such as FranceConnect.
    /// </summary>
    public class UserProfileService : IProfileService
    {
        protected UserManager<ApplicationUser> _userManager;

        public UserProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {

            //Add the requested claims that are already in the user's object claims. Usually, that is
            context.AddRequestedClaims(context.Subject.Claims);

            //Obtain additional user data from the data store
            var user = await _userManager.GetUserAsync(context.Subject);

            //Map and transform the data
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Email, user.Email),
                new Claim(JwtClaimTypes.GivenName, user.GivenName),
                new Claim(JwtClaimTypes.FamilyName, user.FamilyName),
                new Claim(JwtClaimTypes.Gender, user.Gender),
                new Claim("birthdate", user.BirthDate.ToString("yyyy/MM/dd")),
                new Claim("birthplace", user.BirthPlace),
                new Claim("birthcountry", user.BirthCountry)
            };
            if (user.PreferredName != null) claims.Add(new Claim(JwtClaimTypes.PreferredUserName, user.PreferredName));

            //Add the ones that were requested by the user to the context object
            context.AddRequestedClaims(claims);
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            //You can put conditions here to refuse login by returning false. If a user has been deactived during a token request, for example.

            context.IsActive = true;
        }
    }
}
