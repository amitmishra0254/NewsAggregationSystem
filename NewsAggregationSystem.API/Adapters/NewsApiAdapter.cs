using NewsAggregationSystem.API.Adapters.Interfaces;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Articles;
using NewsAggregationSystem.DAL.Repositories.NewsSources;
using NewsAggregationSystem.Service.Interfaces;
using System.Text.Json;

namespace NewsAggregationSystem.API.Adapters
{
    public class NewsApiAdapter : INewsApiAdapter
    {
        private readonly INewsSourceRepository newsSourceRepository;
        private readonly IArticleRepository articleRepository;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ITopicPredictionAdapter topicPredictionAdapter;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();
        private readonly ILogger<NewsApiAdapter> logger;

        public NewsApiAdapter(INewsSourceRepository newsSourceRepository,
            IHttpClientFactory httpClientFactory,
            ITopicPredictionAdapter topicPredictionAdapter,
            IArticleRepository articleRepository,
            ILogger<NewsApiAdapter> logger)
        {
            this.newsSourceRepository = newsSourceRepository;
            this.httpClientFactory = httpClientFactory;
            this.topicPredictionAdapter = topicPredictionAdapter;
            this.articleRepository = articleRepository;
            this.logger = logger;
        }

        public async Task<List<Article>> FetchNewsAsync(string country = "us", string category = "")
        {
            var client = httpClientFactory.CreateClient();
            var newsSourceId = (int)NewsSourcesType.NewsApi;

            logger.LogInformation(ApplicationConstants.LogMessage.FetchingNewsStarted, newsSourceId, country, category);

            var newsSource = await newsSourceRepository.GetSingleOrDefaultAsync(source => source.Id == newsSourceId);

            if (newsSource == null)
            {
                logger.LogWarning(ApplicationConstants.LogMessage.NewsSourceNotFound, newsSourceId);
                return new();
            }

            try
            {
                var url = new Uri($"{newsSource.BaseUrl}?country={country}&category={category}&apiKey={newsSource.ApiKey}");
                logger.LogInformation(ApplicationConstants.LogMessage.SendingRequestToApi, url);

                var response = await client.GetAsync(url);
                logger.LogInformation(ApplicationConstants.LogMessage.ApiResponseStatus, response.StatusCode, newsSource.Id);

                if (!response.IsSuccessStatusCode)
                {
                    throw new NewsSourceFetchFailedException(
                        newsSource.Id,
                        string.Format(ApplicationConstants.FailedToFetchNewsMessage, newsSource.Name, response.StatusCode, await response.Content.ReadAsStringAsync())
                    );
                }

                var json = await response.Content.ReadAsStringAsync();
                var deserializedArticles = JsonSerializer.Deserialize<NewsApiResponseDTO>(json) ?? new NewsApiResponseDTO();
                logger.LogInformation(ApplicationConstants.LogMessage.DeserializationCompleted, newsSource.Id);

                var articles = new List<Article>();
                foreach (var article in deserializedArticles.Articles)
                {
                    logger.LogDebug(ApplicationConstants.LogMessage.ProcessingArticleTitle, article.Title);
                    articles.Add(await ProcessArticle(article));
                }

                logger.LogInformation(ApplicationConstants.LogMessage.SavingArticlesToDb, articles.Count, newsSource.Id);
                await articleRepository.AddRangeAsync(articles);

                logger.LogInformation(ApplicationConstants.LogMessage.SourceStatusUpdated, true, newsSource.Id);
                await newsSourceRepository.ChangeNewsSourceStatus(true, newsSource);
                return articles;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ApplicationConstants.LogMessage.ExceptionWhileFetching, newsSource.Id, exception.Message);
                await newsSourceRepository.ChangeNewsSourceStatus(false, newsSource);
                throw;
            }
        }

        private async Task<Article> ProcessArticle(NewsApiArticleDTO newsApiArticleDTO)
        {
            logger.LogDebug(ApplicationConstants.LogMessage.PredictingCategoryForArticle, newsApiArticleDTO.Title);

            int categoryId = await ResolveCategory(newsApiArticleDTO);
            return new Article
            {
                Title = newsApiArticleDTO.Title,
                Description = newsApiArticleDTO.Description,
                Url = newsApiArticleDTO.Url,
                ImageUrl = newsApiArticleDTO.ImageUrl,
                PublishedAt = newsApiArticleDTO.PublishedAt,
                SourceName = newsApiArticleDTO.Source.Name,
                Author = null,
                Content = newsApiArticleDTO.Content,
                NewsCategoryId = categoryId,
                CreatedById = ApplicationConstants.SystemUserId,
                CreatedDate = dateTimeHelper.CurrentUtcDateTime
            };
        }

        private async Task<int> ResolveCategory(NewsApiArticleDTO newsApiArticleDTO)
        {
            var input = newsApiArticleDTO.Content + newsApiArticleDTO.Title;
            var resolvedCategory = await topicPredictionAdapter.PredictTopicAsync(input);
            var categoryId = await topicPredictionAdapter.ResolveCategory(resolvedCategory);

            logger.LogDebug(ApplicationConstants.LogMessage.ResolvedCategoryId, categoryId, newsApiArticleDTO.Title);

            return categoryId;
        }
    }
}

