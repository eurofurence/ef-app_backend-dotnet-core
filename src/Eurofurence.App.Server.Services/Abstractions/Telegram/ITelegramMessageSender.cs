using System;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.ArtistsAlley;

namespace Eurofurence.App.Server.Services.Abstractions.Telegram
{
    public interface ITelegramMessageSender
    {
        Task SendMarkdownMessageToChatAsync(string chatId, string message);
        Task SendImageToChatAsync(string chatId, byte[] imageBytes, string message = "");
        Task SendTableRegistrationAsync(string chatId, TableRegistrationRecord record);
    }

    public interface ITelegramMessageBroker : ITelegramMessageSender
    {
        event Func<string, string, Task> OnSendMarkdownMessageToChatAsync;
        event Func<string, byte[], string, Task> OnSendImageToChatAsync;
        event Func<string, TableRegistrationRecord, Task> OnTableRegistrationAsync;
    }
}