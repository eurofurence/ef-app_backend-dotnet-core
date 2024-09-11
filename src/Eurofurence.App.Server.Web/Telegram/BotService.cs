using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Eurofurence.App.Common.ExtensionMethods;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Telegram;
using Eurofurence.App.Server.Services.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace Eurofurence.App.Server.Web.Telegram;

public class BotService : BackgroundService, IUpdateHandler
{
    private readonly ILogger<BotService> _logger;
    private readonly ITelegramBotClient _client;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventService _eventService;
    private readonly IEventConferenceRoomService _eventConferenceRoomService;
    private readonly ConventionSettings _conventionSettings;
    private readonly IDealerService _dealerService;
    private readonly IImageService _imageService;
    private readonly ITableRegistrationService _tableRegistrationService;
    private readonly Dictionary<long, IConversation> _conversations = new();
    private readonly Dictionary<long, DateTime> _answeredQueries = new();
    private readonly Dictionary<long, Guid> _tableRegistrations = new();

    public BotService(
        ILogger<BotService> logger,
        ITelegramBotClient client,
        IServiceProvider serviceProvider,
        IEventService eventService,
        IEventConferenceRoomService eventConferenceRoomService,
        ConventionSettings conventionSettings,
        IDealerService dealerService,
        ITelegramMessageBroker telegramMessageBroker,
        IImageService imageService,
        ITableRegistrationService tableRegistrationService)
    {
        _logger = logger;
        _client = client;
        _serviceProvider = serviceProvider;
        _eventService = eventService;
        _eventConferenceRoomService = eventConferenceRoomService;
        _conventionSettings = conventionSettings;
        _dealerService = dealerService;
        _imageService = imageService;
        _tableRegistrationService = tableRegistrationService;

        telegramMessageBroker.OnSendMarkdownMessageToChatAsync += OnSendMarkdownMessageToChatAsync;
        telegramMessageBroker.OnSendImageToChatAsync += OnSendImageToChatAsync;
        telegramMessageBroker.OnTableRegistrationAsync += OnTableRegistrationAsync;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _client.ReceiveAsync(
            this,
            null,
            stoppingToken
        );
    }

    public Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        return update switch
        {
            { Message: { } message } => BotClientOnMessage(message, cancellationToken),
            { EditedMessage: { } message } => BotClientOnMessage(message, cancellationToken),
            { CallbackQuery: { } callbackQuery } => BotClientOnCallbackQuery(callbackQuery, cancellationToken),
            { InlineQuery: { } inlineQuery } => BotClientOnInlineQuery(inlineQuery, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update)
        };
    }

    public Task HandlePollingErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(exception, "Exception while polling for updates");
        return Task.CompletedTask;
    }

    private Task OnSendMarkdownMessageToChatAsync(string chatId, string message)
    {
        return _client.SendTextMessageAsync(chatId, message, parseMode: ParseMode.Markdown);
    }

    private async Task OnSendImageToChatAsync(string chatId, Stream image, string message)
    {
        await using (image)
        {
            await _client.SendPhotoAsync(
                chatId,
                new InputFileStream(image),
                caption: message,
                parseMode: ParseMode.Markdown
            );
        }
    }

    private async Task OnTableRegistrationAsync(string chatId, TableRegistrationRecord record)
    {
        var message = await _client.SendTextMessageAsync(
            chatId,
            $@"*New Request:* {record.OwnerUsername.EscapeMarkdown()} ({record.OwnerUid.EscapeMarkdown()})

*Display Name:* {record.DisplayName.EscapeMarkdown()}
*Location:* {record.Location.RemoveMarkdown()}
*Description:* {record.ShortDescription.EscapeMarkdown()}",
            parseMode: ParseMode.Markdown,
            replyMarkup: new InlineKeyboardMarkup([
                InlineKeyboardButton.WithCallbackData("Approve"),
                InlineKeyboardButton.WithCallbackData("Reject")
            ])
        );

        if (record.Image is { } image)
        {
            await using var stream = await _imageService.GetImageContentByImageIdAsync(image.Id);
            await _client.SendPhotoAsync(
                chatId,
                new InputFileStream(stream)
            );
        }

        lock (_tableRegistrations)
        {
            _tableRegistrations.Add(message.MessageId, record.Id);
        }
    }

    private async Task BotClientOnInlineQuery(InlineQuery inlineQuery, CancellationToken cancellationToken)
    {
        try
        {
            var queryString = inlineQuery.Query;
            var queries = new[]
            {
                QueryEventsAsync(queryString, cancellationToken),
                QueryDealersAsync(queryString, cancellationToken)
            };

            Task.WaitAll(queries, cancellationToken);
            var results = queries
                .SelectMany(task => task.Result)
                .ToArray();

            if (results.Length == 0)
            {
                return;
            }

            await _client.AnswerInlineQueryAsync(
                inlineQuery.Id,
                results,
                0,
                cancellationToken: cancellationToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BotClientOnInlineQuery failed");
        }
    }

    private async Task BotClientOnCallbackQuery(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        try
        {
            await _client.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                //text: $"Received {callbackQuery.Data}",
                cancellationToken: cancellationToken
            );

            _answeredQueries
                .Where(a => DateTime.UtcNow.AddMinutes(-5) > a.Value)
                .ToList()
                .ForEach(a => _answeredQueries.Remove(a.Key));

            if (callbackQuery.Message.Date < DateTime.UtcNow.AddMinutes(-5))
                return;

            if (_answeredQueries.ContainsKey(callbackQuery.Message.MessageId))
                return;

            _answeredQueries.Add(callbackQuery.Message.MessageId, DateTime.UtcNow);

            Guid record;
            lock (_tableRegistrations)
            {
                _tableRegistrations.Remove(callbackQuery.Message.MessageId, out record);
            }

            if (record == Guid.Empty)
            {
                await GetOrCreateConversation(callbackQuery.From.Id).OnCallbackQueryAsync(callbackQuery);
            }
            else
            {
                await HandleTableRegistrationQueryAsync(record, callbackQuery, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BotClientOnCallbackQuery failed");
        }
    }

    private async Task BotClientOnMessage(Message message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(message.From?.Username))
        {
            return;
        }

        try
        {
            await GetOrCreateConversation(message.From.Id).OnMessageAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BotClientOnMessage failed");
        }
    }

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    private IConversation GetOrCreateConversation(long chatId)
    {
        if (_conversations.TryGetValue(chatId, out var conversation))
        {
            if (conversation.LastActivityUtc > DateTime.UtcNow.AddMinutes(-5))
            {
                conversation.LastActivityUtc = DateTime.UtcNow;
                return conversation;
            }
        }

        conversation = _serviceProvider.GetRequiredService<AdminConversation>();
        conversation.ChatId = chatId;
        conversation.BotClient = _client;
        conversation.LastActivityUtc = DateTime.UtcNow;

        _conversations[chatId] = conversation;
        return conversation;
    }

    private async Task<InlineQueryResult[]> QueryEventsAsync(string query, CancellationToken cancellationToken)
    {
        if (query.Length < 3)
        {
            return [];
        }

        var events = await _eventService
            .FindAll(a => a.IsDeleted == 0 && a.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(a => a.StartDateTimeUtc)
            .Take(10)
            .ToListAsync(cancellationToken);

        if (events.Count == 0)
        {
            return [];
        }

        var tasks = events
            .Select(async e =>
            {
                e.ConferenceRoom = await _eventConferenceRoomService
                    .FindAll()
                    .FirstOrDefaultAsync(ecr => ecr.Id == e.ConferenceRoomId, cancellationToken);

                return new InlineQueryResultArticle(
                    e.Id.ToString(),
                    $"EVENT: {e.StartDateTimeUtc.DayOfWeek} {e.StartTime:hh\\:mm}-{e.EndTime:hh\\:mm}: {e.Title} " +
                    (string.IsNullOrEmpty(e.SubTitle) ? "" : $" ({e.SubTitle})"),
                    new InputTextMessageContent(
                        $"*Event:* https://app.eurofurence.org/{_conventionSettings.ConventionIdentifier}/Web/Events/{e.Id}")
                    {
                        ParseMode = ParseMode.Markdown
                    }
                );
            });

        return await Task.WhenAll(tasks);
    }

    private async Task<InlineQueryResult[]> QueryDealersAsync(string query, CancellationToken cancellationToken)
    {
        if (query.Length < 3)
        {
            return [];
        }

        var dealers = await _dealerService
            .FindAll(a => a.IsDeleted == 0 && (
                a.DisplayName.ToLower().Contains(query.ToLower()) ||
                a.AttendeeNickname.ToLower().Contains(query.ToLower())
            ))
            .OrderBy(a => a.DisplayNameOrAttendeeNickname)
            .Take(10)
            .ToListAsync(cancellationToken);

        if (dealers.Count == 0)
        {
            return [];
        }

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

    private async Task HandleTableRegistrationQueryAsync(
        Guid recordId,
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken)
    {
        var record = await _tableRegistrationService.FindOneAsync(recordId, cancellationToken);

        if (record?.State == TableRegistrationRecord.RegistrationStateEnum.Pending)
        {
            switch (callbackQuery.Data)
            {
                case "Approve":
                    await _tableRegistrationService.ApproveByIdAsync(
                        recordId,
                        $"Telegram:@{callbackQuery.From.Username}"
                    );
                    break;

                case "Reject":
                    await _tableRegistrationService.RejectByIdAsync(
                        recordId,
                        $"Telegram:@{callbackQuery.From.Username}"
                    );
                    break;
            }
        }

        await _client.EditMessageReplyMarkupAsync(
            callbackQuery.Message.Chat.Id,
            callbackQuery.Message.MessageId,
            null,
            cancellationToken
        );
    }
}