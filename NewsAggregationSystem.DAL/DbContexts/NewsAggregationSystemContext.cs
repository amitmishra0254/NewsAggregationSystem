using Microsoft.EntityFrameworkCore;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Extensions;
using System.Data.SqlTypes;

namespace NewsAggregationSystem.DAL.DbContexts
{
    public class NewsAggregationSystemContext : DbContext
    {
        public NewsAggregationSystemContext(DbContextOptions<NewsAggregationSystemContext> options) : base(options)
        {

        }

        public virtual DbSet<Article> Articles { get; set; }
        public virtual DbSet<ArticleReaction> ArticleReactions { get; set; }
        public virtual DbSet<NewsCategory> NewsCategories { get; set; }
        public virtual DbSet<NewsSource> NewsSources { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<NotificationPreference> NotificationPreferences { get; set; }
        public virtual DbSet<Reaction> Reactions { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<SavedArticle> SavedArticles { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<UserNewsKeyword> UserNewsKeywords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                throw new SqlNullValueException(ApplicationConstants.DatabaseProviderNotConfigured);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<Notification>(notification =>
            {
                notification.Property(prop => prop.IsRead)
                .HasDefaultValue(false);
            });

            modelBuilder.Entity<NotificationPreference>(notificationPreference =>
            {
                notificationPreference.Property(prop => prop.IsEnabled)
                .HasDefaultValue(true);
            });

            modelBuilder.Entity<User>(user =>
            {
                user.Property(prop => prop.IsActive)
                .HasDefaultValue(true);

                user.Property(prop => prop.IsEmailConfirmed)
                .HasDefaultValue(false);
            });

            modelBuilder.Entity<UserRole>()
                .HasIndex(userRole => new { userRole.UserId, userRole.RoleId })
                .IsUnique();

            modelBuilder.Entity<NewsCategory>()
                .HasIndex(newsCategory => newsCategory.Name)
                .IsUnique();

            modelBuilder.Seed();
        }
    }
}
