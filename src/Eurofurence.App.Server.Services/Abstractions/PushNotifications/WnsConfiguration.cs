namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public class WnsConfiguration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TargetTopic { get; set; }
    }
}