using System;
using System.Security.Cryptography;

namespace Eurofurence.App.Common.Utility
{
    public static class Hashing
    {
        public static string ComputeHashSha1(byte[] bytes)
        {
            using (var sha1 = SHA1.Create())
            {
                return Convert.ToBase64String(sha1.ComputeHash(bytes));
            }
        }
    }
}
