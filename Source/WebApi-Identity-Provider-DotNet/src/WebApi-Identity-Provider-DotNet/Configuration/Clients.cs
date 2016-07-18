using IdentityServer4.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi_Identity_Provider_DotNet.Configuration
{
    public class Clients
    {
        public const string FranceConnectId = "9f2334a7f81648d3ac850c774450c420";
        public const string FranceConnectSecret = "cef97158682942fa98033c6dc90d4984";

        public static IEnumerable<Client> Get()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = FranceConnectId,
                    ClientName = "FranceConnect",
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = new List<string>
                    {
                        "https://fcp.integ01.dev-franceconnect.fr/oidc_callback"
                    },
                    ClientSecrets = new List<Secret>
                    {
                        new Secret(FranceConnectSecret.Sha256())
                    },
                    AllowedScopes = new List<string>
                    {
                        StandardScopes.OpenId.Name,
                        StandardScopes.Profile.Name,
                        StandardScopes.Email.Name,
                        StandardScopes.Address.Name,
                        StandardScopes.Phone.Name,
                        "birth",
                        "preferred_username"
                    }
                }
            };
        }
    }
}
