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
    public class TheNewsApiAdapter : INewsApiAdapter
    {
        private readonly INewsSourceRepository newsSourceRepository;
        private readonly IArticleRepository articleRepository;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ITopicPredictionAdapter topicPredictionAdapter;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();
        private readonly ILogger<TheNewsApiAdapter> logger;

        public TheNewsApiAdapter(INewsSourceRepository newsSourceRepository,
            IHttpClientFactory httpClientFactory,
            ITopicPredictionAdapter topicPredictionAdapter,
            IArticleRepository articleRepository,
            ILogger<TheNewsApiAdapter> logger)
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
            var newsSourceId = (int)NewsSourcesType.TheNewsApi;

            logger.LogInformation(ApplicationConstants.LogMessage.FetchingNewsStarted, newsSourceId, country);

            var newsSource = await GetNewsSource(newsSourceId);
            if (newsSource == null)
            {
                return new List<Article>();
            }

            try
            {
                var url = BuildApiUrl(newsSource, country);
                var response = await MakeApiRequest(client, url, newsSource);
                var articles = await ProcessApiResponse(response, newsSource);
                await SaveArticles(articles, newsSource);
                await ChangeNewsSourceStatus(true, newsSource);
                return articles;
            }
            catch (Exception ex)
            {
                await SaveArticles(new List<Article>(), newsSource);
                await ChangeNewsSourceStatus(false, newsSource);
                logger.LogError(ex, ApplicationConstants.LogMessage.ExceptionWhileFetching, newsSourceId, ex.Message);
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

        private string BuildApiUrl(NewsSource newsSource, string country)
        {
            return $"{newsSource.BaseUrl}?{ApplicationConstants.LocaleParameter}={country}&{ApplicationConstants.LimitParameter}={ApplicationConstants.DefaultLimit}&{ApplicationConstants.ApiTokenParameter}={newsSource.ApiKey}";
        }

        private async Task<HttpResponseMessage> MakeApiRequest(HttpClient client, string url, NewsSource newsSource)
        {
            logger.LogInformation(ApplicationConstants.LogMessage.SendingRequestToApi, url);
            var response = await client.GetAsync(url);
            logger.LogInformation(ApplicationConstants.LogMessage.ApiResponseStatus, response.StatusCode, newsSource.Id);

            if (!response.IsSuccessStatusCode)
            {
                throw new NewsSourceFetchFailedException(
                    newsSource.Id,
                    string.Format(ApplicationConstants.FailedToFetchNewsMessage, newsSource.Name, response.StatusCode)
                );
            }

            return response;
        }

        private async Task<List<Article>> ProcessApiResponse(HttpResponseMessage response, NewsSource newsSource)
        {
            var json = await response.Content.ReadAsStringAsync();
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

        private async Task SaveArticles(List<Article> articles, NewsSource newsSource)
        {
            if (articles.Any())
            {
                logger.LogInformation(ApplicationConstants.LogMessage.SavingArticlesToDb, articles.Count, newsSource.Id);
                await articleRepository.AddRangeAsync(articles);
            }
        }

        private async Task ChangeNewsSourceStatus(bool isActive, NewsSource newsSource)
        {
            await newsSourceRepository.ChangeNewsSourceStatus(isActive, newsSource);
            logger.LogInformation(ApplicationConstants.LogMessage.SourceStatusUpdated, isActive, newsSource.Id);
        }

        private async Task<Article> ProcessArticle(TheNewsApiArticleDTO theNewsApiArticleDTO)
        {
            int categoryId = await ResolveCategory(theNewsApiArticleDTO);
            logger.LogDebug(ApplicationConstants.LogMessage.ProcessingArticle, theNewsApiArticleDTO.Title);

            return CreateArticle(theNewsApiArticleDTO, categoryId);
        }

        private Article CreateArticle(TheNewsApiArticleDTO theNewsApiArticleDTO, int categoryId)
        {
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
    }
}
