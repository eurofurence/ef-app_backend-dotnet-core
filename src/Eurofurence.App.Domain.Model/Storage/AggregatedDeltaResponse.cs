using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eurofurence.App.Domain.Model.Sync
{
    public class AggregatedDeltaResponse
    {
        public DateTime Since { get; set; }
    }


    public class DeltaResponse<T> where T: EntityBase
    {
        public DateTime StorageLastChangeDateTimeUtc { get; set; }
        public DateTime StorageDeltaStartChangeDateTimeUtc { get; set; }

        public bool RemoveAllBeforeInsert { get; set; }

        public T[] ChangedEntities { get; set; }
        public Guid[] DeletedEntities { get; set; }
    }
}
