using NewsAggregationSystem.Common.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAggregationSystem.DAL.Entities
{
    public class NewsSource : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(1000)]
        public string BaseUrl { get; set; }
        [MaxLength(500)]
        public string ApiKey { get; set; }
        public bool IsActive { get; set; }
        [Column(TypeName = ApplicationConstants.DateTime2With2Precision)]
        public DateTime LastAccess { get; set; }
    }
}
