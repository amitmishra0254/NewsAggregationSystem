using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAggregationSystem.Common.DTOs.NewsCategories
{
    public class NewsCategoryDTO
    {
        public string Name { get; set; }
        public List<string> Keywords { get; set; }
    }
}
