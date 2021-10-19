// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using WebApp_IdentityProvider_MFA.Data;
using WebApp_IdentityProvider_MFA.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp_IdentityProvider_MFA.Areas.Identity.Pages.Account
{
    public class Login2FaChoiceModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<Login2FaChoiceModel> _logger;

        public Login2FaChoiceModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<Login2FaChoiceModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public ApplicationUser MfaUser { get; set; }

        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

        public IDictionary<string,string> Providers { get; set; }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
        {            
            // Ensure the user has gone through the username & password screen first
            MfaUser = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (MfaUser == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }
            ReturnUrl = returnUrl;
            RememberMe = rememberMe;
            

            var validTwoFactorProviders = await _userManager.GetValidTwoFactorProvidersAsync(MfaUser);
            Providers = new Dictionary<string, string>(5);

            var HasAuthenticator = validTwoFactorProviders.Any(provider => provider == _userManager.Options.Tokens.AuthenticatorTokenProvider);
            if (HasAuthenticator) Providers.Add("./LoginWithAuthenticator", "Application d'authentification");
            var HasFIDO2 = validTwoFactorProviders.Any(provider => provider == FIDO2TwoFactorProvider.Constants.ProviderName);
            
            if (HasFIDO2) Providers.Add("./LoginWithFIDO2", "Clé de sécurité");
            var HasRecoveryCodes = await _userManager.CountRecoveryCodesAsync(MfaUser) > 0;
            if (HasRecoveryCodes) Providers.Add("./LoginWithRecoveryCode", "Code de récupération");

            return Page();
        }
    }
}
