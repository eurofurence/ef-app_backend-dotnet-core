using System;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Telegram
{
    public interface ITelegramMessageSender
    {
        Task SendMarkdownMessageToChatAsync(string chatId, string message);
    }

    public interface ITelegramMessageBroker : ITelegramMessageSender
    {
        event Func<string, string, Task> OnSendMarkdownMessageToChatAsync;
    }
}