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

using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi_Identity_Provider_DotNet.Configuration
{
    public class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new List<Scope>
            {
                StandardScopes.OpenId,
                StandardScopes.Profile,
                StandardScopes.Email,
                StandardScopes.Address,
                StandardScopes.Phone,
                new Scope
                {
                    Name = "birth",
                    DisplayName = "Birth",
                    Description = "Birth",
                    Type = ScopeType.Identity,
                    Claims = new List<ScopeClaim> { new ScopeClaim("birthplace"), new ScopeClaim("birthcountry") }
                },
                new Scope
                {
                    Name = "preferred_username",
                    DisplayName = "Preferred username",
                    Description = "Preferred username",
                    Type = ScopeType.Identity,
                    Claims = new List<ScopeClaim> { new ScopeClaim("preferred_username") }
                }
            };
        }
    }
}
