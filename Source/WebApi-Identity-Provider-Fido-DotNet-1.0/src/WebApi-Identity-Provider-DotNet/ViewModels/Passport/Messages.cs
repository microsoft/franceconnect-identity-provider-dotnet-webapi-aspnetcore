using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi_Identity_Provider_DotNet.ViewModels.Passport
{
    public class RegisterMessage
    {
        public string UserId { get; set; }
        public string PublicKey { get; set; }
        public string DeviceName { get; set; }
    }

    public class UserAndKeyHintMessage
    {
        public string UserId { get; set; }
        public string PublicKeyHint { get; set; }
    }

    public class SignatureMessage
    {
        public string UserId { get; set; }
        public string Signature { get; set; }
        public string PublicKeyHint { get; set; }
        public string ClientData { get; set; }
        public string AuthnrData { get; set; }
    }

    public class ClientData
    {
        public string Challenge { get; set; }
        public string UserPrompt { get; set; }
    }
}
