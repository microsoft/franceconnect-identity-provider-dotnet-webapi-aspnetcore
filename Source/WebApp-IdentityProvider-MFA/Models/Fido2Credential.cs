// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp_IdentityProvider_MFA.Models
{
    public class Fido2Credential
    {
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
            public byte[] UserHandle { get; set; }
            public byte[] PublicKey { get; set; }
            public uint SignatureCounter { get; set; }
            public byte[] CredentialId { get; set; }
        
    }
}
