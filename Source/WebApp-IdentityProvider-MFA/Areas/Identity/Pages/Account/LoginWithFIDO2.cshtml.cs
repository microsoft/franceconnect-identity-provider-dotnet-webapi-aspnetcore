// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using WebApp_IdentityProvider_MFA.Data;
using WebApp_IdentityProvider_MFA.Services;
using Fido2NetLib;
using System.Text;
using Fido2NetLib.Objects;
using System.ComponentModel;
using System.Text.Json;

namespace WebApp_IdentityProvider_MFA.Areas.Identity.Pages.Account
{
    public class LoginWithFIDO2Model : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginWithFIDO2Model> _logger;

        public LoginWithFIDO2Model(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginWithFIDO2Model> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }


        [BindProperty]
        public InputModel Input { get; set; }

        [HiddenInput(DisplayValue = false)]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

        [ReadOnly(true)]
        [HiddenInput(DisplayValue = false)]
        [BindProperty]
        public string CredentialAssertionOptions { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Insertion de la clé de sécurité en attente. Suivez les instructions qui s'affichent dans votre navigateur.")]
            [DataType(DataType.Text)]
            [HiddenInput(DisplayValue = false)]
            [Display(Name = "Clé de sécurité")]
            public string AssertedResponse { get; set; }

            [Display(Name = "Ne plus demander d'authentification à deux facteurs sur cet appareil")]
            public bool RememberMachine { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }
            CredentialAssertionOptions = await _userManager.GenerateTwoFactorTokenAsync(user, FIDO2TwoFactorProvider.Constants.ProviderName);
            ReturnUrl = returnUrl;
            RememberMe = rememberMe;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            returnUrl ??= Url.Content("~/");

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var result = await _signInManager.TwoFactorSignInAsync(FIDO2TwoFactorProvider.Constants.ProviderName, Input.AssertedResponse, rememberMe, Input.RememberMachine);
            var userId = await _userManager.GetUserIdAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", userId);
                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", userId);
                return RedirectToPage("./Lockout");
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", userId);
                ModelState.AddModelError(string.Empty, "Impossible de valider la clé de sécurité.");
                return Page();
            }
        }
    }
}
