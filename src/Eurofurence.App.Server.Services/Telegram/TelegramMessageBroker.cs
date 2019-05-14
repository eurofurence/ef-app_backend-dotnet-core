using System;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.Telegram;

namespace Eurofurence.App.Server.Services.Telegram
{
    public class TelegramMessageBroker : ITelegramMessageBroker
    {
        public async Task SendMarkdownMessageToChatAsync(string chatId, string message)
        {
            if (OnSendMarkdownMessageToChatAsync == null) return;

            var invocationList = OnSendMarkdownMessageToChatAsync.GetInvocationList();
            var handlerTasks = new Task[invocationList.Length];

            for (var i = 0; i < invocationList.Length; i++)
            {
                handlerTasks[i] = ((Func<string, string, Task>)invocationList[i])(chatId, message);
            }

            await Task.WhenAll(handlerTasks);
        }

        public async Task SendImageToChatAsync(string chatId, byte[] imageBytes, string message = "")
        {
            var invocationList = OnSendImageToChatAsync.GetInvocationList();
            var handlerTasks = new Task[invocationList.Length];

            for (var i = 0; i < invocationList.Length; i++)
            {
                handlerTasks[i] = ((Func<string, byte[], string, Task>)invocationList[i])(chatId, imageBytes, message);
            }

            await Task.WhenAll(handlerTasks);
        }

        public event Func<string, string, Task> OnSendMarkdownMessageToChatAsync;
        public event Func<string, byte[], string, Task> OnSendImageToChatAsync;
    }
}