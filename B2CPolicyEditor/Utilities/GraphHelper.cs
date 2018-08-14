using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace B2CPolicyEditor.Utilities
{
    public class GraphHelper
    {
        public static string CreateSecret(int length)
        {
            var secretKey = new Byte[length];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(secretKey);
                var key = Convert.ToBase64String(secretKey, 0, secretKey.Length);
                return key;
            }
        }
    }
}
