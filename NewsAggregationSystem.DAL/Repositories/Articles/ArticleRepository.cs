using NewsAggregationSystem.DAL.DbContexts;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAggregationSystem.DAL.Repositories.Articles
{
    public class ArticleRepository : RepositoryBase<Article>, IArticleRepository
    {
        private readonly NewsAggregationSystemContext context;
        public ArticleRepository(NewsAggregationSystemContext context) : base(context) 
        {
            this.context = context;
        }
    }
}
