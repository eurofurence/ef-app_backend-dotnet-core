using System;
using System.Collections.Generic;
using System.Text;

namespace Eurofurence.App.Server.Services.Abstractions.Communication
{
    public class PrivateMessageStatus
    {
        public Guid Id { get; set; }
        public string RecipientRegSysId { get; set; }
        public string RecipientIdentityId { get; set; }

        public DateTime CreatedDateTimeUtc { get; set; }

        public DateTime? ReceivedDateTimeUtc { get; set; }

        public DateTime? ReadDateTimeUtc { get; set; }
    }
}
