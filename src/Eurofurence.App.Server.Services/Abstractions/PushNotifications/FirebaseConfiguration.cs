namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public class FirebaseConfiguration
    {
        public string AuthorizationKey { get; set; }
        public string TargetTopicAll { get; set; }
        public string TargetTopicAndroid { get; set; }
        public string TargetTopicIos { get; set; }
    }
}