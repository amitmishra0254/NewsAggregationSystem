using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.DbContexts;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;

namespace NewsAggregationSystem.DAL.Repositories.NewsSources
{
    public class NewsSourceRepository : RepositoryBase<NewsSource>, INewsSourceRepository
    {
        private readonly NewsAggregationSystemContext context;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();

        public NewsSourceRepository(NewsAggregationSystemContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<int> ChangeNewsSourceStatus(bool newsSourceStatus, NewsSource newsSource)
        {
            newsSource.IsActive = newsSourceStatus;
            if (newsSource.IsActive)
            {
                newsSource.LastAccess = dateTimeHelper.CurrentUtcDateTime;
            }
            return await UpdateAsync(newsSource);
        }
    }
}
