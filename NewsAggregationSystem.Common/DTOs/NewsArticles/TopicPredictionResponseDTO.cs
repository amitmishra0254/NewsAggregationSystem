using System.Text.Json.Serialization;

namespace NewsAggregationSystem.Common.DTOs.NewsArticles
{
    public class TopicPredictionResponseDTO
    {
        [JsonPropertyName("topic")]
        public string Topic { get; set; }
    }

}
