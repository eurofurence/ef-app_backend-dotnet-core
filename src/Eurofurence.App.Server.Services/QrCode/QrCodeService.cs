using Eurofurence.App.Server.Services.Abstractions.QrCode;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Services.QrCode
{
    public class QrCodeService : IQrCodeService
    {
        private readonly QrCodeOptions _qrCodeOptions;

        public QrCodeService(
            IOptionsMonitor<QrCodeOptions> qrCodeOptions
        )
        {
            _qrCodeOptions = qrCodeOptions.CurrentValue;
        }
        public string GetTarget(string id)
        {
            return _qrCodeOptions.Targets[id];
        }
    }
}