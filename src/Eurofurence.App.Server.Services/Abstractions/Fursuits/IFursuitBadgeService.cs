using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Fursuits;

namespace Eurofurence.App.Server.Services.Abstractions.Fursuits
{
    public interface IFursuitBadgeService
    {
        Task<Guid> UpsertFursuitBadgeAsync(FursuitBadgeRegistration registration);

        Task<Stream> GetFursuitBadgeImageAsync(Guid id);

        IQueryable<FursuitBadgeRecord> GetFursuitBadges(FursuitBadgeFilter filter = null);
    }
}