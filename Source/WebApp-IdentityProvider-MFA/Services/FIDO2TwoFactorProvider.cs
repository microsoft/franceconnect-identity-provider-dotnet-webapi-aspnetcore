// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Text;
using Fido2NetLib.Objects;
using Fido2NetLib;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using WebApp_IdentityProvider_MFA.Data;
using WebApp_IdentityProvider_MFA.Models;

namespace WebApp_IdentityProvider_MFA.Services
{

    public class FIDO2TwoFactorProvider : IUserTwoFactorTokenProvider<ApplicationUser>
    {
        private readonly IFido2 _fido2;
        private readonly IFido2CredentialsStore _fido2credentialsStore;

        public FIDO2TwoFactorProvider(IFido2 fido2, IFido2CredentialsStore credentialsStore)
        {
            _fido2 = fido2;
            _fido2credentialsStore = credentialsStore;
        }
        /// <summary>
        /// Checks if a two-factor authentication token can be generated for the specified <paramref name="user"/>.
        /// </summary>
        /// <param name="manager">The <see cref="UserManager{TUser}"/> to retrieve the <paramref name="user"/> from.</param>
        /// <param name="user">The <typeparamref name="TUser"/> to check for the possibility of generating a two-factor authentication token.</param>
        /// <returns>True if the user has an authenticator key set, otherwise false.</returns>
        public async Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            var items = await _fido2credentialsStore.GetCredentialsByUserAsync(new Fido2ApplicationUser(user));
            return items.Count > 0;
        }

        public async Task<string> GenerateAsync(string purpose, UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            //TODO purpose check ? 

            AssertionOptions validationOptions = await BuildValidationOptionsAsync(user);
            if (validationOptions == null) throw new Exception("Error creating credential assertion options");

            string jsonOptions = validationOptions.ToJson();
            //Store validationOptions in TokenStorage temporarily
            var saveOptionsResult = await manager.SetAuthenticationTokenAsync(user, Constants.ProviderName, Constants.AssertionOptionsKeyName, jsonOptions);
            if (saveOptionsResult != IdentityResult.Success) throw new Exception("Error saving a credential assertion");

            return jsonOptions;
        }

        public async Task<bool> ValidateAsync(string purpose, string token, UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            //TODO Purpose check ?
            //Get Validation Options from Token storage and remove it
            var jsonAssertionOptions = await manager.GetAuthenticationTokenAsync(user, Constants.ProviderName, Constants.AssertionOptionsKeyName);


            AssertionOptions validationOptions = AssertionOptions.FromJson(jsonAssertionOptions);
            if (validationOptions == null)
            {
                throw new Exception("Error loading credential assertion options");
            }
            AuthenticatorAssertionRawResponse clientRawResponse = JsonConvert.DeserializeObject<AuthenticatorAssertionRawResponse>(token);
            if (clientRawResponse == null)
            {
                throw new Exception("Error loading credential assertion response");
            }

            var result = await ValidateCredentialAssertionResponseAsync(validationOptions, clientRawResponse, user);
            //Remove the 2FA request data from the storage.
            await manager.RemoveAuthenticationTokenAsync(user, Constants.ProviderName, Constants.AssertionOptionsKeyName);

            return result.Status == "ok";
        }

        public async Task RemoveCredentialsAsync(ApplicationUser user)
        {
            await _fido2credentialsStore.RemoveUserCredentialsAsync(new Fido2ApplicationUser(user));
        }


        private async Task<AssertionOptions> BuildValidationOptionsAsync(ApplicationUser user)
        {
            try
            {
                List<PublicKeyCredentialDescriptor> existingCredentials;
                existingCredentials = (await _fido2credentialsStore.GetCredentialsByUserAsync(new Fido2ApplicationUser(user))).Select(c => new PublicKeyCredentialDescriptor(c.CredentialId)).ToList();

                var extensions = new AuthenticationExtensionsClientInputs()
                {
                    UserVerificationIndex = true,
                    Location = true,
                    UserVerificationMethod = true
                };

                AssertionOptions options = _fido2.GetAssertionOptions(
                    existingCredentials,
                    UserVerificationRequirement.Discouraged, // User verification is not recommended for 2FA because the user will have already entered his password
                    extensions
                );

                return options;
            }
            catch (Exception e)
            {
                //TODO
                return null;
            }
        }

        private async Task<AssertionVerificationResult> ValidateCredentialAssertionResponseAsync(AssertionOptions assertionOptions, AuthenticatorAssertionRawResponse clientResponse, ApplicationUser user)
        {
            try
            {
                //TODO Check user vs requestloginuser
                var cred = await _fido2credentialsStore.GetCredentialByIdAsync(clientResponse.Id);
                if (cred == null) throw new Exception("Unknown credentials");
                var storedCounter = cred.SignatureCounter;

                var res = await _fido2.MakeAssertionAsync(clientResponse, assertionOptions, cred.PublicKey, storedCounter, async (IsUserHandleOwnerOfCredentialIdParams args) => cred.UserHandle == args.UserHandle);

                await _fido2credentialsStore.UpdateCredentialCounterAsync(res.CredentialId, res.Counter);
                //We request to update the counter and return directly afterwards
                return res;
            }
            catch (Exception e)
            {
                return new AssertionVerificationResult { Status = "error", ErrorMessage = e.Message ?? e.ToString() };
            }
        }

        public async Task<CredentialCreateOptions> BuildCredentialRegistrationOptionsAsync(ApplicationUser user)
        {
            try
            {
                var fidoUser = new Fido2ApplicationUser(user);

                var items = await _fido2credentialsStore.GetCredentialsByUserAsync(fidoUser);

                List<PublicKeyCredentialDescriptor> existingCredentials = (await _fido2credentialsStore.GetCredentialsByUserAsync(fidoUser)).Select(c => new PublicKeyCredentialDescriptor(c.CredentialId)).ToList();

                var authenticatorSelection = new AuthenticatorSelection
                {
                    RequireResidentKey = false,
                    UserVerification = UserVerificationRequirement.Preferred
                };
                var extensions = new AuthenticationExtensionsClientInputs
                {
                    Extensions = true,
                    Location = true,
                    UserVerificationIndex = true,
                    UserVerificationMethod = true,
                    BiometricAuthenticatorPerformanceBounds = new AuthenticatorBiometricPerfBounds { FAR = float.MaxValue, FRR = float.MaxValue }
                };

                var options = _fido2.RequestNewCredential(fidoUser, existingCredentials, authenticatorSelection, AttestationConveyancePreference.None, extensions);

                return options;
            }
            catch (Exception e)
            {/*TODO*/
                return null;
            }
        }
        public async Task<bool> RegisterCredentialAsync(CredentialCreateOptions createOptions, AuthenticatorAttestationRawResponse attestationResponse, ApplicationUser user)
        {
            try
            {
                //TODO Check user vs requestloginuser

                var creationResult = await _fido2.MakeNewCredentialAsync(attestationResponse, createOptions, async (IsCredentialIdUniqueToUserParams args) => true);
                //A credential is mapped to a userId, which is unique, so we always know that any credential is unique to a user 

                if (creationResult != null && creationResult.Status == "ok")
                {
                    await _fido2credentialsStore.AddCredentialToUserAsync(createOptions.User, new Fido2Credential
                    {
                        CredentialId = creationResult.Result.CredentialId,
                        PublicKey = creationResult.Result.PublicKey,
                        UserHandle = creationResult.Result.User.Id,
                        SignatureCounter = creationResult.Result.Counter
                    });

                    return true;
                }
            }
            catch (Exception e)
            {
                /*TODO*/
            }
            return false;
        }


        public static class Constants
        {
            public const string ProviderName = "IdentityProvider.MFA.FIDO";
            public const string AssertionOptionsKeyName = "assertionOptions";
            public const string CredentialRegistrationOptionsKeyName = "attestationOptions";
            public const string CredentialValidationPurpose = "assertionPurpose";
            public const string CredentialRegistrationPurpose = "attestationPurpose";
        }

        private class Fido2ApplicationUser : Fido2User
        {
            public Fido2ApplicationUser(ApplicationUser user)
            {
                Id = Encoding.UTF8.GetBytes(user.Id);
                Name = user.UserName;
                DisplayName = user.GivenName;
            }
        }
    }
}
