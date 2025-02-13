using System.Collections.Generic;
using Eurofurence.App.Domain.Model.Events;

namespace Eurofurence.App.Domain.Model.Telegram
{
    public class TelegramUserRecord : EntityBase
    {
        public string Username { get; set; }
        public string Acl { get; set; }

        public string RegsysID { get; set; }

        public List<EventRecord> FavoriteEvents { get; set; } = new();
    }
}
