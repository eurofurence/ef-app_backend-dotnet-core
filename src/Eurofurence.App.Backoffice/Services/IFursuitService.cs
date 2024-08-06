using Eurofurence.App.Domain.Model.Fursuits;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IFursuitService
    {
        public Task<FursuitBadgeRecord[]> GetFursuitBadgesAsync();
    }
}
