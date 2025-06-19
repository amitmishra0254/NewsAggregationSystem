using System.ComponentModel.DataAnnotations;

namespace NewsAggregationSystem.DAL.Entities
{
    public class Role : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
    }
}
