namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public class ApnsOptions
    {
        public bool IsConfigured => !string.IsNullOrWhiteSpace(BundleId) && !string.IsNullOrWhiteSpace(CertContent) && !string.IsNullOrWhiteSpace(KeyId) && !string.IsNullOrWhiteSpace(TeamId);
        public string BundleId { get; init; }
        public string CertContent { get; init; }
        public string KeyId { get; init; }
        public string TeamId { get; init; }
        public bool UseDevelopmentServer { get; init; }
    }
}