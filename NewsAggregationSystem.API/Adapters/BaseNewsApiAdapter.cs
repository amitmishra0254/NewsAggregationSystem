using NewsAggregationSystem.API.Adapters.Interfaces;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Articles;
using NewsAggregationSystem.DAL.Repositories.NewsSources;
using NewsAggregationSystem.Service.Interfaces;

namespace NewsAggregationSystem.API.Adapters
{
    public abstract class BaseNewsApiAdapter : INewsApiAdapter
    {
        protected readonly INewsSourceRepository newsSourceRepository;
        protected readonly IArticleRepository articleRepository;
        protected readonly IHttpClientFactory httpClientFactory;
        protected readonly ITopicPredictionAdapter topicPredictionAdapter;
        protected readonly IApiResponseStrategy responseStrategy;
        protected readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();
        protected readonly ILogger logger;

        protected BaseNewsApiAdapter(
            INewsSourceRepository newsSourceRepository,
            IHttpClientFactory httpClientFactory,
            ITopicPredictionAdapter topicPredictionAdapter,
            IArticleRepository articleRepository,
            IApiResponseStrategy responseStrategy,
            ILogger logger)
        {
            this.newsSourceRepository = newsSourceRepository;
            this.httpClientFactory = httpClientFactory;
            this.topicPredictionAdapter = topicPredictionAdapter;
            this.articleRepository = articleRepository;
            this.responseStrategy = responseStrategy;
            this.logger = logger;
        }

        public abstract string AdapterName { get; }
        protected abstract NewsSourcesType NewsSourceType { get; }
        protected abstract string BuildApiUrl(NewsSource newsSource, string country, string category);

        public async Task<List<Article>> FetchNewsAsync(string country = "us", string category = "")
        {
            var client = httpClientFactory.CreateClient();
            var newsSourceId = (int)NewsSourceType;

            logger.LogInformation(ApplicationConstants.LogMessage.FetchingNewsStarted, newsSourceId, country, category);

            var newsSource = await GetNewsSource(newsSourceId);
            if (newsSource == null)
            {
                return new List<Article>();
            }

            try
            {
                var url = BuildApiUrl(newsSource, country, category);
                logger.LogInformation(ApplicationConstants.LogMessage.SendingRequestToApi, url);

                var response = await client.GetAsync(url);
                logger.LogInformation(ApplicationConstants.LogMessage.ApiResponseStatus, response.StatusCode, newsSource.Id);

                await ValidateResponse(response, newsSource);

                var json = await response.Content.ReadAsStringAsync();
                var articles = await responseStrategy.ProcessResponse(json, newsSource);

                await SaveArticles(articles, newsSource);
                await UpdateNewsSourceStatus(true, newsSource);

                return articles;
            }
            catch (Exception exception)
            {
                await UpdateNewsSourceStatus(false, newsSource);
                logger.LogError(exception, ApplicationConstants.LogMessage.ExceptionWhileFetching, newsSource.Id, exception.Message);
                throw;
            }
        }

        protected virtual async Task<NewsSource> GetNewsSource(int newsSourceId)
        {
            var newsSource = await newsSourceRepository.GetSingleOrDefaultAsync(source => source.Id == newsSourceId);
            if (newsSource == null)
            {
                logger.LogWarning(ApplicationConstants.LogMessage.NewsSourceNotFound, newsSourceId);
            }
            return newsSource;
        }

        protected virtual async Task ValidateResponse(HttpResponseMessage response, NewsSource newsSource)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new NewsSourceFetchFailedException(
                    newsSource.Id,
                    string.Format(ApplicationConstants.FailedToFetchNewsMessage, newsSource.Name, response.StatusCode, errorContent)
                );
            }
        }

        protected virtual async Task SaveArticles(List<Article> articles, NewsSource newsSource)
        {
            logger.LogInformation(ApplicationConstants.LogMessage.SavingArticlesToDb, articles.Count, newsSource.Id);
            await articleRepository.AddRangeAsync(articles);
        }

        protected virtual async Task UpdateNewsSourceStatus(bool isActive, NewsSource newsSource)
        {
            await newsSourceRepository.ChangeNewsSourceStatus(isActive, newsSource);
            logger.LogInformation(ApplicationConstants.LogMessage.SourceStatusUpdated, isActive, newsSource.Id);
        }


    }
} 