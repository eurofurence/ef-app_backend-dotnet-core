using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Announcements;

namespace Eurofurence.App.Server.Services.Abstractions.Announcements
{
    public interface IAnnouncementService :
        IEntityServiceOperations<AnnouncementRecord, AnnouncementResponse>,
        IPatchOperationProcessor<AnnouncementRecord>
    {
        /// <summary>
        /// Fetches all announcements regardless of whether the user is a member of the group or not.
        /// </summary>
        /// <returns>Collection of all found records.</returns>
        public IQueryable<AnnouncementRecord> FetchAll();

        /// <summary>
        /// Finds an announcement by its id regardless of whether the user is a member of the group or not.
        /// </summary>
        /// <param name="id">The id to look up.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A task with the record. Can be null.</returns>
        public Task<AnnouncementRecord> FindOneInAllAsync(Guid id, CancellationToken cancellationToken = default);
    }
}