using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

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
