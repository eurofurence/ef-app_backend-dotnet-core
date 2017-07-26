using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Fursuits;

namespace Eurofurence.App.Server.Services.Abstractions.Fursuits
{
    public interface IFursuitBadgeService
    {
        Task<Guid> UpsertFursuitBadgeAsync(FursuitBadgeRegistration registration);

        Task<byte[]> GetFursuitBadgeImageAsync(Guid id);

        Task<IEnumerable<FursuitBadgeRecord>> GetFursuitBadgesAsync();
    }
}