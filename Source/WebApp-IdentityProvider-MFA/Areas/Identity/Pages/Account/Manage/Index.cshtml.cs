// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp_IdentityProvider_MFA.Data;
using WebApp_IdentityProvider_MFA.Models;

namespace WebApp_IdentityProvider_MFA.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }


        public class InputModel
        {
            [Required]
            [Display(Name = "Genre")]
            [GenderValidation]
            public string Gender { get; set; }


            [Required]
            [Display(Name = "Prénom")]
            public string GivenName { get; set; }

            [Required]
            [Display(Name = "Nom")]
            public string FamilyName { get; set; }

            [Display(Name = "Nom d'usage")]
            public string PreferredName { get; set; }

            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
            [Display(Name = "Date de naissance")]
            public DateTime BirthDate { get; set; }
            [Required]
            [StringLength(6, MinimumLength = 5)]
            [Display(Name = "Lieu de naissance au format INSEE a 5 chiffres")]
            public string BirthPlace { get; set; }

            [Required]
            [StringLength(6, MinimumLength = 5)]
            [Display(Name = "Pays de naissance au format INSEE a 5 chiffres")]
            public string BirthCountry { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Input = new InputModel
            {
                FamilyName = user.FamilyName,
                BirthDate = user.BirthDate,
                BirthCountry = user.BirthCountry,
                BirthPlace = user.BirthPlace,
                GivenName = user.GivenName,
                Gender = user.Gender,
                PreferredName = user.PreferredName,
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
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
                await LoadAsync(user);
                return Page();
            }

            {
                user.PreferredName = Input.PreferredName;
                user.GivenName = Input.GivenName;
                user.Gender = Input.Gender;
                user.FamilyName = Input.FamilyName;
                user.BirthDate = Input.BirthDate;
            }
            var userUpdateResult = await _userManager.UpdateAsync(user);
            if (!userUpdateResult.Succeeded)
            {
                StatusMessage = "Erreur lors de la mise à jour du profil.";
                return RedirectToPage();
            }
            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                StatusMessage = "Erreur lors de la mise à jour du profil.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Votre profil a été mis à jour.";
            return RedirectToPage();
        }
    }
}
