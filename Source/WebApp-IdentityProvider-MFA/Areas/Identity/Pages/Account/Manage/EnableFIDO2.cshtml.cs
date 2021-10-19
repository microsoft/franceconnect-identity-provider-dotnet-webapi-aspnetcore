// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fido2NetLib;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebApp_IdentityProvider_MFA.Data;
using WebApp_IdentityProvider_MFA.Services;

namespace WebApp_IdentityProvider_MFA.Areas.Identity.Pages.Account.Manage
{
    public class EnableFIDO2Model : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FIDO2TwoFactorProvider _fido2TwoFactorProvider;
        private readonly ILogger<EnableFIDO2Model> _logger;

        public EnableFIDO2Model(
            UserManager<ApplicationUser> userManager,
            ILogger<EnableFIDO2Model> logger,
            FIDO2TwoFactorProvider fido2TwoFactorProvider)
        {
            _fido2TwoFactorProvider = fido2TwoFactorProvider;
            _userManager = userManager;
            _logger = logger;
        }


        [TempData]
        public string StatusMessage { get; set; }

        [TempData]
        public string[] RecoveryCodes { get; set; }


        [ReadOnly(true)]
        [HiddenInput(DisplayValue = false)]
        [BindProperty]
        public string CredentialRegistrationOptions { get; set; }
        
        [BindProperty]
        public InputModel Input { get; set; }


        public class InputModel
        {
            [Required(ErrorMessage = "Insertion de la clé de sécurité en attente. Suivez les instructions qui s'affichent dans votre navigateur.")]
            [DataType(DataType.Text)]
            [Display(Name = "Clé de sécurité")]
            public string AttestationRawResponse { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            CredentialRegistrationOptions = (await  _fido2TwoFactorProvider.BuildCredentialRegistrationOptionsAsync(user)).ToJson();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            if (!ModelState.IsValid)
            {
                return Page();
            }

            bool result = await _fido2TwoFactorProvider.RegisterCredentialAsync(CredentialCreateOptions.FromJson(CredentialRegistrationOptions), JsonConvert.DeserializeObject<AuthenticatorAttestationRawResponse>(Input.AttestationRawResponse), user);
            
            if (!result)
            {
                ModelState.AddModelError(String.Empty,"L'ajout de la clé de sécurité a échoué.");
                return Page();
            }


            await _userManager.SetTwoFactorEnabledAsync(user, true);
            var userId = await _userManager.GetUserIdAsync(user);
            _logger.LogInformation("User with ID '{UserId}' has enabled 2FA with a security key.", userId);

            StatusMessage = "Votre clé de sécurité a été validée.";

            if (await _userManager.CountRecoveryCodesAsync(user) == 0)
            {
                var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                RecoveryCodes = recoveryCodes.ToArray();
                return RedirectToPage("./ShowRecoveryCodes");
            }
            else
            {
                return RedirectToPage("./TwoFactorAuthentication");
            }
        }
    }
}
