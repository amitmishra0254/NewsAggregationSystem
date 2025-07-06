using NewsAggregationSystem.API.Adapters.Interfaces;
using NewsAggregationSystem.API.Adapters.Strategies;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.DAL.Repositories.Articles;
using NewsAggregationSystem.DAL.Repositories.NewsSources;
using NewsAggregationSystem.Service.Interfaces;

namespace NewsAggregationSystem.API.Adapters
{
    public class NewsApiAdapterFactory : INewsApiAdapterFactory
    {
        private readonly INewsSourceRepository newsSourceRepository;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ITopicPredictionAdapter topicPredictionAdapter;
        private readonly IArticleRepository articleRepository;
        private readonly ILogger<NewsApiAdapter> newsApiLogger;
        private readonly ILogger<TheNewsApiAdapter> theNewsApiLogger;
        private readonly NewsApiResponseStrategy newsApiResponseStrategy;
        private readonly TheNewsApiResponseStrategy theNewsApiResponseStrategy;

        public NewsApiAdapterFactory(
            INewsSourceRepository newsSourceRepository,
            IHttpClientFactory httpClientFactory,
            ITopicPredictionAdapter topicPredictionAdapter,
            IArticleRepository articleRepository,
            ILogger<NewsApiAdapter> newsApiLogger,
            ILogger<TheNewsApiAdapter> theNewsApiLogger,
            NewsApiResponseStrategy newsApiResponseStrategy,
            TheNewsApiResponseStrategy theNewsApiResponseStrategy)
        {
            this.newsSourceRepository = newsSourceRepository;
            this.httpClientFactory = httpClientFactory;
            this.topicPredictionAdapter = topicPredictionAdapter;
            this.articleRepository = articleRepository;
            this.newsApiLogger = newsApiLogger;
            this.theNewsApiLogger = theNewsApiLogger;
            this.newsApiResponseStrategy = newsApiResponseStrategy;
            this.theNewsApiResponseStrategy = theNewsApiResponseStrategy;
        }

        public INewsApiAdapter CreateAdapter(NewsSourcesType newsSourceType)
        {
            return newsSourceType switch
            {
                NewsSourcesType.NewsApi => new NewsApiAdapter(
                    newsSourceRepository, httpClientFactory, topicPredictionAdapter, articleRepository, newsApiResponseStrategy, newsApiLogger),
                
                NewsSourcesType.TheNewsApi => new TheNewsApiAdapter(
                    newsSourceRepository, httpClientFactory, topicPredictionAdapter, articleRepository, theNewsApiResponseStrategy, theNewsApiLogger),
                
                _ => throw new NotFoundException($"No adapter found for news source type: {newsSourceType}")
            };
        }

        public INewsApiAdapter CreateAdapter(string adapterName)
        {
            return adapterName.ToLowerInvariant() switch
            {
                "newsapi" or "news-api" => CreateAdapter(NewsSourcesType.NewsApi),
                "thenewsapi" or "the-news-api" => CreateAdapter(NewsSourcesType.TheNewsApi),
                _ => throw new NotFoundException($"No adapter found with name: {adapterName}")
            };
        }
    }
} 