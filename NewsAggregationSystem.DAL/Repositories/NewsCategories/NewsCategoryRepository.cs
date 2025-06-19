using NewsAggregationSystem.DAL.DbContexts;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAggregationSystem.DAL.Repositories.NewsCategories
{
    public class NewsCategoryRepository : RepositoryBase<NewsCategory>, INewsCategoryRepository
    {
        private readonly NewsAggregationSystemContext context;
        public NewsCategoryRepository(NewsAggregationSystemContext context) : base(context)
        {
            this.context = context;
        }
    }
}
