﻿using System.Collections.Generic;
using Eurofurence.App.Domain.Model.Events;

namespace Eurofurence.App.Domain.Model.Telegram
{
    public class TelegramUserRecord : EntityBase
    {
        public string Username { get; set; }
        public string Acl { get; set; }
    }
}
