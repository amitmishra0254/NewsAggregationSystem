using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAggregationSystem.DAL.Entities
{
    public class UserNewsKeyword : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
        public int NewsCategoryId { get; set; }
        [ForeignKey(nameof(NewsCategoryId))]
        public virtual NewsCategory NewsCategory { get; set; }
    }
}
