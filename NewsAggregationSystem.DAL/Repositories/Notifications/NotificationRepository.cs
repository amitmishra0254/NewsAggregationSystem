using Microsoft.EntityFrameworkCore.ChangeTracking;
using NewsAggregationSystem.DAL.DbContexts;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAggregationSystem.DAL.Repositories.Notifications
{
    public class NotificationRepository : RepositoryBase<Notification>, INotificationRepository
    {
        private readonly NewsAggregationSystemContext context;
        public NotificationRepository(NewsAggregationSystemContext context) : base(context) 
        {
            this.context = context;
        }
    }
}
