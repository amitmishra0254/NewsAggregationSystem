using NewsAggregationSystem.Common.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAggregationSystem.DAL.Entities
{
    public class User
    {
        public User()
        {
            NotificationPreferences = new HashSet<NotificationPreference>();
            Notifications = new HashSet<Notification>();
            UserRoles = new HashSet<UserRole>();
            SavedArticles = new HashSet<SavedArticle>();
            ReactedArticles = new HashSet<ArticleReaction>();
        }
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string FirstName { get; set; }
        [MaxLength(50)]
        public string LastName { get; set; }
        [MaxLength(50)]
        public string UserName { get; set; }
        [MaxLength(256)]
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public bool IsActive { get; set; } = true;
        [Column(TypeName = ApplicationConstants.DateTime2With2Precision)]
        public DateTime? LockoutEndDate { get; set; }
        public int AccessFailedCount { get; set; } = 0;
        public bool IsEmailConfirmed { get; set; } = false;
        [Column(TypeName = ApplicationConstants.DateTime2With2Precision)]
        public DateTime CreatedDate { get; set; }
        [Column(TypeName = ApplicationConstants.DateTime2With2Precision)]
        public DateTime? ModifiedDate { get; set; }
        public virtual ICollection<NotificationPreference> NotificationPreferences { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<SavedArticle> SavedArticles { get; set; }
        public virtual ICollection<ArticleReaction> ReactedArticles { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
