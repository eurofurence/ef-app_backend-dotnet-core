using System;

namespace Eurofurence.App.Domain.Model.ArtShow
{
    public class ItemActivityRecord : EntityBase
    {
        public enum StatusEnum
        {
            Unknown,
            Sold,
            Auction
        }

        public string OwnerUid { get; set; }
        public int ASIDNO { get; set; }
        public string ArtistName { get; set; }
        public string ArtPieceTitle { get; set; }
        public StatusEnum Status { get; set; }
        public int? FinalBidAmount { get; set; }

        public DateTime ImportDateTimeUtc { get; set; }
        public DateTime? NotificationDateTimeUtc { get; set; }

        public Guid? PrivateMessageId { get; set; }

        public string ImportHash { get; set; }
    }
}
