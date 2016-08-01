using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi_Identity_Provider_DotNet.Models
{
    public class Credential
    {
        public string UserId { get; set; }
        public string PublicKey { get; set; }
        public string PublicKeyHash { get; set; }
        public string DeviceName { get; set; }
        public string ActiveChallenge { get; set; }
    }
}
