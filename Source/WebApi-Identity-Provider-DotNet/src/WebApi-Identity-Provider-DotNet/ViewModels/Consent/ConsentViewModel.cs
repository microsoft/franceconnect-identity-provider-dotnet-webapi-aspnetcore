using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi_Identity_Provider_DotNet.ViewModels.Consent
{
    public class ConsentViewModel
    {
        public string ConsentId { get; set; }
        public string ClientName { get; set; }
        public IEnumerable<string> Scopes { get; set; }
    }
}
