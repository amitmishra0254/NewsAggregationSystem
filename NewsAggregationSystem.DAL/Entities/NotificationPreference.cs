using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAggregationSystem.DAL.Entities
{
    public class NotificationPreference : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int NewsCategoryId { get; set; }
        public int? UserNewsKeywordId { get; set; }
        public int UserId { get; set; }
        [ForeignKey(nameof(NewsCategoryId))]
        public virtual NewsCategory NewsCategory { get; set; }
        [ForeignKey(nameof(UserNewsKeywordId))]
        public virtual UserNewsKeyword? UserNewsKeyword { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
    }
}