using System;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Telegram
{
    public interface ITelegramMessageSender
    {
        Task SendMarkdownMessageToChatAsync(string chatId, string message);
        Task SendImageToChatAsync(string chatId, byte[] imageBytes, string message = "");
    }

    public interface ITelegramMessageBroker : ITelegramMessageSender
    {
        event Func<string, string, Task> OnSendMarkdownMessageToChatAsync;
        event Func<string, byte[], string, Task> OnSendImageToChatAsync;
    }
}