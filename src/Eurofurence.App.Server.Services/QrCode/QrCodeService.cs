using Eurofurence.App.Server.Services.Abstractions.QrCode;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Services.Communication
{
    public class QrCodeService : IQrCodeService
    {
        private readonly QrCodeConfiguration _qrCodeConfiguration;

        public QrCodeService(
            IOptionsMonitor<QrCodeConfiguration> qrCodeConfiguration
        )
        {
            _qrCodeConfiguration = qrCodeConfiguration.CurrentValue;
        }
        public string GetTarget(string id)
        {
            return _qrCodeConfiguration.Targets[id];
        }
    }
}