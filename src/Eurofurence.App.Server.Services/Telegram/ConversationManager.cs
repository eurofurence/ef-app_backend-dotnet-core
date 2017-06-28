using System;
using System.Collections.Generic;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Eurofurence.App.Server.Services.Telegram
{
    public class ConversationManager
    {
        private object _syncRoot = new object();

        public  ITelegramBotClient BotClient { get; }
        public IEventService EventService { get; }
        public IEventConferenceDayService EventConferenceDayService { get; }

        private Dictionary<int, Conversation> _conversations = new Dictionary<int, Conversation>();

        public ConversationManager(
            ITelegramBotClient botClient, 
            IEventService eventService,
            IEventConferenceDayService eventConferenceDayService
            )
        {
            BotClient = botClient;
            EventService = eventService;
            EventConferenceDayService = eventConferenceDayService;
        }

        public Conversation this[ChatId chatId]
        {
            get { return GetConversation(chatId.GetHashCode()); }
        }

        private Conversation GetConversation(int chatIdHashCode)
        {
            lock (_syncRoot)
            {
                if (_conversations.ContainsKey(chatIdHashCode))
                {
                    if (_conversations[chatIdHashCode].LastActivity > DateTime.Now.AddSeconds(-30))
                    {
                        _conversations[chatIdHashCode].LastActivity = DateTime.Now;
                        return _conversations[chatIdHashCode];
                    }
                    _conversations.Remove(chatIdHashCode);
                }

                var conf = new Conversation(this) { ChatId = chatIdHashCode, LastActivity = DateTime.Now };
                _conversations.Add(chatIdHashCode, conf);
                return conf;
            }
        }
    }
}