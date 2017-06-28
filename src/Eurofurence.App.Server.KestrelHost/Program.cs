using Eurofurence.App.Server.Services.Telegram;
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
                .UseUrls("http://*:30001")
                .Build();

            var botManager = (BotManager)host.Services.GetService(typeof(BotManager));
            botManager.Start();

            host.Run();
        }
    }
}