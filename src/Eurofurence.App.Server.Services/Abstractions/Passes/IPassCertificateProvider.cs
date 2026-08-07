using System.Security.Cryptography.X509Certificates;

namespace Eurofurence.App.Server.Services.Abstractions.Passes
{
    public interface IPassCertificateProvider
    {
        public X509Certificate2 AppleWwdrCertificate { get; init; }
        public X509Certificate2 PassbookCertificate { get; init; }
    }
}
