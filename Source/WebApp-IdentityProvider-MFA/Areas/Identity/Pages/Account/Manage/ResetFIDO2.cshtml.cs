// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp_IdentityProvider_MFA.Data;
using WebApp_IdentityProvider_MFA.Services;

namespace WebApp_IdentityProvider_MFA.Areas.Identity.Pages.Account.Manage
{
    public class ResetFIDO2Model : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly FIDO2TwoFactorProvider _fido2TwoFactorProvider;
        private readonly ILogger<ResetFIDO2Model> _logger;

        public ResetFIDO2Model(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            FIDO2TwoFactorProvider fido2TwoFactorProvider,
            ILogger<ResetFIDO2Model> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _fido2TwoFactorProvider = fido2TwoFactorProvider;
            _logger = logger;
        }


        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            
            var validTwoFactorProviders = await _userManager.GetValidTwoFactorProvidersAsync(user);
            var HasAuthenticator = validTwoFactorProviders.Any(provider => provider == _userManager.Options.Tokens.AuthenticatorTokenProvider);
            if (!HasAuthenticator)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, false);
            }
            await _fido2TwoFactorProvider.RemoveCredentialsAsync(user);

            var userId = await _userManager.GetUserIdAsync(user);
            _logger.LogInformation("User with ID '{UserId}' has reset their security keys.", userId);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Vos clés de scurité ont été réinitialisées, vous devrez en ajouter de nouvelles.";

            return RedirectToPage("./EnableFIDO2");
        }
    }
}
