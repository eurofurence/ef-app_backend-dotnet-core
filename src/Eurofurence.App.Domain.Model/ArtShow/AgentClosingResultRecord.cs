using System;
using System.Collections.Generic;
using System.Text;

namespace Eurofurence.App.Domain.Model.ArtShow
{
    public class AgentClosingResultRecord : EntityBase
    {
        public string OwnerUid { get; set; }
        public int AgentBadgeNo { get; set; }
        public string AgentName { get; set; }
        public string ArtistName { get; set; }
        public int ExhibitsSold { get; set; }
        public int ExhibitsUnsold { get; set; }
        public int ExhibitsToAuction { get; set; }
        public int ExhibitsTotal => ExhibitsSold + ExhibitsUnsold + ExhibitsToAuction;
        public decimal TotalCashAmount { get; set; }

        public DateTime ImportDateTimeUtc { get; set; }
        public DateTime? NotificationDateTimeUtc { get; set; }

        public Guid? PrivateMessageId { get; set; }

        public string ImportHash { get; set; }
    }
}
