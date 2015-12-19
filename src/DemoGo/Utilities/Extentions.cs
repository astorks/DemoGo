using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DemoGo
{
    public static class Extentions
    {
        public static string GetBase64String(this RandomNumberGenerator rng, int length)
        {
            var randomData = new byte[length];
            rng.GetBytes(randomData);
            return Convert.ToBase64String(randomData);
        }
    }
}
