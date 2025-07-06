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

        public async Task<List<Article>> FetchNewsAsync(string country = ApplicationConstants.DefaultCountry, string category = ApplicationConstants.DefaultCategory)
        {
            var client = httpClientFactory.CreateClient();
            var newsSourceId = (int)NewsSourcesType.NewsApi;

            logger.LogInformation(ApplicationConstants.LogMessage.FetchingNewsStarted, newsSourceId, country, category);

            var newsSource = await GetNewsSource(newsSourceId);
            if (newsSource == null)
            {
                return new List<Article>();
            }

            try
            {
                var url = BuildApiUrl(newsSource, country, category);
                var response = await MakeApiRequest(client, url, newsSource);
                var articles = await ProcessApiResponse(response, newsSource);
                await SaveArticles(articles, newsSource);
                await UpdateNewsSourceStatus(newsSource, true);

                return articles;
            }
            catch (Exception exception)
            {
                await SaveArticles(new List<Article>(), newsSource);
                await UpdateNewsSourceStatus(newsSource, false);
                logger.LogError(exception, ApplicationConstants.LogMessage.ExceptionWhileFetching, newsSource.Id, exception.Message);
                throw;
            }
        }

        private async Task<NewsSource> GetNewsSource(int newsSourceId)
        {
            var newsSource = await newsSourceRepository.GetSingleOrDefaultAsync(source => source.Id == newsSourceId);
            if (newsSource == null)
            {
                logger.LogWarning(ApplicationConstants.LogMessage.NewsSourceNotFound, newsSourceId);
            }
            return newsSource;
        }

        private string BuildApiUrl(NewsSource newsSource, string country, string category)
        {
            return $"{newsSource.BaseUrl}?{ApplicationConstants.CountryParameter}={country}&{ApplicationConstants.CategoryParameter}={category}&{ApplicationConstants.ApiKeyParameter}={newsSource.ApiKey}";
        }

        private async Task<HttpResponseMessage> MakeApiRequest(HttpClient client, string url, NewsSource newsSource)
        {
            logger.LogInformation(ApplicationConstants.LogMessage.SendingRequestToApi, url);
            var response = await client.GetAsync(url);
            logger.LogInformation(ApplicationConstants.LogMessage.ApiResponseStatus, response.StatusCode, newsSource.Id);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new NewsSourceFetchFailedException(
                    newsSource.Id,
                    string.Format(ApplicationConstants.FailedToFetchNewsMessage, newsSource.Name, response.StatusCode, errorContent)
                );
            }

            return response;
        }

        private async Task<List<Article>> ProcessApiResponse(HttpResponseMessage response, NewsSource newsSource)
        {
            var json = await response.Content.ReadAsStringAsync();
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

        private async Task SaveArticles(List<Article> articles, NewsSource newsSource)
        {
            if (articles.Any())
            {
                logger.LogInformation(ApplicationConstants.LogMessage.SavingArticlesToDb, articles.Count, newsSource.Id);
                await articleRepository.AddRangeAsync(articles);
            }
        }

        private async Task UpdateNewsSourceStatus(NewsSource newsSource, bool isActive)
        {
            await newsSourceRepository.ChangeNewsSourceStatus(isActive, newsSource);
            logger.LogInformation(ApplicationConstants.LogMessage.SourceStatusUpdated, isActive, newsSource.Id);
        }

        private async Task<Article> ProcessArticle(NewsApiArticleDTO newsApiArticleDTO)
        {
            logger.LogDebug(ApplicationConstants.LogMessage.PredictingCategoryForArticle, newsApiArticleDTO.Title);

            int categoryId = await ResolveCategory(newsApiArticleDTO);
            return CreateArticle(newsApiArticleDTO, categoryId);
        }

        private Article CreateArticle(NewsApiArticleDTO newsApiArticleDTO, int categoryId)
        {
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

