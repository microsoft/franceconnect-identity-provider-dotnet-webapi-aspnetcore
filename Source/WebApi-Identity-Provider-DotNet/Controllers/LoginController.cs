//
// The MIT License (MIT)
// Copyright (c) 2016 Microsoft France
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// You may obtain a copy of the License at https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IdentityServer4;
using WebApi_Identity_Provider_DotNet.ViewModels.Login;
using WebApi_Identity_Provider_DotNet.Services;
using IdentityModel;
using System.Security.Claims;
using IdentityServer4.Services.InMemory;
using IdentityServer4.Services;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApi_Identity_Provider_DotNet.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInService _signInService;
        private readonly SignInInteraction _signInInteraction;

        public LoginController(
            SignInService signInService,
            SignInInteraction signInInteraction)
        {
            _signInService = signInService;
            _signInInteraction = signInInteraction;
        }

        [HttpGet(Constants.RoutePaths.Login, Name = "Login")]
        public async Task<IActionResult> Index(string id)
        {
            var vm = new LoginViewModel();

            if (!string.IsNullOrEmpty(id))
            {
                var request = await _signInInteraction.GetRequestAsync(id);
                if (request != null)
                {
                    vm.Username = request.LoginHint;
                    vm.SignInId = id;
                }
            }

            return View(vm);
        }

        [HttpPost(Constants.RoutePaths.Login)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_signInService.ValidateCredentials(model.Username, model.Password))
                {
                    var user = _signInService.FindByUsername(model.Username);
                    await IssueCookie(user, "idsvr", "password");

                    if (model.SignInId != null)
                    {
                        return new SignInResult(model.SignInId);
                    }

                    return Redirect("~/");
                }

                ModelState.AddModelError("", "Invalid username or password.");
            }

            return View(model);
        }

        private async Task IssueCookie(InMemoryUser user, string identityProvider, string authenticationType)
        {
            var name = user.Claims.Where(x => x.Type == JwtClaimTypes.Name).Select(x => x.Value).FirstOrDefault() ?? user.Username;

            var claims = new Claim[]
            {
                new Claim(JwtClaimTypes.Subject, user.Subject),
                new Claim(JwtClaimTypes.Name, name),
                new Claim(JwtClaimTypes.IdentityProvider, identityProvider),
                new Claim(JwtClaimTypes.AuthenticationTime, DateTime.UtcNow.ToEpochTime().ToString()),
            };
            var claimIdentity = new ClaimsIdentity(claims, authenticationType, JwtClaimTypes.Name, JwtClaimTypes.Role);
            var claimPricipal = new ClaimsPrincipal(claimIdentity);

            await HttpContext.Authentication.SignInAsync(Constants.PrimaryAuthenticationType, claimPricipal);
        }
    }
}
