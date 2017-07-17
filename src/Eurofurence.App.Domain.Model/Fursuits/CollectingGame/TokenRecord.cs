using System;

namespace Eurofurence.App.Domain.Model.Fursuits.CollectingGame
{
    public class TokenRecord : EntityBase
    {
        public string Value { get; set; }
        public bool IsLinked { get; set; }
        public DateTime? LinkDateTimeUtc { get; set; }
        public Guid? LinkedFursuitParticipantUid { get; set; }
    }
}
