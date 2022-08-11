using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Eurofurence.App.Server.Services.Telegram
{
    public class ConversationManager
    {
        private readonly ITelegramBotClient _botClient;
        private readonly Func<ChatId, IConversation> _conversationFactory;

        private object _syncRoot = new object();

        private Dictionary<long, IConversation> _conversations = new Dictionary<long, IConversation>();
        private ILogger _logger;

        public ConversationManager(
            ILoggerFactory loggerFactory,
            ITelegramBotClient botClient, 
            Func<ChatId, IConversation> conversationFactory
            )
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _conversationFactory = conversationFactory;
            _botClient = botClient;
        }

        public IConversation this[long chatId]
        {
            get { return GetConversation(chatId); }
        }

        private IConversation GetConversation(long chatId)
        {
            lock (_syncRoot)
            {
                if (_conversations.ContainsKey(chatId))
                {
                    if (_conversations[chatId].LastActivityUtc > DateTime.UtcNow.AddMinutes(-5))
                    {
                        _conversations[chatId].LastActivityUtc = DateTime.UtcNow;
                        return _conversations[chatId];
                    }
                    _conversations.Remove(chatId);
                }

                var newConversation = _conversationFactory(chatId);
                newConversation.ChatId = chatId;
                newConversation.BotClient = _botClient;
                newConversation.LastActivityUtc = DateTime.UtcNow;

                _conversations.Add(chatId, newConversation);
                return newConversation;
            }
        }
    }
}