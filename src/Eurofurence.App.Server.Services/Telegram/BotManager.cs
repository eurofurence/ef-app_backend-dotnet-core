using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstraction.Telegram;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;

// ReSharper disable CoVariantArrayConversion

namespace Eurofurence.App.Server.Services.Telegram
{
    public class BotManager
    {
        private readonly IDealerService _dealerService;
        private readonly IImageService _imageService;
        private readonly IEventService _eventService;
        private readonly IEventConferenceDayService _eventConferenceDayService;
        private readonly IEventConferenceTrackService _eventConferenceTrackService;
        private readonly IEventConferenceRoomService _eventConferenceRoomService;
        private readonly TelegramBotClient _botClient;
        private readonly ConversationManager _conversationManager;

        internal class MiniProxy : IWebProxy
        {
            private readonly string _uri;

            public MiniProxy(string uri)
            {
                _uri = uri;
            }
            public Uri GetProxy(Uri destination)
            {
                return new Uri(_uri);
            }

            public bool IsBypassed(Uri host)
            {
                return false;
            }

            public ICredentials Credentials { get; set; }
        }

        public BotManager(
            TelegramConfiguration telegramConfiguration,
            IDealerService dealerService,
            IImageService imageService,
            IEventService eventService,
            IEventConferenceDayService eventConferenceDayService,
            IEventConferenceTrackService eventConferenceTrackService,
            IEventConferenceRoomService eventConferenceRoomService
            )
        {
            _dealerService = dealerService;
            _imageService = imageService;
            _eventService = eventService;
            _eventConferenceDayService = eventConferenceDayService;
            _eventConferenceTrackService = eventConferenceTrackService;
            _eventConferenceRoomService = eventConferenceRoomService;

            _botClient =
                string.IsNullOrEmpty(telegramConfiguration.Proxy)
                    ? new TelegramBotClient(telegramConfiguration.AccessToken)
                    : new TelegramBotClient(telegramConfiguration.AccessToken,
                        new MiniProxy(telegramConfiguration.Proxy));

            _conversationManager = new ConversationManager(
                _botClient,
                _eventService,
                _eventConferenceDayService
                );

            //_botClient.OnMessage += BotClientOnOnMessage;
            //_botClient.OnCallbackQuery += BotClientOnOnCallbackQuery;

            _botClient.OnInlineQuery += BotClientOnOnInlineQuery;
        }

        private async Task<InlineQueryResult[]> QueryEvents(string query)
        {
            if (query.Length < 3) return new InlineQueryResult[0];

            var events =
                (await _eventService.FindAllAsync(a => a.IsDeleted == 0 && a.Title.ToLower().Contains(query.ToLower())))
                .OrderBy(a => a.StartDateTimeUtc)
                .Take(10)
                .ToList();

            if (events.Count == 0) return new InlineQueryResult[0];

            var eventConferenceRooms = await _eventConferenceRoomService.FindAllAsync();

            return events.Select(e =>
                {
                    e.ConferenceRoom = eventConferenceRooms.Single(r => r.Id == e.ConferenceRoomId);

                    var messageBuilder = new StringBuilder();
                    messageBuilder.Append($"*{e.Title}*");

                    if (!string.IsNullOrEmpty(e.SubTitle))
                        messageBuilder.Append($" - ({e.SubTitle})");

                    messageBuilder.Append(
                        $"\n{e.StartDateTimeUtc.DayOfWeek}, {e.StartTime} to {e.EndTime} in {e.ConferenceRoom.Name}");

                    if (!string.IsNullOrEmpty(e.Description))
                    {
                        var desc = e.Description;
                        if (desc.Length > 500) desc = desc.Substring(0, 500) + "...";
                        messageBuilder.Append($"\n\n_{desc}_");
                    }

                    messageBuilder.Append("\n\n[Read more...](https://app.eurofurence.org)");

                    return new InlineQueryResultArticle()
                    {
                        Id = e.Id.ToString(),
                        InputMessageContent = new InputTextMessageContent()
                        {
                            MessageText = messageBuilder.ToString(),
                            ParseMode = ParseMode.Markdown
                        },
                        Title = e.Title + (string.IsNullOrEmpty(e.SubTitle) ? "" : $" ({e.SubTitle})"),
                        Description =
                            $"{e.StartDateTimeUtc.DayOfWeek}, {e.StartDateTimeUtc.Day}.{e.StartDateTimeUtc.Month} - {e.StartTime} until {e.EndTime}"
                    };
                })
                .ToArray();
        }

        private async Task<InlineQueryResult[]> QueryDealers(string query)
        {
            if (query.Length < 3) return new InlineQueryResult[0];

            var dealers =
                (await _dealerService.FindAllAsync(a => a.IsDeleted == 0 && (
                    a.DisplayName.ToLower().Contains(query.ToLower()) || a.AttendeeNickname.ToLower().Contains(query.ToLower())
                    )))
                .Take(5)
                .ToList();

            if (dealers.Count == 0) return new InlineQueryResult[0];

            return dealers.Select(e =>
                {
                    var messageBuilder = new StringBuilder();
                    messageBuilder.Append($"*{e.AttendeeNickname} {e.DisplayName}*");

                    messageBuilder.Append("\n\n[Read more...](https://app.eurofurence.org)");

                    return new InlineQueryResultPhoto()
                    {
                        Id = e.Id.ToString(),
                        Url = "https://app.eurofurence.org/images/qrcode_getAndroidApp.png",
                        ThumbUrl = "https://app.eurofurence.org/images/qrcode_getAndroidApp.png",
                        Title = e.AttendeeNickname,
                        Description = "..."
                    };
                })
                .ToArray();
        }


        private async void BotClientOnOnInlineQuery(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            try
            {
                var queryString = inlineQueryEventArgs.InlineQuery.Query;
                var queries = new[]
                {
                    QueryEvents(queryString),
                  //  QueryDealers(queryString)
                };

                Task.WaitAll(queries);
                var results = queries.SelectMany(task => task.Result).ToArray();

                if (results.Length == 0) return;

                await _botClient.AnswerInlineQueryAsync(
                    inlineQueryEventArgs.InlineQuery.Id,
                    results,
                    cacheTime: 0);
            }
            catch (Exception e)
            {
            }
        }

        public void Start()
        {
            _botClient.StartReceiving();
        }

        private async void BotClientOnOnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            await _conversationManager[e.CallbackQuery.From.Id].OnCallbackQueryAsync(e);
            await _botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id);
        }

        private async void BotClientOnOnMessage(object sender, MessageEventArgs e)
        {
            await _conversationManager[e.Message.From.Id].OnMessageAsync(e);
        }
    }
}
