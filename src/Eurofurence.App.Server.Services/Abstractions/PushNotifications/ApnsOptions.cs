namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public class ApnsOptions
    {
        public bool IsConfigured => !string.IsNullOrWhiteSpace(BundleId) && !string.IsNullOrWhiteSpace(CertContent) && !string.IsNullOrWhiteSpace(KeyId) && !string.IsNullOrWhiteSpace(TeamId);
        public string BundleId { get; set; }
        public string CertContent { get; set; }
        public string KeyId { get; set; }
        public string TeamId { get; set; }
        public bool UseDevelopmentServer { get; set; }
    }
}