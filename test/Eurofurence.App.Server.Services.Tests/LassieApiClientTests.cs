using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Eurofurence.App.Server.Services.Abstractions.Lassie;
using Eurofurence.App.Server.Web.Extensions;
using static Eurofurence.App.Server.Services.Lassie.LassieApiClient;
using System;

namespace Eurofurence.App.Server.Services.Tests
{
    public class LassieApiClientTests
    {
        public static JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = null,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(), new JsonDateTimeConverter() }
        };

        [Fact]
        public void LassieApiClient_DeserializeLostAndFoundResponse_ValidReturnValue()
        {
            // Arrange
            string _testData = @"
            {
              ""data"": [
                {
                  ""id"": 1925,
                  ""image"": null,
                  ""thumb"": null,
                  ""title"": ""Image"",
                  ""description"": ""invisible, watercolour, found in Schroedinger's box"",
                  ""status"": ""F"",
                  ""lost_timestamp"": null,
                  ""found_timestamp"": ""2023-11-24 00:27:24"",
                  ""return_timestamp"": null
                },
                {
                  ""id"": 1921,
                  ""image"": ""https://api.lassie.furcom.org/images/lostandfound_db/fe3c6670a33d8f4199ffa95a5c23b622.png"",
                  ""thumb"": ""https://api.lassie.furcom.org/images/lostandfound_db/thumbnail/fe3c6670a33d8f4199ffa95a5c23b622.png"",
                  ""title"": ""Sense of Taste"",
                  ""description"": ""lost in Hotel Restaurant"",
                  ""status"": ""L"",
                  ""lost_timestamp"": ""2023-11-01 16:13:02"",
                  ""found_timestamp"": null,
                  ""return_timestamp"": null
                },
                {
                  ""id"": 1920,
                  ""image"": ""https://api.lassie.furcom.org/images/lostandfound_db/bc7c343fb3807d4d41b7e04fa909aad5.png"",
                  ""thumb"": ""https://api.lassie.furcom.org/images/lostandfound_db/thumbnail/bc7c343fb3807d4d41b7e04fa909aad5.png"",
                  ""title"": ""Vape"",
                  ""description"": ""black / rainbow, \""GEEKVAPE\"", found at Open Stage"",
                  ""status"": ""F"",
                  ""lost_timestamp"": null,
                  ""found_timestamp"": ""2023-09-30 23:52:38"",
                  ""return_timestamp"": null
                }
              ]
            }
            ";

            var expectedLength = 3;

            // Act
            var result =
                JsonSerializer.Deserialize<DataResponseWrapper<LostAndFoundResponse>>(_testData, SerializerOptions);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(result.Data.Length, expectedLength);
            Assert.Equal(new DateTime(2023, 11, 24, 0, 27, 24, DateTimeKind.Utc), result.Data[0].FoundDateTimeLocal);
        }
    }
}