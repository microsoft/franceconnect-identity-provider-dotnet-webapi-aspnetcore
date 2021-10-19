// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;
using static IdentityServer4.IdentityServerConstants;

namespace WebApp_IdentityProvider_MFA
{
    public class IdentityInMemoryConfiguration
    {
        public string FranceConnectId { get; set; }
        public string FranceConnectSecret { get; set; }
        public string FranceConnectRedirectUri { get; set; }

        public IdentityInMemoryConfiguration(string franceConnectId, string franceConnectSecret, string franceConnectRedirectUri)
        {
            FranceConnectId = franceConnectId;
            FranceConnectSecret = franceConnectSecret;
            FranceConnectRedirectUri = franceConnectRedirectUri;
        }

        public IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResources.Address(),
                new IdentityResources.Phone(),
                new IdentityResource(
                    "birth",
                    "Birth",
                    new [] { "birthplace", "birthcountry" }
                ),
                new IdentityResource(
                    "preferred_username",
                    "Preferred username",
                    new [] { "preferred_username" })

            };

        public IEnumerable<Client> Clients =>
    new List<Client>
    {
        new Client
        {
                    ClientName = "FranceConnect",
                    ClientId = FranceConnectId,

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = false,
                    RedirectUris = new List<string>
                    {
                        FranceConnectRedirectUri
                    },
                    ClientSecrets = new List<Secret>
                    {
                        new Secret(FranceConnectSecret.Sha256())
                    },
                    AllowedIdentityTokenSigningAlgorithms=new string[]{"HS256"},
                    AllowedScopes = new List<string>
                    {
                        StandardScopes.OpenId,
                        StandardScopes.Profile,
                        StandardScopes.Email,
                        StandardScopes.Address,
                        StandardScopes.Phone,
                        "birth",
                        "preferred_username"
                    }
        }
    };
    }
}