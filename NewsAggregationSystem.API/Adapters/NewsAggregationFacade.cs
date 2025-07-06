using NewsAggregationSystem.API.Adapters.Interfaces;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.DAL.Entities;

namespace NewsAggregationSystem.API.Adapters
{
    public class NewsAggregationFacade : INewsAggregationFacade
    {
        private readonly INewsApiAdapterFactory adapterFactory;
        private readonly ILogger<NewsAggregationFacade> logger;

        public NewsAggregationFacade(
            INewsApiAdapterFactory adapterFactory,
            ILogger<NewsAggregationFacade> logger)
        {
            this.adapterFactory = adapterFactory;
            this.logger = logger;
        }

        public async Task<List<Article>> FetchNewsFromSourceAsync(NewsSourcesType newsSourceType, string country = "us", string category = "")
        {
            try
            {
                logger.LogInformation(ApplicationConstants.LogMessage.FetchingNewsStarted, (int)newsSourceType, country, category);
                
                var adapter = adapterFactory.CreateAdapter(newsSourceType);
                var articles = await adapter.FetchNewsAsync(country, category);
                
                logger.LogInformation(ApplicationConstants.LogMessage.FetchingNewsCompleted, articles.Count, adapter.AdapterName);
                return articles;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ApplicationConstants.LogMessage.ExceptionWhileFetching, (int)newsSourceType, ex.Message);
                throw;
            }
        }
    }
} 