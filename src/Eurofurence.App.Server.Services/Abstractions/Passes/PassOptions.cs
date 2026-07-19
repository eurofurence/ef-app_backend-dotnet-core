using System;

namespace Eurofurence.App.Server.Services.Abstractions.Passes
{
    public class PassOptions
    {
        public string AppleWwdrX509CertificatePem { get; init; }
        public string PassbookX509CertificatePem { get; init; }
        public string PassbookX509KeyPem { get; init; }
        public string VenueRoom { get; init; }
        public string PassTypeIdentifier { get; init; }
        public string TeamIdentifier { get; init; }
        public Guid IconImageId { get; init; }
        public Guid Icon2XImageId { get; init; }
        public Guid Icon3XImageId { get; init; }
    }
}