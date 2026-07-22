using System.Security.Cryptography.X509Certificates;
using Eurofurence.App.Server.Services.Abstractions.Passes;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Services.Passes
{
    public class PassCertificateProvider : IPassCertificateProvider
    {
        public X509Certificate2 AppleWwdrCertificate { get; init; }
        public X509Certificate2 PassbookCertificate { get; init; }
        public PassCertificateProvider(
            IOptions<PassOptions> passOptions)
        {
            AppleWwdrCertificate = X509Certificate2.CreateFromPem(passOptions.Value.AppleWwdrX509CertificatePem);
            PassbookCertificate = X509Certificate2.CreateFromPem(passOptions.Value.PassbookX509CertificatePem, passOptions.Value.PassbookX509KeyPem);
        }
    }
}