using IdentityModel;
using IdentityServer4.Core.Services.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApi_Identity_Provider_DotNet.Configuration
{
    public class Users
    {
        public static List<InMemoryUser> Get()
        {
            return new List<InMemoryUser>
            {
                new InMemoryUser
                {
                    Subject = "6867085672678036750625",
                    Username = "jean",
                    Password = "password",
                    Claims = new Claim[]
                    {
                        new Claim(JwtClaimTypes.GivenName, "Jean"),
                        new Claim(JwtClaimTypes.FamilyName, "Dupont"),
                        new Claim(JwtClaimTypes.Email, "jean.dupont@contoso.com"),
                        new Claim(JwtClaimTypes.BirthDate, "1993-06-16"),
                        new Claim(JwtClaimTypes.Gender, "male"),
                        new Claim("birthplace", "75056"),
                        new Claim("birthcountry", "99100"),
                        new Claim(JwtClaimTypes.PreferredUserName, "Jean Dupont")
                    }
                }
            };
        }
    }
}
