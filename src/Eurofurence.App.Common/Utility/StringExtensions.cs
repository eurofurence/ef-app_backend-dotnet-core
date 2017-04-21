using System;
using System.Security.Cryptography;
using System.Text;

namespace Eurofurence.App.Common.Utility
{

    public static class StringExtensions
    {
    
        public static Guid AsHashToGuid(this string input, string salt = null)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(salt + input);
            byte[] hashBytes = MD5.Create().ComputeHash(inputBytes);

            Guid hashGuid = new Guid(hashBytes);

            return hashGuid;
        }
    }
}
