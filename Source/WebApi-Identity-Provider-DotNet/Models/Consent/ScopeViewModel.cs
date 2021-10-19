// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE-IdentityServer4-Templates.md in the project root for license information.

// This file was changed for the needs of the project. These modifications are subject to 
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WebApi_Identity_Provider_DotNet.Models.Consent 
{
    public class ScopeViewModel
    {
        public string Value { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool Emphasize { get; set; }
        public bool Required { get; set; }
        public bool Checked { get; set; }
    }
}
