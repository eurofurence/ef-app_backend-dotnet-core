using Newtonsoft.Json;
using System;

namespace Eurofurence.App.Server.Services.Abstractions.Lassie
{
    public class LostAndFoundResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("image")]
        public string ImageUrl { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("lost_timestamp")]
        public DateTime? LostDateTimeLocal { get; set; }

        [JsonProperty("found_timestamp")]
        public DateTime? FoundDateTimeLocal { get; set; }

        [JsonProperty("return_timestamp")]
        public DateTime? ReturnDateTimeLocal { get; set; }
    }
}
