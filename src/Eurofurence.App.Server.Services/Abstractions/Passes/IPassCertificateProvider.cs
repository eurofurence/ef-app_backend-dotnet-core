using System.Security.Cryptography.X509Certificates;

namespace Eurofurence.App.Server.Services.Abstractions.Passes
{
    public interface IPassCertificateProvider
    {
        public X509Certificate2 AppleWwdrCertificate { get; init; }
        public X509Certificate2 PassbookCertificate { get; init; }
        /// <summary>
        /// Check if certificates have been configured.
        /// </summary>
        /// <returns><c>false</c> if either of the certificates is missing.</returns>
        public bool IsConfigured();
    }
}
