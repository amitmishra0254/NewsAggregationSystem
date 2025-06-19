using NewsAggregationSystem.DAL.DbContexts;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAggregationSystem.DAL.Repositories.Users
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        private readonly NewsAggregationSystemContext context;
        public UserRepository(NewsAggregationSystemContext context) : base(context) 
        {
            this.context = context;
        }
    }
}
