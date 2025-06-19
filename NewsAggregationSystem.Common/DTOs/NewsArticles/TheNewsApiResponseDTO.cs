using System.Text.Json.Serialization;

namespace NewsAggregationSystem.Common.DTOs.NewsArticles
{
    public class TheNewsApiResponseDTO
    {
        [JsonPropertyName("meta")]
        public MetaDTO Meta { get; set; }

        [JsonPropertyName("data")]
        public List<TheNewsApiArticleDTO> Data { get; set; }
    }
}
