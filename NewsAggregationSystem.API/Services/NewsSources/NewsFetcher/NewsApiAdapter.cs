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
    public class NewsApiAdapter : INewsApiAdapter
    {
        private readonly INewsSourceRepository newsSourceRepository;
        private readonly INewsCategoryRepository newsCategoryRepository;
        private readonly IArticleRepository articleRepository;
        private readonly INotificationPreferenceService notificationPreferenceService;
        private readonly INewsCategoryService newsCategoryService;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ITopicPredictionAdapter topicPredictionAdapter;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();

        public NewsApiAdapter(INewsSourceRepository newsSourceRepository,
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
            var newsSource = await newsSourceRepository.GetSingleOrDefaultAsync(source => source.Id == (int)NewsSourcesType.NewsApi && source.IsActive);
            if (newsSource == null)
            {
                return new List<Article>();
            }
            var url = new Uri($"{newsSource.BaseUrl}?country={country}&category={category}&apiKey={newsSource.ApiKey}");

            try
            {
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new NewsSourceFetchFailedException(
                        newsSource.Id,
                        string.Format(ApplicationConstants.FailedToFetchNewsMessage, newsSource.Name, response.StatusCode, await response.Content.ReadAsStringAsync())
                    );
                }

                var json = await response.Content.ReadAsStringAsync();
                var deserializedArticles = JsonSerializer.Deserialize<NewsApiResponseDTO>(json) ?? new NewsApiResponseDTO();

                var articles = new List<Article>();

                foreach (var deserializedArticle in deserializedArticles.Articles)
                {
                    var predictedTopic = await topicPredictionAdapter.PredictTopicAsync(deserializedArticle.Content + deserializedArticle.Title);
                    var topicInTitleCase = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(predictedTopic);
                    var matchedCategory = await newsCategoryRepository.GetSingleOrDefaultAsync(c => c.Name.ToLower() == predictedTopic.ToLower());

                    int categoryId;
                    if (matchedCategory == null)
                    {
                        categoryId = await newsCategoryService.AddNewsCategory(topicInTitleCase, ApplicationConstants.SystemUserId);
                        await notificationPreferenceService.AddNotificationPreferencesPerCategory(categoryId);
                    }
                    else
                    {
                        categoryId = matchedCategory.Id;
                    }

                    var article = new Article
                    {
                        Title = deserializedArticle.Title,
                        Description = deserializedArticle.Description,
                        Url = deserializedArticle.Url,
                        ImageUrl = deserializedArticle.ImageUrl,
                        PublishedAt = deserializedArticle.PublishedAt,
                        SourceName = deserializedArticle.Source?.Name,
                        Author = deserializedArticle.Author,
                        Content = deserializedArticle.Content,
                        NewsCategoryId = categoryId,
                        CreatedById = ApplicationConstants.SystemUserId,
                        CreatedDate = dateTimeHelper.CurrentUtcDateTime
                    };

                    articles.Add(article);
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
    }
}

