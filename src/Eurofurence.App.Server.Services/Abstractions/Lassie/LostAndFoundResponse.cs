using System;
using System.Text.Json.Serialization;

namespace Eurofurence.App.Server.Services.Abstractions.Lassie
{
    public class LostAndFoundResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("image")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
        
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("lost_timestamp")]
        public DateTime? LostDateTimeLocal { get; set; }

        [JsonPropertyName("found_timestamp")]
        public DateTime? FoundDateTimeLocal { get; set; }

        [JsonPropertyName("return_timestamp")]
        public DateTime? ReturnDateTimeLocal { get; set; }
    }
}
