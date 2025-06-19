using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAggregationSystem.Common.DTOs.Providers
{
    public class NotificationEmailDTO
    {
        public string Email { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
    }
}
