// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fido2NetLib;
using WebApp_IdentityProvider_MFA.Data;
using Microsoft.EntityFrameworkCore;
using WebApp_IdentityProvider_MFA.Models;

namespace WebApp_IdentityProvider_MFA.Services
{
    public class Fido2CredentialsStore : IFido2CredentialsStore
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Fido2CredentialsStore(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<Fido2Credential> GetCredentialByIdAsync(byte[] id)
        {
            return await _applicationDbContext.Fido2Credentials.Where(c => c.CredentialId.SequenceEqual(id)).FirstOrDefaultAsync();
        }

        public async Task<byte[]> GetUserHandleByCredentialIdAsync(byte[] credentialId)
        {
            return (await GetCredentialByIdAsync(credentialId))?.UserHandle;
        }

        public async Task<List<Fido2Credential>> GetCredentialsByUserHandleAsync(byte[] userHandle)
        {
            return await _applicationDbContext.Fido2Credentials.Where(c => c.UserHandle.SequenceEqual(userHandle)).ToListAsync();
        }

        public async Task<List<Fido2Credential>> GetCredentialsByUserAsync(Fido2User user)
        {
            return await GetCredentialsByUserHandleAsync(user.Id);
        }

        public async Task AddCredentialToUserAsync(Fido2User user, Fido2Credential credential)
        {
            credential.UserHandle = user.Id;
            _applicationDbContext.Fido2Credentials.Add(credential);
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task UpdateCredentialCounterAsync(byte[] credentialId, uint counter)
        {
            var cred = await GetCredentialByIdAsync(credentialId);
            if (cred != null)
            {
                cred.SignatureCounter = counter;
                await _applicationDbContext.SaveChangesAsync();
            }
        }

        public async Task RemoveUserCredentialsAsync(Fido2User user)
        {
            var items = await GetCredentialsByUserAsync(user);
            if (items != null)
            {
                foreach (var fido2Key in items)
                {
                    _applicationDbContext.Fido2Credentials.Remove(fido2Key);
                };
                await _applicationDbContext.SaveChangesAsync();
            }
        }


    }
}

