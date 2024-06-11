using Microsoft.Extensions.Configuration;

namespace Eurofurence.App.Server.Services.Abstractions.MinIO
{
    public class MinIoConfiguration
    {
        public string Endpoint { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public bool Secure { get; set; }
        public string Bucket { get; set; }

        public static MinIoConfiguration FromConfiguration(IConfiguration configuration)
            => new()
            {
                Endpoint = configuration["minIo:endpoint"],
                AccessKey = configuration["minIo:accessKey"],
                SecretKey = configuration["minIo:secretKey"],
                Secure = configuration.GetSection("minIo:secure").Get<bool>(),
                Bucket = configuration["minIo:bucket"]
            };
    }
}
