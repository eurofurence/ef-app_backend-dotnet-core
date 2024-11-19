using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Services.Dealers
{
    public class DealerApiClient : IDealerApiClient
    {
        private readonly DealerOptions _dealerOptions;
        public DealerApiClient(IOptions<DealerOptions> dealerOptions)
        {
            _dealerOptions = dealerOptions.Value;
        }

        public async Task<bool> DownloadDealersExportAsync(string path)
        {
            using var handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(_dealerOptions.User, _dealerOptions.Password);
            using var httpClient = new HttpClient(handler);
            var fileStream = await httpClient.GetStreamAsync(_dealerOptions.Url);
            await using var outputFileStream = new FileStream(path, FileMode.Create);
            await fileStream.CopyToAsync(outputFileStream);
            return true;
        }
    }
}