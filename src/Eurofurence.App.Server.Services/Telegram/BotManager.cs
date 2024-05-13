using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Domain.Model.Security;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Services.Abstractions.Telegram;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

// ReSharper disable CoVariantArrayConversion

namespace Eurofurence.App.Server.Services.Telegram
{
    public class BotManager
    {
        private readonly AppDbContext _appDbContext;
        private readonly IUserManager _userManager;
        private readonly IDealerService _dealerService;
        private readonly IEventService _eventService;
        private readonly IEventConferenceRoomService _eventConferenceRoomService;
        private readonly IRegSysAlternativePinAuthenticationProvider _regSysAlternativePinAuthenticationProvider;
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
            AppDbContext appDbContext,
            TelegramConfiguration telegramConfiguration,
            IUserManager userManager,
            IDealerService dealerService,
            IEventService eventService,
            IEventConferenceRoomService eventConferenceRoomService,
            IRegSysAlternativePinAuthenticationProvider regSysAlternativePinAuthenticationProvider,
            ITableRegistrationService tableRegistrationService,
            IPrivateMessageService privateMessageService,
            ICollectingGameService collectingGameService,
            ConventionSettings conventionSettings,
            ILoggerFactory loggerFactory,
            ITelegramMessageBroker telegramMessageBroker
        )
        {
            _appDbContext = appDbContext;
            _logger = loggerFactory.CreateLogger(GetType());
            _userManager = userManager;
            _dealerService = dealerService;
            _eventService = eventService;
            _eventConferenceRoomService = eventConferenceRoomService;
            _regSysAlternativePinAuthenticationProvider = regSysAlternativePinAuthenticationProvider;
            _privateMessageService = privateMessageService;
            _tableRegistrationService = tableRegistrationService;
            _collectingGameService = collectingGameService;
            _conventionSettings = conventionSettings;
            _telegramMessageBroker = telegramMessageBroker;

            if (!telegramConfiguration.IsConfigured)
            {
                _logger.LogWarning("Telegram configuration not available. Telegram bot will not be run.");
                return;
            }

            _botClient =
                string.IsNullOrEmpty(telegramConfiguration.Proxy)
                    ? new TelegramBotClient(telegramConfiguration.AccessToken)
                    : new TelegramBotClient(telegramConfiguration.AccessToken,
                        new HttpClient(
                            new HttpClientHandler
                            {
                                Proxy = new MiniProxy(telegramConfiguration.Proxy),
                                UseProxy = true
                            }));

            _conversationManager = new ConversationManager(
                loggerFactory,
                _botClient,
                (chatId) => new AdminConversation(
                    _appDbContext,
                    _userManager,
                    _regSysAlternativePinAuthenticationProvider,
                    privateMessageService,
                    _tableRegistrationService,
                    _collectingGameService,
                    _conventionSettings,
                    loggerFactory
                )
            );

            _telegramMessageBroker.OnSendMarkdownMessageToChatAsync +=
                _telegramMessageBroker_OnSendMarkdownMessageToChatAsync;
            _telegramMessageBroker.OnSendImageToChatAsync += _telegramMessageBroker_OnSendImageToChatAsync;

            Start();
        }

        private async Task<InlineQueryResult[]> QueryEventsAsync(string query)
        {
            if (query.Length < 3) return new InlineQueryResult[0];

            var events = await
                (_eventService.FindAll(a => a.IsDeleted == 0 && a.Title.ToLower().Contains(query.ToLower())))
                .OrderBy(a => a.StartDateTimeUtc)
                .Take(10)
                .ToListAsync();

            if (events.Count == 0) return Array.Empty<InlineQueryResult>();

            return events.Select(e =>
                {
                    e.ConferenceRoom = _eventConferenceRoomService.FindAll()
                        .FirstOrDefault(ecr => ecr.Id == e.ConferenceRoomId);

                    return new InlineQueryResultArticle(
                        e.Id.ToString(),
                        $"EVENT: {e.StartDateTimeUtc.DayOfWeek} {e.StartTime.ToString("hh\\:mm")}-{e.EndTime.ToString("hh\\:mm")}: {e.Title} " +
                        (string.IsNullOrEmpty(e.SubTitle) ? "" : $" ({e.SubTitle})"),
                        new InputTextMessageContent(
                            $"*Event:* https://app.eurofurence.org/{_conventionSettings.ConventionIdentifier}/Web/Events/{e.Id}")
                        {
                            ParseMode = ParseMode.Markdown
                        });
                })
                .ToArray();
        }

        private async Task<InlineQueryResult[]> QueryDealersAsync(string query)
        {
            if (query.Length < 3) return Array.Empty<InlineQueryResult>();

            var dealers =
                await _dealerService.FindAll(a => a.IsDeleted == 0 && (
                        a.DisplayName.ToLower().Contains(query.ToLower()) ||
                        a.AttendeeNickname.ToLower().Contains(query.ToLower())
                    ))
                    .OrderBy(a => a.DisplayNameOrAttendeeNickname)
                    .Take(10)
                    .ToListAsync();

            if (dealers.Count == 0) return Array.Empty<InlineQueryResult>();

            return dealers.Select(e => new InlineQueryResultArticle(
                    e.Id.ToString(),
                    $"DEALER: {e.DisplayNameOrAttendeeNickname}",
                    new InputTextMessageContent(
                        $"*Dealer:* https://app.eurofurence.org/{_conventionSettings.ConventionIdentifier}/Web/Dealers/{e.Id}")
                    {
                        ParseMode = ParseMode.Markdown
                    }))
                .ToArray();
        }

        private async Task<InlineQueryResult[]> QueryFursuitBadges(string query)
        {
            if (query.Length < 3) return Array.Empty<InlineQueryResult>();

            var badges = await
                (_appDbContext.FursuitBadges
                    .AsNoTracking()
                    .Where(
                        a => a.IsDeleted == 0 && a.IsPublic && a.Name.ToLower().Contains(query.ToLower())))
                .Take(5)
                .ToListAsync();

            if (badges.Count == 0) return Array.Empty<InlineQueryResult>();

            return badges.Select(e =>
                {
                    return new InlineQueryResultPhoto(e.Id.ToString(),
                        $"https://app.eurofurence.org/api/v2/Fursuits/Badges/{e.Id}/Image",
                        $"https://app.eurofurence.org/api/v2/Fursuits/Badges/{e.Id}/Image")
                    {
                        Title = e.Name,
                        Caption =
                            $"{e.Name}\n{e.Species} ({e.Gender})\n\nWorn by:{e.WornBy}\n\nhttps://fursuit.eurofurence.org/showSuit.php?id={e.ExternalReference}"
                    };
                })
                .ToArray();
        }

        public void Start()
        {
            //_botClient?.StartReceiving();
            using var cts = new CancellationTokenSource();
            _botClient?.StartReceiving(HandleUpdateAsync, PollingErrorHandler, null, cts.Token);
        }

        async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            Task handler = update switch
            {
                { Message: { } message } => BotClientOnMessage(message, cancellationToken),
                { EditedMessage: { } message } => BotClientOnMessage(message, cancellationToken),
                { CallbackQuery: { } callbackQuery } => BotClientOnCallbackQuery(callbackQuery, cancellationToken),
                { InlineQuery: { } inlineQuery } => BotClientOnInlineQuery(inlineQuery, cancellationToken),
                _ => UnknownUpdateHandlerAsync(update, cancellationToken)
            };

            await handler;
        }

        private async Task BotClientOnInlineQuery(InlineQuery inlineQuery, CancellationToken cancellationToken)
        {
            try
            {
                var queryString = inlineQuery.Query;
                var queries = new[]
                {
                    QueryEventsAsync(queryString),
                    QueryDealersAsync(queryString)
                };

                Task.WaitAll(queries);
                var results = queries.SelectMany(task => task.Result).ToArray();

                if (results.Length == 0) return;

                await _botClient.AnswerInlineQueryAsync(
                    inlineQuery.Id,
                    results,
                    cacheTime: 0,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("BotClientOnInlineQuery failed: {Message} {StackTrace}",
                    ex.Message, ex.StackTrace);
            }
        }

        private async Task BotClientOnCallbackQuery(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            try
            {
                await _botClient.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    //text: $"Received {callbackQuery.Data}",
                    cancellationToken: cancellationToken);

                lock (_answerredQueries)
                {
                    _answerredQueries.Where(a => DateTime.UtcNow.AddMinutes(-5) > a.Value)
                        .ToList()
                        .ForEach(a => _answerredQueries.Remove(a.Key));

                    if (callbackQuery.Message.Date < DateTime.UtcNow.AddMinutes(-5))
                        return;

                    if (_answerredQueries.ContainsKey(callbackQuery.Message.MessageId))
                        return;

                    _answerredQueries.Add(callbackQuery.Message.MessageId, DateTime.UtcNow);
                }

                await _conversationManager[callbackQuery.From.Id].OnCallbackQueryAsync(callbackQuery);
            }
            catch (Exception ex)
            {
                _logger.LogError("BotClientOnCallbackQuery failed: {Message} {StackTrace}",
                    ex.Message, ex.StackTrace);
            }
        }

        private async Task BotClientOnMessage(Message message, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(message.From.Username)) return;

            try
            {
                await _conversationManager[message.From.Id].OnMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError("BotClientOnMessage failed: {Message} {StackTrace}",
                    ex.Message, ex.StackTrace);
            }
        }

#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable RCS1163 // Unused parameter.
        private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
#pragma warning restore RCS1163 // Unused parameter.
#pragma warning restore IDE0060 // Remove unused parameter
        {
            _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
            return Task.CompletedTask;
        }

        Task PollingErrorHandler(ITelegramBotClient bot, Exception ex, CancellationToken ct)
        {
            Console.WriteLine($"Exception while polling for updates: {ex}");
            return Task.CompletedTask;
        }

        private async Task _telegramMessageBroker_OnSendMarkdownMessageToChatAsync(string chatId, string message)
        {
            await _botClient.SendTextMessageAsync(chatId: chatId, text: message, parseMode: ParseMode.Markdown);
        }

        private async Task _telegramMessageBroker_OnSendImageToChatAsync(string chatId, byte[] imageBytes,
            string message)
        {
            await _botClient.SendPhotoAsync(
                chatId: chatId,
                photo: new InputFileStream(new MemoryStream(imageBytes)),
                caption: message,
                parseMode: ParseMode.Markdown);
        }
    }
}