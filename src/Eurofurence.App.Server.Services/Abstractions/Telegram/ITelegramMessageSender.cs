using System;
using System.IO;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.ArtistsAlley;

namespace Eurofurence.App.Server.Services.Abstractions.Telegram
{
    public interface ITelegramMessageSender
    {
        Task SendMarkdownMessageToChatAsync(string chatId, string message);
        Task SendImageToChatAsync(string chatId, Stream image, string message = "");
        Task SendTableRegistrationAsync(string chatId, TableRegistrationRecord record);
    }

    public interface ITelegramMessageBroker : ITelegramMessageSender
    {
        event Func<string, string, Task> OnSendMarkdownMessageToChatAsync;
        event Func<string, Stream, string, Task> OnSendImageToChatAsync;
        event Func<string, TableRegistrationRecord, Task> OnTableRegistrationAsync;
    }
}