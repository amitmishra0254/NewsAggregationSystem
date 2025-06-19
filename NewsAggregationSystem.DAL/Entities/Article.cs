using NewsAggregationSystem.Common.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAggregationSystem.DAL.Entities
{
    public class Article : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(500)]
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? ImageUrl { get; set; }
        [Column(TypeName = ApplicationConstants.DateTime2With2Precision)]
        public DateTime? PublishedAt { get; set; }
        [MaxLength(255)]
        public string? SourceName { get; set; }
        [MaxLength(255)]
        public string? Author { get; set; }
        public string? Content { get; set; }
        public int NewsCategoryId { get; set; }
        public virtual NewsCategory NewsCategory { get; set; }
    }
}
