// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE-IdentityServer4-Templates.md in the project root for license information.

// This file was changed for the needs of the project. These modifications are subject to 
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IdentityServer4.Services;
using WebApi_Identity_Provider_DotNet.Models.Error;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApi_Identity_Provider_DotNet.Controllers
{
    public class ErrorController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IWebHostEnvironment _environment;

        public ErrorController(IIdentityServerInteractionService interaction, IWebHostEnvironment environment)
        {
            _interaction = interaction;
            _environment = environment;
        }


        /// <summary>
        /// Shows the error page
        /// </summary>
        public async Task<IActionResult> Error(string errorId)
        {
            var vm = new ErrorViewModel();

            // retrieve error details from identityserver
            var message = await _interaction.GetErrorContextAsync(errorId);
            if (message != null)
            {
                vm.Error = message;

                if (!_environment.IsDevelopment())
                {
                    // only show in development
                    message.ErrorDescription = null;
                }
            }

            return View("Error", vm);
        }
    }
}
