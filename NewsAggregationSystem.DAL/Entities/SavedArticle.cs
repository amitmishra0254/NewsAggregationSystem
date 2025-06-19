using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAggregationSystem.DAL.Entities
{
    public class SavedArticle : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ArticleId { get; set; }
        [ForeignKey(nameof(ArticleId))]
        public virtual Article Article { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
    }
}
