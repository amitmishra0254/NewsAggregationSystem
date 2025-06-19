using NewsAggregationSystem.API.Services.NewsArticleClassifier;
using NewsAggregationSystem.API.Services.NewsCategories;
using NewsAggregationSystem.API.Services.NotificationPreferences;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Articles;
using NewsAggregationSystem.DAL.Repositories.NewsCategories;
using NewsAggregationSystem.DAL.Repositories.NewsSources;
using System.Globalization;
using System.Text.Json;

namespace NewsAggregationSystem.API.Services.NewsSources.NewsFetcher
{
    public class TheNewsApiAdapter : INewsApiAdapter
    {
        private readonly INewsSourceRepository newsSourceRepository;
        private readonly INewsCategoryRepository newsCategoryRepository;
        private readonly IArticleRepository articleRepository;
        private readonly INotificationPreferenceService notificationPreferenceService;
        private readonly INewsCategoryService newsCategoryService;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ITopicPredictionAdapter topicPredictionAdapter;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();

        public TheNewsApiAdapter(INewsSourceRepository newsSourceRepository,
            IHttpClientFactory httpClientFactory,
            ITopicPredictionAdapter topicPredictionAdapter,
            INewsCategoryRepository newsCategoryRepository,
            INotificationPreferenceService notificationPreferenceService,
            INewsCategoryService newsCategoryService,
            IArticleRepository articleRepository)
        {
            this.newsSourceRepository = newsSourceRepository;
            this.httpClientFactory = httpClientFactory;
            this.topicPredictionAdapter = topicPredictionAdapter;
            this.newsCategoryRepository = newsCategoryRepository;
            this.notificationPreferenceService = notificationPreferenceService;
            this.newsCategoryService = newsCategoryService;
            this.articleRepository = articleRepository;
        }
        public async Task<List<Article>> FetchNewsAsync(string country = "us", string category = "")
        {
            var client = httpClientFactory.CreateClient();
            var newsSource = await newsSourceRepository.GetSingleOrDefaultAsync(source => source.Id == (int)NewsSourcesType.TheNewsApi && source.IsActive);
            if (newsSource == null)
            {
                return new List<Article>();
            }
            var url = new Uri($"{newsSource.BaseUrl}?locale={country}&limit=3&api_token={newsSource.ApiKey}");

            try
            {
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new NewsSourceFetchFailedException(
                    newsSource.Id,
                        string.Format(ApplicationConstants.FailedToFetchNewsMessage, newsSource.Name, response.StatusCode)
                    );
                }

                var json = await response.Content.ReadAsStringAsync();
                var deserializedArticles = JsonSerializer.Deserialize<TheNewsApiResponseDTO>(json) ?? new TheNewsApiResponseDTO();
                var articles = new List<Article>();
                foreach (var article in deserializedArticles.Data)
                {
                    int categoryId = await ResolveCategoryAsync(article);
                    articles.Add(new Article
                    {
                        Title = article.Title,
                        Description = article.Description,
                        Url = article.Url,
                        ImageUrl = article.ImageUrl,
                        PublishedAt = article.PublishedAt,
                        SourceName = article.Source,
                        Author = null,
                        Content = article.Snippet,
                        NewsCategoryId = categoryId,
                        CreatedById = ApplicationConstants.SystemUserId,
                        CreatedDate = dateTimeHelper.CurrentUtcDateTime
                    });
                }
                await articleRepository.AddRangeAsync(articles);
                return articles;
            }
            catch (Exception ex)
            {
                newsSource.IsActive = false;
                await newsSourceRepository.UpdateAsync(newsSource);
                throw ex;
            }
        }

        private async Task<Article> ProcessArticleAsync(TheNewsApiArticleDTO theNewsApiArticleDTO)
        {
            int categoryId = await ResolveCategoryAsync(theNewsApiArticleDTO);
            return new Article
            {
                Title = theNewsApiArticleDTO.Title,
                Description = theNewsApiArticleDTO.Description,
                Url = theNewsApiArticleDTO.Url,
                ImageUrl = theNewsApiArticleDTO.ImageUrl,
                PublishedAt = theNewsApiArticleDTO.PublishedAt,
                SourceName = theNewsApiArticleDTO.Source,
                Author = null,
                Content = theNewsApiArticleDTO.Snippet,
                NewsCategoryId = categoryId,
                CreatedById = ApplicationConstants.SystemUserId,
                CreatedDate = dateTimeHelper.CurrentUtcDateTime
            };
        }

        private async Task<int> ResolveCategoryAsync(TheNewsApiArticleDTO theNewsApiArticleDTO)
        {
            string resolvedCategory = theNewsApiArticleDTO.Categories?.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c))?.Trim();

            if (!string.IsNullOrEmpty(resolvedCategory))
            {
                resolvedCategory = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(resolvedCategory.ToLower());

                var existingCategory = await newsCategoryRepository.GetSingleOrDefaultAsync(
                    c => c.Name.ToLower() == resolvedCategory.ToLower());

                if (existingCategory != null)
                    return existingCategory.Id;

                var newCategoryId = await newsCategoryService.AddNewsCategory(resolvedCategory, ApplicationConstants.SystemUserId);
                await notificationPreferenceService.AddNotificationPreferencesPerCategory(newCategoryId);
                return newCategoryId;
            }
            else
            {
                var predictedCategory = await topicPredictionAdapter.PredictTopicAsync(theNewsApiArticleDTO.Snippet + theNewsApiArticleDTO.Title);
                var normalizedPrediction = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(predictedCategory.ToLower());

                var matchedCategory = await newsCategoryRepository.GetSingleOrDefaultAsync(
                    c => c.Name.ToLower() == normalizedPrediction.ToLower());

                if (matchedCategory != null)
                    return matchedCategory.Id;

                var newCategoryId = await newsCategoryService.AddNewsCategory(normalizedPrediction, ApplicationConstants.SystemUserId);
                await notificationPreferenceService.AddNotificationPreferencesPerCategory(newCategoryId);
                return newCategoryId;
            }
        }
    }
}
