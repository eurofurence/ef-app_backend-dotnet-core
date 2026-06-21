using System;

namespace Eurofurence.App.Server.Web.Extensions
{
    public static class StringCheckIsValidHttpsUrl
    {
        /// <summary>
        /// Use System.Uri.TryCreate to check if the string is a valid, absolute HTTPS URL.
        /// </summary>
        public static bool CheckIsValidHttpsUrl(this string str)
        {
            return Uri.TryCreate(str, UriKind.Absolute, out Uri apiUri) &&
                        apiUri.Scheme == Uri.UriSchemeHttps;
        }
    }
}