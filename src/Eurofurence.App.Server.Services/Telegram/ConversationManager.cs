using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Eurofurence.App.Server.Services.Telegram
{
    public class ConversationManager
    {
        private readonly ITelegramBotClient _botClient;
        private readonly Func<ChatId, IConversation> _conversationFactory;

        private object _syncRoot = new object();

        private Dictionary<string, IConversation> _conversations = new Dictionary<string, IConversation>();

        public ConversationManager(
            ITelegramBotClient botClient, 
            Func<ChatId, IConversation> conversationFactory
            )
        {
            _conversationFactory = conversationFactory;
            _botClient = botClient;
        }

        public IConversation this[string chatId]
        {
            get { return GetConversation(chatId); }
        }

        private IConversation GetConversation(string chatId)
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