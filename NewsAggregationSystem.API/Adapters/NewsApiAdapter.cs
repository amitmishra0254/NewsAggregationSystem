using NewsAggregationSystem.API.Adapters.Interfaces;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Articles;
using NewsAggregationSystem.DAL.Repositories.NewsSources;
using NewsAggregationSystem.Service.Interfaces;
using System.Text.Json;

namespace NewsAggregationSystem.API.Adapters
{
    public class NewsApiAdapter : BaseNewsApiAdapter
    {
        public NewsApiAdapter(
            INewsSourceRepository newsSourceRepository,
            IHttpClientFactory httpClientFactory,
            ITopicPredictionAdapter topicPredictionAdapter,
            IArticleRepository articleRepository,
            IApiResponseStrategy responseStrategy,
            ILogger<NewsApiAdapter> logger)
            : base(newsSourceRepository, httpClientFactory, topicPredictionAdapter, articleRepository, responseStrategy, logger)
        {
        }

        public override string AdapterName => "NewsAPI";
        protected override NewsSourcesType NewsSourceType => NewsSourcesType.NewsApi;

        protected override string BuildApiUrl(NewsSource newsSource, string country, string category)
        {
            return $"{newsSource.BaseUrl}?country={country}&category={category}&apiKey={newsSource.ApiKey}";
        }
    }
}

