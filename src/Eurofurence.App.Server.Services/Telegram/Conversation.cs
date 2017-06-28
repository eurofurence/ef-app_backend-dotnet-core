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
        private readonly ConversationManager _manager;

        public ChatId ChatId { get; set; }
        public DateTime LastActivity { get; set; }

        public Conversation(ConversationManager manager)
        {
            _manager = manager;
        }

        private async Task RespondWithChoicesAsync(Message message,
            params KeyValuePair<string, string>[] choices)
        {
            var keyboard = new InlineKeyboardMarkup(
                choices.Select(c => new [] { new InlineKeyboardButton(c.Key, c.Value) }).ToArray());


            await _manager.BotClient.SendTextMessageAsync(message.Chat.Id, "Choose", replyMarkup: keyboard);
        }

        public async Task OnMessageAsync(MessageEventArgs e)
        {
            if (e.Message.Text == "/start")
            {
                await RespondWithChoicesAsync(e.Message,
                    new KeyValuePair<string, string>("List Events", "e:listEvents"),
                    new KeyValuePair<string, string>("Do Stuff", "e:doStuff")
                );
            }
        }

        public async Task OnCallbackQueryAsync(CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var parts = callbackQueryEventArgs.CallbackQuery.Data.Split(':');
            if (parts[0] == "e" && parts.Length >= 2)
            {
                await _manager.BotClient.SendTextMessageAsync(callbackQueryEventArgs.CallbackQuery.Message.Chat.Id,
                    $"You chose {parts[1]}");

                if (parts[1] == "listEvents")
                {
                    await RespondWithChoicesAsync(callbackQueryEventArgs.CallbackQuery.Message,
                        (await _manager.EventConferenceDayService.FindAllAsync())
                        .OrderBy(a => a.Date)
                        .Select(a => new KeyValuePair<string, string>(
                            $"{a.Date.DayOfWeek} ({a.Date.Day}.{a.Date.Month}): {a.Name}", 
                            $"e:listEventsByDay:{a.Id}")).ToArray());
                }

                if (parts[1] == "listEventsByDay" && parts.Length == 3)
                {
                    await RespondWithChoicesAsync(callbackQueryEventArgs.CallbackQuery.Message,
                        (await _manager.EventService.FindAllAsync(a => a.ConferenceDayId == Guid.Parse(parts[2])))
                        .OrderBy(a => a.StartDateTimeUtc)
                        .Select(a => new KeyValuePair<string, string>(
                            $"{a.StartTime}: {a.Title} **{a.SubTitle}**",
                            $"e:showEventDetails:{a.Id}")).ToArray());
                }
            }
        }
    }
}
