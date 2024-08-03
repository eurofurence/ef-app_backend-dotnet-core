using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Dealers
{
    public interface IDealerApiClient
    {
        public Task<bool> DownloadDealersExportAsync(string path);
    }
}
