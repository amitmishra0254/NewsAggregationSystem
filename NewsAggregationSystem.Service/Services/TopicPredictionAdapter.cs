using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.DAL.Repositories.NewsCategories;
using NewsAggregationSystem.Service.Interfaces;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace NewsAggregationSystem.Service.Services
{
    public class TopicPredictionAdapter : ITopicPredictionAdapter
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly INewsCategoryRepository newsCategoryRepository;
        private readonly INewsCategoryService newsCategoryService;
        private readonly INotificationPreferenceService notificationPreferenceService;

        public TopicPredictionAdapter(IHttpClientFactory httpClientFactory, INewsCategoryRepository newsCategoryRepository, INewsCategoryService newsCategoryService, INotificationPreferenceService notificationPreferenceService)
        {
            this.httpClientFactory = httpClientFactory;
            this.newsCategoryRepository = newsCategoryRepository;
            this.newsCategoryService = newsCategoryService;
            this.notificationPreferenceService = notificationPreferenceService;
        }

        public async Task<string> PredictTopicAsync(string text)
        {
            var client = httpClientFactory.CreateClient();

            var requestBody = new TopicPredictionRequestDTO { Text = text };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, ApplicationConstants.JsonContentType);

            var response = await client.PostAsync(ApplicationConstants.TopicPredictionUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                return CategoryType.All.ToString();
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TopicPredictionResponseDTO>(responseContent);

            return result?.Topic ?? CategoryType.All.ToString();
        }

        public async Task<int> ResolveCategory(string resolvedCategory)
        {
            var existingCategory = await newsCategoryRepository.GetSingleOrDefaultAsync(
                    category => category.Name.ToLower() == resolvedCategory.ToLower());

            if (existingCategory != null)
                return existingCategory.Id;

            resolvedCategory = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resolvedCategory.ToLower());

            var newCategoryId = await newsCategoryService.AddNewsCategory(resolvedCategory, ApplicationConstants.SystemUserId);
            await notificationPreferenceService.AddNotificationPreferencesPerCategory(newCategoryId);
            return newCategoryId;
        }
    }
}
