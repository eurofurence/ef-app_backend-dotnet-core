using System;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Fursuits
{
    public interface IFursuitBadgeService
    {
        Task<Guid> UpsertFursuitBadgeAsync(FursuitBadgeRegistration registration);

        Task<byte[]> GetFursuitBadgeImageAsync(Guid id);
    }
}