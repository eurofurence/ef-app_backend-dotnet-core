using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Eurofurence.App.Server.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var baseUrl = (args.Length >= 1) ? args[0] : "http://*:30001";

            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(baseUrl);
                    webBuilder.UseStartup<Startup>();
                })
                .Build()
                .Run();
        }
    }
}
