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

using IdentityModel;
using IdentityServer4;
using IdentityServer4.Services;
using IdentityServer4.Services.InMemory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebApi_Identity_Provider_DotNet.Data;
using WebApi_Identity_Provider_DotNet.Helpers;
using WebApi_Identity_Provider_DotNet.Services;
using WebApi_Identity_Provider_DotNet.ViewModels.Login;
using WebApi_Identity_Provider_DotNet.ViewModels.Passport;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApi_Identity_Provider_DotNet.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInService _signInService;
        private readonly SignInInteraction _signInInteraction;
        private readonly CredentialService _credentialService;

        public LoginController(
            SignInService signInService,
            SignInInteraction signInInteraction,
            CredentialService credentialService)
        {
            _signInService = signInService;
            _signInInteraction = signInInteraction;
            _credentialService = credentialService;
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

                    if (!model.IsFidoAvailable || model.IsFidoEnable)
                    {
                        return new SignInResult(model.SignInId);
                    }
                    else
                    {
                        return Redirect("~/ui/add_credential?SignInId=" + model.SignInId + "&UserId=" + user.Subject);
                    }
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


        // FIDO

        [HttpGet("ui/add_credential")]
        public IActionResult AddCredential(AddCredentialViewModel model)
        {
            return View(model);
        }

        [HttpGet("ui/finalize_credential")]
        public IActionResult FinalizeCredential(string id)
        {
            return new SignInResult(id);
        }

        [HttpPost("ui/request_challenge")]
        public string RequestChallenge(UserAndKeyHintMessage message)
        {
            try
            {
                string challenge = Guid.NewGuid().ToString("N");

                _credentialService.SetActiveChallenge(message.UserId, message.PublicKeyHint, challenge);
                return challenge;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        [HttpPost("ui/submit_response")]
        public async Task<bool> SubmitResponse(SignatureMessage message)
        {
            bool retval = false;

            try
            {
                string challenge;
                JsonWebKey publicKey = _credentialService.GetPublicKeyForUser(message.UserId, message.PublicKeyHint, out challenge);

                var decodedClientData = message.ClientData.Rfc4648Base64UrlDecode();
                var decodedAuthnrData = message.AuthnrData.Rfc4648Base64UrlDecode();

                var clientDataJson = Encoding.UTF8.GetString(decodedClientData);
                var clientData = JsonConvert.DeserializeObject<ClientData>(clientDataJson);
                if (clientData.Challenge != challenge) return false;
                
                var sha256 = SHA256.Create();
                var hashedClientData = sha256.ComputeHash(decodedClientData);
                var buffer = new byte[decodedAuthnrData.Length + hashedClientData.Length];
                decodedAuthnrData.CopyTo(buffer, 0);
                hashedClientData.CopyTo(buffer, decodedAuthnrData.Length);

                var publicKeyInfo = new RSAParameters();
                publicKeyInfo.Modulus = publicKey.N.Rfc4648Base64UrlDecode();
                publicKeyInfo.Exponent = publicKey.E.Rfc4648Base64UrlDecode();
                var rsa = new RSACng();
                rsa.ImportParameters(publicKeyInfo);

                byte[] signature = message.Signature.Rfc4648Base64UrlDecode();
                retval = rsa.VerifyData(buffer, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                if (retval)
                {
                    var user = _signInService.FindBySubject(message.UserId);
                    await IssueCookie(user, "idsvr", "fido");
                }
            }
            catch (Exception)
            { }
            
            return retval;
        }

        [HttpPost("ui/register_credential")]
        public async Task<bool> Register(RegisterMessage message)
        {
            bool retval = false;

            var info = await HttpContext.Authentication.GetAuthenticateInfoAsync(Constants.PrimaryAuthenticationType);
            var sub = info.Principal.Claims.Where(x => x.Type == JwtClaimTypes.Subject).Select(x => x.Value).FirstOrDefault();
            var user = _signInService.FindBySubject(sub);
            if (user == null || message.PublicKey == null)
            {
                return false;
            }

            try
            {
                SHA256 hashalg = SHA256.Create();
                byte[] publicKeyHashBuffer = hashalg.ComputeHash(Encoding.UTF8.GetBytes(message.PublicKey));
                string publicKeyHash = Convert.ToBase64String(publicKeyHashBuffer);

                retval = _credentialService.RegisterCredential(message.UserId, message.PublicKey, publicKeyHash, message.DeviceName);
            }
            catch (Exception)
            { }

            return retval;
        }

        [HttpPost("ui/remove_credential")]
        public bool RemoveRegisteredKey(UserAndKeyHintMessage message)
        {
            bool retval = false;
            try
            {
                retval = _credentialService.RemoveCredential(message.UserId, message.PublicKeyHint);
            }
            catch (Exception)
            { }

            return retval;
        }
    }
}
