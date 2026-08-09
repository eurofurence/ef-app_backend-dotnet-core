#nullable enable
using System.Security.Cryptography.X509Certificates;
using Eurofurence.App.Server.Services.Abstractions.Passes;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Services.Passes
{
    public class PassCertificateProvider : IPassCertificateProvider
    {
        public X509Certificate2? AppleWwdrCertificate { get; init; }
        public X509Certificate2? PassbookCertificate { get; init; }
        public PassCertificateProvider(
            IOptions<PassOptions> passOptions)
        {
            if (string.IsNullOrWhiteSpace(passOptions.Value.AppleWwdrX509CertificatePem) &&
            string.IsNullOrWhiteSpace(passOptions.Value.PassbookX509CertificatePem) &&
            string.IsNullOrWhiteSpace(passOptions.Value.PassbookX509KeyPem))
            {
                AppleWwdrCertificate = X509Certificate2.CreateFromPem(passOptions.Value.AppleWwdrX509CertificatePem);
                PassbookCertificate = X509Certificate2.CreateFromPem(passOptions.Value.PassbookX509CertificatePem, passOptions.Value.PassbookX509KeyPem);
            }
        }

        public bool IsConfigured()
        {
            return AppleWwdrCertificate is not null && PassbookCertificate is not null;
        }
    }
}