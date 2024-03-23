using System.Text.Json;
using System.Text.Json.Serialization;
using Eurofurence.App.Server.Web.Extensions;

namespace Eurofurence.App.Server.Web.Tests.Extensions
{
    public class JsonDateTimeConverterTests
    {
        public static TheoryData<string, DateTime> TestData =
            new()
            {
                { "{\"Data\": \"2023-11-24 00:27:24\"}", new DateTime(2023, 11, 24, 0, 27, 24, DateTimeKind.Utc)},
                { "{\"Data\": \"2023-01-01 00:00:00\"}", new DateTime(2023, 01, 01, 0, 0, 0, DateTimeKind.Utc)},
                { "{\"Data\": \"2024-02-29 12:00:00\"}", new DateTime(2024, 02, 29, 12, 0, 0, DateTimeKind.Utc)}
            };

        public static JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = null,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(), new JsonDateTimeConverter() }
        };

        public class TestDataWrapper
        {
            public DateTime Data { get; set; }
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void JsonDateTimeConverterTests_Deserialize_ValidReturnValue(string json, DateTime dateTime)
        {
            // Act
            var result = JsonSerializer.Deserialize<TestDataWrapper>(json, SerializerOptions);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dateTime, result.Data);
        }
    }
}
