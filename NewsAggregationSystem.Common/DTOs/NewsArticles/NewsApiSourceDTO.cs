using System.Text.Json.Serialization;

namespace NewsAggregationSystem.Common.DTOs.NewsArticles
{
    public class NewsApiSourceDTO
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
