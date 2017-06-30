using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace Eurofurence.App.Server.Services.Telegram
{
    public interface IConversation
    {
        ChatId ChatId { get; set; }
        DateTime LastActivityUtc { get; set; }
        ITelegramBotClient BotClient { set; }

        Task OnCallbackQueryAsync(CallbackQueryEventArgs callbackQueryEventArgs);
        Task OnMessageAsync(MessageEventArgs e);
    }
}