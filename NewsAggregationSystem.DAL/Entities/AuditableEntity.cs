using NewsAggregationSystem.Common.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAggregationSystem.DAL.Entities
{
    public abstract class AuditableEntity
    {
        [Column(TypeName = ApplicationConstants.DateTime2With2Precision)]
        public DateTime CreatedDate { get; set; }
        [Column(TypeName = ApplicationConstants.DateTime2With2Precision)]
        public DateTime? ModifiedDate { get; set; }
        public int CreatedById { get; set; }
        public int? ModifiedById { get; set; }
    }
}
