using System.Text.Json.Serialization;

namespace NewsAggregationSystem.Common.DTOs.NewsArticles
{
    public class MetaDTO
    {
        [JsonPropertyName("found")]
        public int Found { get; set; }

        [JsonPropertyName("returned")]
        public int Returned { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }
    }
}
