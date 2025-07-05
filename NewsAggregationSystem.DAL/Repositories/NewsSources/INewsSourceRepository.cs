using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;

namespace NewsAggregationSystem.DAL.Repositories.NewsSources
{
    public interface INewsSourceRepository : IRepositoryBase<NewsSource>
    {
        Task<int> ChangeNewsSourceStatus(bool newsSourceStatus, NewsSource newsSource);
    }
}
