﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE-IdentityServer4-Templates.md in the project root for license information.
// This file was changed for the needs of the projects. These modifications are subject to 
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using IdentityServer4.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using IdentityModel;
using Newtonsoft.Json.Linq;
using WebApi_Identity_Provider_DotNet.Configuration;
using static IdentityServer4.IdentityServerConstants;

namespace WebApi_Identity_Provider_DotNet.Jwt
{
    public class FranceConnectTokenCreationService : ITokenCreationService
    {

        private readonly IdentityInMemoryConfiguration _identityConfig;

        public FranceConnectTokenCreationService(IdentityInMemoryConfiguration identityConfig)
        {
            _identityConfig = identityConfig;
        }

        public virtual async Task<string> CreateTokenAsync(Token token)
        {
            var header = await CreateHeaderAsync(token);
            var payload = await CreatePayloadAsync(token);

            return await SignAsync(new JwtSecurityToken(header, payload));
        }

        protected virtual Task<JwtHeader> CreateHeaderAsync(Token token)
        {
            JwtHeader header = new JwtHeader(new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_identityConfig.FranceConnectSecret)), "HS256"));

            return Task.FromResult(header);
        }
        
        protected virtual Task<JwtPayload> CreatePayloadAsync(Token token)
        {
            var payload = new JwtPayload(
                token.Issuer,
                token.Audiences.FirstOrDefault(),
                null,
                DateTime.UtcNow,
                DateTime.UtcNow.AddSeconds(token.Lifetime));

            var amrClaims = token.Claims.Where(x => x.Type == JwtClaimTypes.AuthenticationMethod);
            var jsonClaims = token.Claims.Where(x => x.ValueType == ClaimValueTypes.Json);
            var normalClaims = token.Claims.Except(amrClaims).Except(jsonClaims);

            payload.AddClaims(normalClaims);

            // deal with amr
            var amrValues = amrClaims.Select(x => x.Value).Distinct().ToArray();
            if (amrValues.Any())
            {
                payload.Add(JwtClaimTypes.AuthenticationMethod, amrValues);
            }

            // deal with json types
            // calling ToArray() to trigger JSON parsing once and so later 
            // collection identity comparisons work for the anonymous type
            var jsonTokens = jsonClaims.Select(x => new { x.Type, JsonValue = JRaw.Parse(x.Value) }).ToArray();

            var jsonObjects = jsonTokens.Where(x => x.JsonValue.Type == JTokenType.Object).ToArray();
            var jsonObjectGroups = jsonObjects.GroupBy(x => x.Type).ToArray();
            foreach (var group in jsonObjectGroups)
            {
                if (payload.ContainsKey(group.Key))
                {
                    throw new Exception(String.Format("Can't add two claims where one is a JSON object and the other is not a JSON object ({0})", group.Key));
                }

                if (group.Skip(1).Any())
                {
                    // add as array
                    payload.Add(group.Key, group.Select(x => x.JsonValue).ToArray());
                }
                else
                {
                    // add just one
                    payload.Add(group.Key, group.First().JsonValue);
                }
            }

            var jsonArrays = jsonTokens.Where(x => x.JsonValue.Type == JTokenType.Array).ToArray();
            var jsonArrayGroups = jsonArrays.GroupBy(x => x.Type).ToArray();
            foreach (var group in jsonArrayGroups)
            {
                if (payload.ContainsKey(group.Key))
                {
                    throw new Exception(String.Format("Can't add two claims where one is a JSON array and the other is not a JSON array ({0})", group.Key));
                }

                List<JToken> newArr = new List<JToken>();
                foreach (var arrays in group)
                {
                    var arr = (JArray)arrays.JsonValue;
                    newArr.AddRange(arr);
                }

                // add just one array for the group/key/claim type
                payload.Add(group.Key, newArr.ToArray());
            }

            var unsupportedJsonTokens = jsonTokens.Except(jsonObjects).Except(jsonArrays);
            var unsupportedJsonClaimTypes = unsupportedJsonTokens.Select(x => x.Type).Distinct();
            if (unsupportedJsonClaimTypes.Any())
            {
                throw new Exception(String.Format("Unsupported JSON type for claim types: {0}", unsupportedJsonClaimTypes.Aggregate((x, y) => x + ", " + y)));
            }

            return Task.FromResult(payload);
        }
        
        protected virtual Task<string> SignAsync(JwtSecurityToken jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            return Task.FromResult(handler.WriteToken(jwt));
        }
    }
}
