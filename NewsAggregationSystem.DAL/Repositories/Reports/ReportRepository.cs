using NewsAggregationSystem.DAL.DbContexts;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;

namespace NewsAggregationSystem.DAL.Repositories.Reports
{
    public class ReportRepository : RepositoryBase<ReportedArticle>, IReportRepository
    {
        private readonly NewsAggregationSystemContext context;
        public ReportRepository(NewsAggregationSystemContext context) : base(context)
        {
            this.context = context;
        }
    }
}
