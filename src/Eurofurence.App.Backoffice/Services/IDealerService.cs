using Eurofurence.App.Domain.Model.Dealers;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IDealerService
    {
        public Task<DealerRecord[]> GetDealersAsync();
    }
}
