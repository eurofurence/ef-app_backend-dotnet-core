using System;
using Eurofurence.App.Server.Services.Abstractions.Lassie;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Lassie
{
    public class LassieApiClient : ILassieApiClient
    {
        private class DataResponseWrapper<T>
        {
            public T[] Data { get; set; }
        }

        private LassieConfiguration _configuration;

        public LassieApiClient(LassieConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<LostAndFoundResponse[]> QueryLostAndFoundDbAsync(string command = "lostandfound")
        {
            var outgoingQuery = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("apikey", _configuration.ApiKey),
                new KeyValuePair<string, string>("request", "lostandfounddb"),
                new KeyValuePair<string, string>("command", command)
            };

            using var client = new HttpClient();
            var response = await client.PostAsync(_configuration.BaseApiUrl, new FormUrlEncodedContent(outgoingQuery));
            var content = await response.Content.ReadAsStringAsync();

            var dataResponse = JsonSerializer.Deserialize<DataResponseWrapper<LostAndFoundResponse>>(content);

            return dataResponse.Data ?? Array.Empty<LostAndFoundResponse>();
        }
    }
}