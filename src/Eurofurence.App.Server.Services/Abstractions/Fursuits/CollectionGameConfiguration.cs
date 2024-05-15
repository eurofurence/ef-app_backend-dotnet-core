using Microsoft.Extensions.Configuration;
using System;

namespace Eurofurence.App.Server.Services.Abstractions.Fursuits
{
    public class CollectionGameConfiguration
    {
        public const string CollectionGame = "collectionGame";

        public int LogLevel { get; set; }
        public string LogFile { get; set; }
        public string TelegramManagementChatId { get; set; }

        public static CollectionGameConfiguration FromConfiguration(IConfiguration configuration)
            => new CollectionGameConfiguration()
            {
                LogFile = configuration["collectionGame:logFile"],
                LogLevel = Convert.ToInt32(configuration["collectionGame:logLevel"]),
                TelegramManagementChatId = configuration["collectionGame:telegramManagementChatId"]
            };
    }
}