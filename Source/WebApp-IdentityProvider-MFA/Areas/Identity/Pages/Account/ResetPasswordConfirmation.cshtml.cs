// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp_IdentityProvider_MFA.Areas.Identity.Pages.Account
{

    [AllowAnonymous]
    public class ResetPasswordConfirmationModel : PageModel
    {

        public void OnGet()
        {
        }
    }
}
