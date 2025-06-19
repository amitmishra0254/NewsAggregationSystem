using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAggregationSystem.DAL.Entities
{
    public class ArticleReaction : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int ArticleId { get; set; }
        public int ReactionId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
        [ForeignKey(nameof(ArticleId))]
        public virtual Article Article { get; set; }
        [ForeignKey(nameof(ReactionId))]
        public virtual Reaction Reaction { get; set; }
    }
}
