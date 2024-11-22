namespace Eurofurence.App.Server.Services.Abstractions.MinIO
{
    public class MinIoOptions
    {
        public string Endpoint { get; set; }
        public string BaseUrl { get; set; }
        public string Region { get; set; } = "us-east-1";
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public bool Secure { get; set; }
        public string Bucket { get; set; }
    }
}
