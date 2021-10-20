// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WebApp_IdentityProvider_MFA.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}