using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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

        public static string ComputeHashSha1(string text)
        {
            return ComputeHashSha1(Encoding.UTF8.GetBytes(text));
        }

        public static string ComputeHashSha1(params object[] items)
        {
            var hashString = string.Join(";", items.Select(_ => _.ToString()));
            return ComputeHashSha1(Encoding.UTF8.GetBytes(hashString));
        }
    }
}
