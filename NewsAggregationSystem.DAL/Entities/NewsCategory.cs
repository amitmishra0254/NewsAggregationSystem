using System.ComponentModel.DataAnnotations;

namespace NewsAggregationSystem.DAL.Entities
{
    public class NewsCategory : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
    }
}
