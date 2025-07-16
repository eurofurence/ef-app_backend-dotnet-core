using System;

namespace Eurofurence.App.Server.Services.Telegram
{
    public class Conversation
    {
        public DateTime LastActivityUtc { get; set; }

        public Conversation()
        {

        }

        //protected async Task RespondWithChoicesAsync(
        //    Message message, 
        //    string text,
        //    params KeyValuePair<string, string>[] choices)
        //{
        //    var keyboard = new InlineKeyboardMarkup(
        //        choices.Select(c => new [] { new InlineKeyboardButton(c.Key, c.Value) }).ToArray());


        //    await _manager._botClient.SendTextMessageAsync(message.Chat.Id, text, replyMarkup: keyboard);
        //}
    }
}
