using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.Dealers;

namespace Eurofurence.App.Server.Services.Dealers
{
    public class DealerApiClient : IDealerApiClient
    {
        public class DataResponseWrapper<T>
        {
            public T[] Data { get; set; }
        }

        private readonly DealerConfiguration _configuration;
        public DealerApiClient(DealerConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> DownloadDealersExportAsync(string path)
        {
            using var handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(_configuration.User, _configuration.Password);
            using var httpClient = new HttpClient(handler);
            var fileStream = await httpClient.GetStreamAsync(_configuration.Url);
            await using var outputFileStream = new FileStream(path, FileMode.Create);
            await fileStream.CopyToAsync(outputFileStream);
            return true;
        }
    }
}
