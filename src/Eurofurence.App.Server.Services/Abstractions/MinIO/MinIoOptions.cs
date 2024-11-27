namespace Eurofurence.App.Server.Services.Abstractions.MinIO
{
    public class MinIoOptions
    {
        public string Endpoint { get; init; }
        public string BaseUrl { get; init; }
        public string Region { get; init; } = "us-east-1";
        public string AccessKey { get; init; }
        public string SecretKey { get; init; }
        public bool Secure { get; init; }
        public string Bucket { get; init; }
    }
}
