using NewsAggregationSystem.DAL.Entities;
using System.Text.Json;
using System.Text;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.Common.Constants;

namespace NewsAggregationSystem.API.Services.NewsArticleClassifier
{
    public class TopicPredictionAdapter : ITopicPredictionAdapter
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TopicPredictionAdapter(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> PredictTopicAsync(string text)
        {
            var client = _httpClientFactory.CreateClient();

            var requestBody = new TopicPredictionRequestDTO { Text = text };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(ApplicationConstants.TopicPredictionUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                return "";
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TopicPredictionResponseDTO>(responseContent);

            return result?.Topic ?? "All";
        }
    }
}
