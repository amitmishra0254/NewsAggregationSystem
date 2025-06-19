using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAggregationSystem.Common.DTOs.NewsSources
{
    public class CreateNewsSourceDTO
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(1000)]
        public string BaseUrl { get; set; }

        [Required, MaxLength(500)]
        public string ApiKey { get; set; }
    }
}
