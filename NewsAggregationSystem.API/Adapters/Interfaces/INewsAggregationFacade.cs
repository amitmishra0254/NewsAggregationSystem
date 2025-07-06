using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.DAL.Entities;

namespace NewsAggregationSystem.API.Adapters.Interfaces
{
    public interface INewsAggregationFacade
    {
        Task<List<Article>> FetchNewsFromSourceAsync(NewsSourcesType newsSourceType, string country = "us", string category = "");
    }
} 