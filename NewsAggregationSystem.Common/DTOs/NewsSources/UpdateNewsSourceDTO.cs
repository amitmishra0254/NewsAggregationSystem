using System.ComponentModel.DataAnnotations;

namespace NewsAggregationSystem.Common.DTOs.NewsSources
{
    public class UpdateNewsSourceDTO
    {
        [Required, MaxLength(500)]
        public string ApiKey { get; set; }
        [Required]
        public bool IsActive { get; set; }
    }
}
