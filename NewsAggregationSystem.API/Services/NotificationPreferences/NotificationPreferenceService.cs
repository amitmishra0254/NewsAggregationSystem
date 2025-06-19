using Microsoft.EntityFrameworkCore;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsCategories;
using NewsAggregationSystem.Common.DTOs.NotificationPreferences;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.NewsCategories;
using NewsAggregationSystem.DAL.Repositories.NotificationPreferences;
using NewsAggregationSystem.DAL.Repositories.Users;

namespace NewsAggregationSystem.API.Services.NotificationPreferences
{
    public class NotificationPreferenceService : INotificationPreferenceService
    {
        private readonly INotificationPreferenceRepository notificationPreferenceRepository;
        private readonly IUserRepository userRepository;
        private readonly INewsCategoryRepository newsCategoryRepository;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();
        public NotificationPreferenceService(INotificationPreferenceRepository notificationPreferenceRepository, IUserRepository userRepository, INewsCategoryRepository newsCategoryRepository)
        {
            this.notificationPreferenceRepository = notificationPreferenceRepository;
            this.userRepository = userRepository;
            this.newsCategoryRepository = newsCategoryRepository;
        }

        public async Task AddNotificationPreferencesPerCategory(int newsCategoryId)
        {
            var users = await userRepository.GetAll().ToListAsync();
            var batchSize = 100;

            foreach (var batch in users.Chunk(batchSize))
            {
                var preferences = batch.Select(user => new NotificationPreference
                {
                    UserId = user.Id,
                    NewsCategoryId = newsCategoryId,
                    IsEnabled = false,
                    CreatedById = ApplicationConstants.SystemUserId,
                    CreatedDate = DateTime.UtcNow
                }).ToList();

                await notificationPreferenceRepository.AddRangeAsync(preferences);
            }
        }

        public async Task AddNotificationPreferencesPerUser(int userId)
        {
            var user = await userRepository.GetWhere(user => user.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new NotFoundException(string.Format(ApplicationConstants.UserNotFoundWithThisId, userId.ToString()));
            }
            else
            {
                var categories = await newsCategoryRepository.GetAll().ToListAsync();

                var preferences = categories.Select(category => new NotificationPreference
                {
                    UserId = userId,
                    NewsCategoryId = category.Id,
                    IsEnabled = false,
                    CreatedById = ApplicationConstants.SystemUserId,
                    CreatedDate = dateTimeHelper.CurrentUtcDateTime
                }).ToList();

                await notificationPreferenceRepository.AddRangeAsync(preferences);
            }
        }

        public async Task<List<NotificationPreferenceDTO>> GetNotificationPreferences(List<int> userIds)
        {
            if (!userIds.Any())
            {
                userIds = await userRepository
                    .GetWhere(user => user.IsActive)
                    .Select(user => user.Id)
                    .ToListAsync();
            }

            var preferences = await notificationPreferenceRepository
                .GetWhere(np => userIds.Contains(np.UserId) && np.IsEnabled)
                .Include(np => np.NewsCategory)
                .Include(np => np.UserNewsKeyword)
                .ToListAsync();

            var result = preferences
                .GroupBy(p => p.UserId)
                .Select(userGroup => new NotificationPreferenceDTO
                {
                    UserId = userGroup.Key,
                    NewsCategories = userGroup
                        .GroupBy(p => p.NewsCategoryId)
                        .Select(catGroup => new NewsCategoryDTO
                        {
                            Name = catGroup.First().NewsCategory.Name,
                            Keywords = catGroup
                                .Where(p => p.UserNewsKeyword != null)
                                .Select(p => p.UserNewsKeyword.Name)
                                .Distinct()
                                .ToList()
                        })
                        .ToList()
                })
                .ToList();

            return result;
        }

    }
}
