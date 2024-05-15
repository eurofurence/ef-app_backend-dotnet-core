using System;

namespace Eurofurence.App.Domain.Model.CollectionGame
{
    public class CollectionEntryRecord : EntityBase
    {
        public string PlayerParticipationId { get; set; }
        public Guid FursuitParticipationId { get; set; }
        public DateTime EventDateTimeUtc { get; set; }
    }
}
