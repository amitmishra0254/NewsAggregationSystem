using NewsAggregationSystem.API.Adapters.Interfaces;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.Service.Interfaces;
using System.Text.Json;

namespace NewsAggregationSystem.API.Adapters.Strategies
{
    public class NewsApiResponseStrategy : IApiResponseStrategy
    {
        private readonly ITopicPredictionAdapter topicPredictionAdapter;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();
        private readonly ILogger<NewsApiResponseStrategy> logger;

        public NewsApiResponseStrategy(
            ITopicPredictionAdapter topicPredictionAdapter,
            ILogger<NewsApiResponseStrategy> logger)
        {
            this.topicPredictionAdapter = topicPredictionAdapter;
            this.logger = logger;
        }

        public string StrategyName => "NewsAPI";

        public async Task<List<Article>> ProcessResponse(string json, NewsSource newsSource)
        {
            var deserializedArticles = JsonSerializer.Deserialize<NewsApiResponseDTO>(json) ?? new NewsApiResponseDTO();
            logger.LogInformation(ApplicationConstants.LogMessage.DeserializationCompleted, newsSource.Id);

            var articles = new List<Article>();
            foreach (var article in deserializedArticles.Articles)
            {
                logger.LogDebug(ApplicationConstants.LogMessage.ProcessingArticleTitle, article.Title);
                articles.Add(await ProcessArticle(article));
            }

            return articles;
        }

        private async Task<Article> ProcessArticle(NewsApiArticleDTO newsApiArticleDTO)
        {
            logger.LogDebug(ApplicationConstants.LogMessage.PredictingCategoryForArticle, newsApiArticleDTO.Title);

            int categoryId = await ResolveCategory(newsApiArticleDTO.Content, newsApiArticleDTO.Title);
            return await CreateArticle(
                newsApiArticleDTO.Title,
                newsApiArticleDTO.Description,
                newsApiArticleDTO.Url,
                newsApiArticleDTO.ImageUrl,
                newsApiArticleDTO.PublishedAt,
                newsApiArticleDTO.Source.Name,
                newsApiArticleDTO.Content,
                categoryId
            );
        }

        private async Task<int> ResolveCategory(string content, string title)
        {
            var input = content + title;
            var resolvedCategory = await topicPredictionAdapter.PredictTopicAsync(input);
            var categoryId = await topicPredictionAdapter.ResolveCategory(resolvedCategory);

            logger.LogDebug(ApplicationConstants.LogMessage.ResolvedCategoryId, categoryId, title);
            return categoryId;
        }

        private async Task<Article> CreateArticle(string title, string description, string url, string imageUrl,
            DateTime publishedAt, string sourceName, string content, int categoryId)
        {
            return new Article
            {
                Title = title,
                Description = description,
                Url = url,
                ImageUrl = imageUrl,
                PublishedAt = publishedAt,
                SourceName = sourceName,
                Author = null,
                Content = content,
                NewsCategoryId = categoryId,
                CreatedById = ApplicationConstants.SystemUserId,
                CreatedDate = dateTimeHelper.CurrentUtcDateTime
            };
        }
    }
} 