// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WebApp_IdentityProvider_MFA.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
