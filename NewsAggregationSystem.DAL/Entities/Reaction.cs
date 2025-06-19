using System.ComponentModel.DataAnnotations;

namespace NewsAggregationSystem.DAL.Entities
{
    public class Reaction : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
