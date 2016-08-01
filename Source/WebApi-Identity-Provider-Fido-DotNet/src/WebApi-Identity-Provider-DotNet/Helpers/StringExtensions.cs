using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi_Identity_Provider_DotNet.Helpers
{
    public static class StringExtensions
    {
        public static byte[] Rfc4648Base64UrlDecode(this string url)
        {
            url = url.Replace('-', '+');
            url = url.Replace('_', '/');

            switch (url.Length % 4)
            {
                // Pad with trailing '='s
                case 0:
                    // No pad chars in this case
                    break;
                case 2:
                    // Two pad chars
                    url += "==";
                    break;
                case 3:
                    // One pad char
                    url += "=";
                    break;
                default:
                    throw new Exception("Invalid string.");
            }
            return Convert.FromBase64String(url);
        }
    }
}
