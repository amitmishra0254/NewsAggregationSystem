using NewsAggregationSystem.DAL.DbContexts;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;

namespace NewsAggregationSystem.DAL.Repositories.NewsSources
{
    public class NewsSourceRepository : RepositoryBase<NewsSource>, INewsSourceRepository
    {
        private readonly NewsAggregationSystemContext context;
        public NewsSourceRepository(NewsAggregationSystemContext context) : base(context)
        {
            this.context = context;
        }
    }
}
