using System;

namespace Eurofurence.App.Domain.Model.Sync
{
    public class EntityStorageInfoRecord : EntityBase
    {
        public string EntityType { get; set; }
        public DateTime DeltaStartDateTimeUtc { get; set; }
    }
}