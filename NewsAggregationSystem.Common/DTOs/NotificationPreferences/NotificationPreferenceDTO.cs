using NewsAggregationSystem.Common.DTOs.NewsCategories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAggregationSystem.Common.DTOs.NotificationPreferences
{
    public class NotificationPreferenceDTO
    {
        public int UserId { get; set; }
        public List<NewsCategoryDTO> NewsCategories { get; set; }
    }
}
