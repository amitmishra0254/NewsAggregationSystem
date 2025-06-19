using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAggregationSystem.DAL.Entities
{
    [Index(nameof(UserId), nameof(RoleId), IsUnique = true)]
    public class UserRole : AuditableEntity
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
        [ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; set; }
    }
}
