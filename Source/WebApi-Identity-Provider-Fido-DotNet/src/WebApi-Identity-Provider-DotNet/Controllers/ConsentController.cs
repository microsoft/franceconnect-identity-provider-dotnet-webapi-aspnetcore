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
using Microsoft.Extensions.Logging;
using IdentityServer4.Services;
using WebApi_Identity_Provider_DotNet.ViewModels.Consent;
using IdentityServer4.Models;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApi_Identity_Provider_DotNet.Controllers
{
    public class ConsentController : Controller
    {
        private readonly ILogger<ConsentController> _logger;
        private readonly IClientStore _clientStore;
        private readonly ConsentInteraction _consentInteraction;
        private readonly IScopeStore _scopeStore;

        public ConsentController(
            ILogger<ConsentController> logger,
            ConsentInteraction consentInteraction,
            IClientStore clientStore,
            IScopeStore scopeStore)
        {
            _logger = logger;
            _consentInteraction = consentInteraction;
            _clientStore = clientStore;
            _scopeStore = scopeStore;
        }

        [HttpGet(Constants.RoutePaths.Consent, Name = "Consent")]
        public async Task<IActionResult> Index(string id)
        {
            if (id != null)
            {
                var request = await _consentInteraction.GetRequestAsync(id);
                if (request != null)
                {
                    var client = await _clientStore.FindClientByIdAsync(request.ClientId);
                    if (client != null)
                    {
                        var scopes = await _scopeStore.FindScopesAsync(request.ScopesRequested);
                        if (scopes != null && scopes.Any())
                        {
                            return View(new ConsentViewModel
                            {
                                ClientName = client.ClientName,
                                ConsentId = id,
                                Scopes = scopes.Select(s => s.Name)
                            });
                        }
                        else
                        {
                            _logger.LogError("No scopes matching: {0}", request.ScopesRequested.Aggregate((x, y) => x + ", " + y));
                        }
                    }
                    else
                    {
                        _logger.LogError("Invalid client id: {0}", request.ClientId);
                    }
                }
                else
                {
                    _logger.LogError("No consent request matching id: {0}", id);
                }
            }
            else
            {
                _logger.LogError("No id passed");
            }

            return View("Error");
        }

        [HttpPost(Constants.RoutePaths.Consent)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string button, string id)
        {
            if (id != null)
            {
                var request = await _consentInteraction.GetRequestAsync(id);
                if (request != null)
                {
                    var scopes = await _scopeStore.FindScopesAsync(request.ScopesRequested);
                    if (scopes != null && scopes.Any())
                    {
                        if (button == "no")
                        {
                            return new ConsentResult(id, ConsentResponse.Denied);
                        }
                        else if (button == "yes")
                        {
                            return new ConsentResult(id, new ConsentResponse
                            {
                                ScopesConsented = scopes.Select(s => s.Name)
                            });
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid Selection");
                        }

                        var client = await _clientStore.FindClientByIdAsync(request.ClientId);
                        if (client != null)
                        {
                            return View(new ConsentViewModel
                            {
                                ClientName = client.ClientName,
                                ConsentId = id,
                                Scopes = scopes.Select(s => s.Name)
                            });
                        }
                        else
                        {
                            _logger.LogError("Invalid client id: {0}", request.ClientId);
                        }
                    }
                    else
                    {
                        _logger.LogError("No scopes matching: {0}", request.ScopesRequested.Aggregate((x, y) => x + ", " + y));
                    }
                }
                else
                {
                    _logger.LogError("No consent request matching id: {0}", id);
                }
            }
            else
            {
                _logger.LogError("No id passed");
            }

            return View("Error");
        }

        async Task<IActionResult> BuildConsentResponse(string id, string[] scopesConsented, bool rememberConsent)
        {
            if (id != null)
            {
                var request = await _consentInteraction.GetRequestAsync(id);
            }

            return View("Error");
        }
    }
}
