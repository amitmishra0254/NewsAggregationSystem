using NewsAggregationSystem.DAL.Entities;

namespace NewsAggregationSystem.API.Adapters.Interfaces
{
    public interface IApiResponseStrategy
    {
        Task<List<Article>> ProcessResponse(string json, NewsSource newsSource);
        string StrategyName { get; }
    }
} 