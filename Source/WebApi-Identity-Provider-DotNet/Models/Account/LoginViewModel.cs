// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE-IdentityServer4-Templates.md in the project root for license information.

// This file was changed for the needs of the project. These modifications are subject to 
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WebApi_Identity_Provider_DotNet.Models.Account
{
    public class LoginViewModel : LoginInputModel
    {
        public bool AllowRememberLogin { get; set; } = true;
    }
}