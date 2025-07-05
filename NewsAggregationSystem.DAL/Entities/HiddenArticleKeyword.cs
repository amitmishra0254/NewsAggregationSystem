using System.ComponentModel.DataAnnotations;

namespace NewsAggregationSystem.DAL.Entities
{
    public class HiddenArticleKeyword : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
