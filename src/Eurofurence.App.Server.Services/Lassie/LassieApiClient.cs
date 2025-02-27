﻿using System;
using Eurofurence.App.Server.Services.Abstractions.Lassie;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Services.Lassie
{
    public class LassieApiClient : ILassieApiClient
    {
        public class DataResponseWrapper<T>
        {
            public T[] Data { get; set; }
        }

        private LassieOptions _lassieOptions;
        private JsonSerializerOptions _serializerOptions;

        public LassieApiClient(IOptions<LassieOptions> lassieOptions, JsonSerializerOptions serializerOptions)
        {
            _lassieOptions = lassieOptions.Value;
            _serializerOptions = serializerOptions;
        }

        public async Task<LostAndFoundResponse[]> QueryLostAndFoundDbAsync(string command = "lostandfound")
        {
            var outgoingQuery = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("apikey", _lassieOptions.ApiKey),
                new KeyValuePair<string, string>("request", "lostandfounddb"),
                new KeyValuePair<string, string>("command", command)
            };

            using var client = new HttpClient();
            var response = await client.PostAsync(_lassieOptions.BaseApiUrl, new FormUrlEncodedContent(outgoingQuery));
            var content = await response.Content.ReadAsStringAsync();

            var dataResponse = DeserializeLostAndFoundResponse(content);

            return dataResponse.Data ?? Array.Empty<LostAndFoundResponse>();
        }

        public DataResponseWrapper<LostAndFoundResponse> DeserializeLostAndFoundResponse(string content)
        {
            return JsonSerializer.Deserialize<DataResponseWrapper<LostAndFoundResponse>>(content, _serializerOptions);
        }
    }
}