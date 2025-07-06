using NewsAggregationSystem.API.Adapters.Interfaces;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.Service.Interfaces;
using System.Text.Json;

namespace NewsAggregationSystem.API.Adapters.Strategies
{
    public class TheNewsApiResponseStrategy : IApiResponseStrategy
    {
        private readonly ITopicPredictionAdapter topicPredictionAdapter;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();
        private readonly ILogger<TheNewsApiResponseStrategy> logger;

        public TheNewsApiResponseStrategy(
            ITopicPredictionAdapter topicPredictionAdapter,
            ILogger<TheNewsApiResponseStrategy> logger)
        {
            this.topicPredictionAdapter = topicPredictionAdapter;
            this.logger = logger;
        }

        public string StrategyName => "TheNewsAPI";

        public async Task<List<Article>> ProcessResponse(string json, NewsSource newsSource)
        {
            var deserializedArticles = JsonSerializer.Deserialize<TheNewsApiResponseDTO>(json) ?? new TheNewsApiResponseDTO();
            logger.LogInformation(ApplicationConstants.LogMessage.DeserializationCompleted, newsSource.Id);

            var articles = new List<Article>();
            foreach (var article in deserializedArticles.Data)
            {
                logger.LogDebug(ApplicationConstants.LogMessage.ProcessingArticleTitle, article.Title);
                articles.Add(await ProcessArticle(article));
            }

            return articles;
        }

        private async Task<Article> ProcessArticle(TheNewsApiArticleDTO theNewsApiArticleDTO)
        {
            int categoryId = await ResolveCategory(theNewsApiArticleDTO);
            logger.LogDebug(ApplicationConstants.LogMessage.ProcessingArticle, theNewsApiArticleDTO.Title);

            return await CreateArticle(
                theNewsApiArticleDTO.Title,
                theNewsApiArticleDTO.Description,
                theNewsApiArticleDTO.Url,
                theNewsApiArticleDTO.ImageUrl,
                theNewsApiArticleDTO.PublishedAt,
                theNewsApiArticleDTO.Source,
                theNewsApiArticleDTO.Snippet,
                categoryId
            );
        }

        private async Task<int> ResolveCategory(TheNewsApiArticleDTO theNewsApiArticleDTO)
        {
            string resolvedCategory = theNewsApiArticleDTO.Categories?.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c))?.Trim();

            if (string.IsNullOrWhiteSpace(resolvedCategory))
            {
                logger.LogDebug(ApplicationConstants.LogMessage.PredictingCategory, theNewsApiArticleDTO.Title);
                resolvedCategory = await topicPredictionAdapter.PredictTopicAsync(theNewsApiArticleDTO.Snippet + theNewsApiArticleDTO.Title);
            }

            int categoryId = await topicPredictionAdapter.ResolveCategory(resolvedCategory);

            logger.LogDebug(ApplicationConstants.LogMessage.ResolvedCategory, categoryId, theNewsApiArticleDTO.Title);

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