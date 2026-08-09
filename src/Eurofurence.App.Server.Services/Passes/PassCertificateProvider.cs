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
            if (passOptions.Value.AppleWwdrX509CertificatePem is { Length: > 0 } appleWwdrX509CertificatePem &&
            passOptions.Value.PassbookX509CertificatePem is { Length: > 0 } passbookX509CertificatePem &&
            passOptions.Value.PassbookX509KeyPem is { Length: > 0 } passbookX509KeyPem)
            {
                AppleWwdrCertificate = X509Certificate2.CreateFromPem(appleWwdrX509CertificatePem);
                PassbookCertificate = X509Certificate2.CreateFromPem(passbookX509CertificatePem, passbookX509KeyPem);
            }
        }

        public bool IsConfigured()
        {
            return AppleWwdrCertificate is not null && PassbookCertificate is not null;
        }
    }
}