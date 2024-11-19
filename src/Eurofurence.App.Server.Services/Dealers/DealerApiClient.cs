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
        private readonly DealersOptions _dealersOptions;
        public DealerApiClient(IOptions<DealersOptions> dealersOptions)
        {
            _dealersOptions = dealersOptions.Value;
        }

        public async Task<bool> DownloadDealersExportAsync(string path)
        {
            using var handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(_dealersOptions.User, _dealersOptions.Password);
            using var httpClient = new HttpClient(handler);
            var fileStream = await httpClient.GetStreamAsync(_dealersOptions.Url);
            await using var outputFileStream = new FileStream(path, FileMode.Create);
            await fileStream.CopyToAsync(outputFileStream);
            return true;
        }
    }
}