// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fido2NetLib;
using WebApp_IdentityProvider_MFA.Models;

namespace WebApp_IdentityProvider_MFA.Services
{
    public interface IFido2CredentialsStore
    {
        public Task AddCredentialToUserAsync(Fido2User user, Fido2Credential credential);
        public Task UpdateCredentialCounterAsync(byte[] credentialId, uint counter);
        public Task RemoveUserCredentialsAsync(Fido2User user);
        public Task<byte[]> GetUserHandleByCredentialIdAsync(byte[] credentialId);
        public Task<List<Fido2Credential>> GetCredentialsByUserAsync(Fido2User user);
        public Task<List<Fido2Credential>> GetCredentialsByUserHandleAsync(byte[] userHandle);
        public Task<Fido2Credential> GetCredentialByIdAsync(byte[] id);

    }
}
