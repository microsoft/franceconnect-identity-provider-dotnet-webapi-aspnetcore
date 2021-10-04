// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;
using WebApi_Identity_Provider_DotNet.Configuration;

namespace WebApi_Identity_Provider_DotNet.Jwt
{
    public class FranceConnectTokenCreationService : DefaultTokenCreationService
    {
        private readonly IdentityInMemoryConfiguration _identityConfig;

        public FranceConnectTokenCreationService(ISystemClock clock, IKeyMaterialService keys, IdentityServerOptions options, ILogger<DefaultTokenCreationService> logger,
            IdentityInMemoryConfiguration identityConfig) : base(clock,keys,options,logger)
        {
            _identityConfig = identityConfig;  
            
        }

        protected override async Task<JwtHeader> CreateHeaderAsync(Token token)
        {
            JwtHeader header = new JwtHeader(new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_identityConfig.FranceConnectSecret)), "HS256"));
            return await Task.FromResult(header);
        }
    }
}
