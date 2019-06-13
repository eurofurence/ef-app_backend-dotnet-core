using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Eurofurence.App.Server.Web
{
    public class Program
    {
        private static string _baseUrl;

        public static void Main(string[] args)
        {
            _baseUrl = (args.Length >= 1) ? args[0] : "http://*:30001";

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls(_baseUrl)
                .UseStartup<Startup>();
    }
}
