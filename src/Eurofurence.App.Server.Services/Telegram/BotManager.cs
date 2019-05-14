using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Server.Services.Abstraction.Telegram;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Eurofurence.App.Server.Services.Abstractions.Telegram;
using Microsoft.Extensions.Logging;
using System.IO;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Telegram.Bot.Types.InputFiles;
using Eurofurence.App.Domain.Model.Security;

// ReSharper disable CoVariantArrayConversion

namespace Eurofurence.App.Server.Services.Telegram
{
    public class BotManager
    {
        private readonly IUserManager _userManager;
        private readonly IDealerService _dealerService;
        private readonly IEventService _eventService;
        private readonly IEventConferenceRoomService _eventConferenceRoomService;
        private readonly IRegSysAlternativePinAuthenticationProvider _regSysAlternativePinAuthenticationProvider;
        private readonly IEntityRepository<PushNotificationChannelRecord> _pushNotificationChannelRepository;
        private readonly IEntityRepository<FursuitBadgeRecord> _fursuitBadgeRepository;
        private readonly IEntityRepository<FursuitBadgeImageRecord> _fursuitBadgeImageRepository;
        private readonly IEntityRepository<RegSysIdentityRecord> _regSysIdentityRepository;
        private readonly IPrivateMessageService _privateMessageService;
        private readonly ITableRegistrationService _tableRegistrationService;
        private readonly ICollectingGameService _collectingGameService;
        private readonly ConventionSettings _conventionSettings;
        private readonly ITelegramMessageBroker _telegramMessageBroker;
        private readonly TelegramBotClient _botClient;
        private readonly ConversationManager _conversationManager;

        private Dictionary<int, DateTime> _answerredQueries = new Dictionary<int, DateTime>();
        private ILogger _logger;


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
            IUserManager userManager,
            IDealerService dealerService,
            IEventService eventService,
            IEventConferenceRoomService eventConferenceRoomService,
            IRegSysAlternativePinAuthenticationProvider regSysAlternativePinAuthenticationProvider,
            IEntityRepository<PushNotificationChannelRecord> pushNotificationChannelRepository,
            IEntityRepository<FursuitBadgeRecord> fursuitBadgeRepository,
            IEntityRepository<FursuitBadgeImageRecord> fursuitBadgeImageRepository,
            IEntityRepository<RegSysIdentityRecord> regSysIdentityRepository,
            ITableRegistrationService tableRegistrationService,
            IPrivateMessageService privateMessageService,
            ICollectingGameService collectingGameService,
            ConventionSettings conventionSettings,
            ILoggerFactory loggerFactory,
            ITelegramMessageBroker telegramMessageBroker
            )
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _userManager = userManager;
            _dealerService = dealerService;
            _eventService = eventService;
            _eventConferenceRoomService = eventConferenceRoomService;
            _regSysAlternativePinAuthenticationProvider = regSysAlternativePinAuthenticationProvider;
            _pushNotificationChannelRepository = pushNotificationChannelRepository;
            _fursuitBadgeRepository = fursuitBadgeRepository;
            _fursuitBadgeImageRepository = fursuitBadgeImageRepository;
            _regSysIdentityRepository = regSysIdentityRepository;
            _privateMessageService = privateMessageService;
            _tableRegistrationService = tableRegistrationService;
            _collectingGameService = collectingGameService;
            _conventionSettings = conventionSettings;
            _telegramMessageBroker = telegramMessageBroker;

            if (string.IsNullOrWhiteSpace(telegramConfiguration.AccessToken))
            {
                _logger.LogInformation("No access token for Telegram Bot provided - not running bot.");
                return;
            }

            _botClient =
                string.IsNullOrEmpty(telegramConfiguration.Proxy)
                    ? new TelegramBotClient(telegramConfiguration.AccessToken)
                    : new TelegramBotClient(telegramConfiguration.AccessToken,
                        new MiniProxy(telegramConfiguration.Proxy));

            _conversationManager = new ConversationManager(
                loggerFactory,
                _botClient,
                (chatId) => new AdminConversation(
                    _userManager, 
                    _regSysAlternativePinAuthenticationProvider, 
                    _pushNotificationChannelRepository,
                    _fursuitBadgeRepository,
                    _fursuitBadgeImageRepository,
                    _regSysIdentityRepository,
                    privateMessageService,
                    _tableRegistrationService,
                    _collectingGameService,
                    _conventionSettings,
                    loggerFactory
                    )
                );

            _botClient.OnMessage += BotClientOnOnMessage;
            _botClient.OnCallbackQuery += BotClientOnOnCallbackQuery;

            _botClient.OnInlineQuery += BotClientOnOnInlineQuery;

            _telegramMessageBroker.OnSendMarkdownMessageToChatAsync += _telegramMessageBroker_OnSendMarkdownMessageToChatAsync;
            _telegramMessageBroker.OnSendImageToChatAsync += _telegramMessageBroker_OnSendImageToChatAsync;
        }

        private async Task<InlineQueryResultBase[]> QueryEvents(string query)
        {
            if (query.Length < 3) return new InlineQueryResultBase[0];

            var events =
                (await _eventService.FindAllAsync(a => a.IsDeleted == 0 && a.Title.ToLower().Contains(query.ToLower())))
                .OrderBy(a => a.StartDateTimeUtc)
                .Take(10)
                .ToList();

            if (events.Count == 0) return new InlineQueryResultBase[0];

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

                    messageBuilder.Append($"\n\n[Read more...](https://www.eurofurence.org/{_conventionSettings.ConventionIdentifier}/schedule/events/{e.SourceEventId}.en.html)");

                    var inputMessageContent = new InputTextMessageContent(messageBuilder.ToString())
                    {
                        ParseMode = ParseMode.Markdown
                    };

                    return new InlineQueryResultArticle(
                        e.Id.ToString(),
                        e.Title + (string.IsNullOrEmpty(e.SubTitle) ? "" : $" ({e.SubTitle})"),
                        inputMessageContent)
                    {
                        Description =
                            $"{e.StartDateTimeUtc.DayOfWeek}, {e.StartDateTimeUtc.Day}.{e.StartDateTimeUtc.Month} - {e.StartTime} until {e.EndTime}"
                    };
                })
                .ToArray();
        }


        private async Task<InlineQueryResultBase[]> QueryFursuitBadges(string query)
        {
            if (query.Length < 3) return new InlineQueryResultBase[0];


            var badges =
                (await _fursuitBadgeRepository.FindAllAsync(
                    a => a.IsDeleted == 0 && a.IsPublic && a.Name.ToLower().Contains(query.ToLower())))
                .Take(5)
                .ToList();

            if (badges.Count == 0) return new InlineQueryResultBase[0];

            return badges.Select(e =>
                {
                    return new InlineQueryResultPhoto(e.Id.ToString(),
                        $"https://app.eurofurence.org/api/v2/Fursuits/Badges/{e.Id}/Image",
                        $"https://app.eurofurence.org/api/v2/Fursuits/Badges/{e.Id}/Image")
                    {
                        
                        Title = e.Name,
                        Caption = $"{e.Name}\n{e.Species} ({e.Gender})\n\nWorn by:{e.WornBy}\n\nhttps://fursuit.eurofurence.org/showSuit.php?id={e.ExternalReference}"
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
                };

                Task.WaitAll(queries);
                var results = queries.SelectMany(task => task.Result).ToArray();

                if (results.Length == 0) return;

                await _botClient.AnswerInlineQueryAsync(
                    inlineQueryEventArgs.InlineQuery.Id,
                    results,
                    cacheTime: 0);
            }
            catch (Exception ex)
            {
                _logger.LogError("BotClientOnOnInlineQuery failed: {Message} {StackTrace}",
                    ex.Message, ex.StackTrace);
            }
        }

        public void Start()
        {
            _botClient?.StartReceiving();
        }



        private async void BotClientOnOnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            try
            {
                await _botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id);

                lock (_answerredQueries)
                {
                    _answerredQueries.Where(a => DateTime.UtcNow.AddMinutes(-5) > a.Value)
                        .ToList()
                        .ForEach(a => _answerredQueries.Remove(a.Key));

                    if (e.CallbackQuery.Message.Date < DateTime.UtcNow.AddMinutes(-5))
                        return;

                    if (_answerredQueries.ContainsKey(e.CallbackQuery.Message.MessageId))
                        return;

                    _answerredQueries.Add(e.CallbackQuery.Message.MessageId, DateTime.UtcNow);
                }
                
                await _conversationManager[e.CallbackQuery.From.Id].OnCallbackQueryAsync(e);
            }
            catch (Exception ex)
            {
                _logger.LogError("BotClientOnOnCallbackQuery failed: {Message} {StackTrace}",
                    ex.Message, ex.StackTrace);
            }
        }

        private async void BotClientOnOnMessage(object sender, MessageEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Message.From.Username)) return;

            try
            {
                await _conversationManager[e.Message.From.Id].OnMessageAsync(e);
            }
            catch (Exception ex)
            {
                _logger.LogError("BotClientOnOnMessage failed: {Message} {StackTrace}",
                    ex.Message, ex.StackTrace);
            }
        }

        private async Task _telegramMessageBroker_OnSendMarkdownMessageToChatAsync(string chatId, string message)
        {
            await _botClient.SendTextMessageAsync(chatId, message, ParseMode.Markdown);
        }

        private async Task _telegramMessageBroker_OnSendImageToChatAsync(string chatId, byte[] imageBytes, string message)
        {
            await _botClient.SendPhotoAsync(
                chatId, new InputOnlineFile(new MemoryStream(imageBytes)),
                caption: message,
                parseMode: ParseMode.Markdown);
                
        }

    }
}
