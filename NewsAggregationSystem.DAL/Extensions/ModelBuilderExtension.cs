using Microsoft.EntityFrameworkCore;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.DAL.Entities;
using System.Security.Cryptography;

namespace NewsAggregationSystem.DAL.Extensions
{
    public static class ModelBuilderExtension
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            var hmac = new HMACSHA512();
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = ApplicationConstants.SystemUserId,
                    FirstName = "System",
                    LastName = "Scheduler",
                    UserName = "system",
                    Email = "newsaggregator@yopmail.com",
                    PasswordHash = new byte[] { 121, 63, 61, 71, 31, 178, 180, 206, 106, 29, 26, 155, 83, 60, 19, 40, 145, 145, 15, 127, 67, 26, 204, 174, 115, 136, 231, 30, 242, 165, 15, 1, 245, 92, 154, 92, 222, 125, 4, 54, 220, 55, 230, 139, 189, 185, 166, 15, 247, 211, 131, 146, 66, 203, 19, 29, 164, 32, 47, 239, 206, 142, 62, 165, 3, 175, 204, 171, 124, 124, 170, 234, 39, 222, 247, 62, 27, 194, 22, 13, 19, 234, 170, 240, 5, 169, 107, 245, 89, 93, 13, 6, 243, 79, 134, 77, 161, 54, 252, 133, 168, 12, 47, 12, 221, 150, 49, 175, 175, 161, 124, 205, 37, 45, 78, 210, 153, 160, 49, 1, 94, 117, 222, 73, 216, 15, 209, 191 },
                    PasswordSalt = new byte[] { 27, 42, 28, 246, 170, 215, 3, 124, 113, 165, 135, 52, 62, 155, 96, 154, 73, 72, 52, 138, 230, 89, 125, 162, 107, 94, 76, 30, 213, 228, 165, 14, 254, 121, 31, 121, 33, 85, 65, 242, 61, 17, 129, 66, 82, 75, 233, 45, 131, 34, 63, 74, 149, 21, 214, 46, 121, 58, 211, 48, 5, 222, 87, 51 },
                    IsActive = true,
                    IsEmailConfirmed = true,
                    AccessFailedCount = 0,
                    CreatedDate = new DateTime(2025, 6, 18, 0, 0, 0),
                    LockoutEndDate = null,
                    ModifiedDate = null
                }
            );

            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = (int)Common.Enums.UserRoles.Admin,
                    Name = "Admin",
                    Description = "The Admin role has full access to all features and data in the system.",
                    CreatedDate = new DateTime(2025, 6, 18, 0, 0, 0),
                    CreatedById = ApplicationConstants.SystemUserId
                });

            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = (int)Common.Enums.UserRoles.User,
                    Name = "User",
                    Description = "The User role has limited access to the features and data in the system.",
                    CreatedDate = new DateTime(2025, 6, 18, 0, 0, 0),
                    CreatedById = ApplicationConstants.SystemUserId
                });

            modelBuilder.Entity<UserRole>().HasData(
                new UserRole
                {
                    UserId = ApplicationConstants.SystemUserId,
                    RoleId = (int)Common.Enums.UserRoles.Admin,
                    CreatedDate = new DateTime(2025, 6, 18, 0, 0, 0),
                    CreatedById = ApplicationConstants.SystemUserId
                });

            modelBuilder.Entity<Reaction>().HasData(
                new Reaction
                {
                    Id = 1,
                    Name = "Like",
                    CreatedDate = new DateTime(2025, 6, 18, 0, 0, 0),
                    CreatedById = ApplicationConstants.SystemUserId
                });

            modelBuilder.Entity<Reaction>().HasData(
                new Reaction
                {
                    Id = 2,
                    Name = "Dislike",
                    CreatedDate = new DateTime(2025, 6, 18, 0, 0, 0),
                    CreatedById = ApplicationConstants.SystemUserId
                });
        }
    }
}
