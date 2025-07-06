using NewsAggregationSystem.DAL.Entities;

namespace NewsAggregationSystem.API.Adapters.Interfaces
{
    public interface INewsApiAdapter
    {
        Task<List<Article>> FetchNewsAsync(string country = "us", string category = "");
        string AdapterName { get; }
    }
}
