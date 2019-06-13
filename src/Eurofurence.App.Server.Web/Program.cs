using Eurofurence.App.Server.Services.Telegram;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Eurofurence.App.Server.Web
{
    public class Program
    {
        public static BotManager _botManager { get; private set; }

        public static void Main(string[] args)
        {
            var baseUrl = (args.Length >= 1) ? args[0] : "http://*:30001";

            var host = CreateWebHostBuilder(baseUrl).Build();

            _botManager = (BotManager)host.Services.GetService(typeof(BotManager));
            _botManager.Start();

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string baseUrl) =>
            WebHost.CreateDefaultBuilder()
                .UseUrls(baseUrl)
                .UseStartup<Startup>();
    }
}
