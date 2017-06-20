using System.Collections.Generic;
using System.Linq;

namespace Eurofurence.App.Domain.Model.Sync
{
    public static class SyncExtensions
    {
        public static DeltaResponse<T> ToDeltaResponse<T>(this IEnumerable<T> entities) where T : EntityBase
        {
            var enumerable = entities as IList<T> ?? entities.ToList();

            var response = new DeltaResponse<T>
            {
                ChangedEntities = enumerable.Where(a => a.IsDeleted == 0).ToArray(),
                DeletedEntities = enumerable.Where(a => a.IsDeleted == 1).Select(a => a.Id).ToArray()
            };

            return response;
        }
    }
}