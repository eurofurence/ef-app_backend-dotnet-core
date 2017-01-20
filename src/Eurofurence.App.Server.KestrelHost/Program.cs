using Eurofurence.App.Server.Web;
using Microsoft.AspNetCore.Hosting;

namespace Eurofurence.App.Server.KestrelHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseStartup<Startup>()
                .UseKestrel()
                .UseUrls("http://localhost:30001")
                .Build();


            
            host.Run();
        }
    }
}
