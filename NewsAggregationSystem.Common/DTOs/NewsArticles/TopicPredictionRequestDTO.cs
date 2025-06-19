using System.Text.Json.Serialization;

namespace NewsAggregationSystem.Common.DTOs.NewsArticles
{
    public class TopicPredictionRequestDTO
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

}
