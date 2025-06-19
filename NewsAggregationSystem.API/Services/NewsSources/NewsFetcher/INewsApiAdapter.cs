using NewsAggregationSystem.DAL.Entities;

namespace NewsAggregationSystem.API.Services.NewsSources.NewsFetcher
{
    public interface INewsApiAdapter
    {
        Task<List<Article>> FetchNewsAsync(string country = "us", string category = "");
    }
}
