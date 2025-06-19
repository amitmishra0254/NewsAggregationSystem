using System.ComponentModel.DataAnnotations;

namespace NewsAggregationSystem.Common.DTOs.NewsSources
{
    public class UpdateNewsSourceDTO : CreateNewsSourceDTO
    {
        [Required]
        public bool IsActive { get; set; }
    }
}
