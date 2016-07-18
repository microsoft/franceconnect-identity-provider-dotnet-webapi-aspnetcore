using IdentityServer4.Core.Models;
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
