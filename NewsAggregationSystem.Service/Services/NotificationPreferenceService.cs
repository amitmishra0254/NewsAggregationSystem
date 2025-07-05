using Microsoft.EntityFrameworkCore;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsCategories;
using NewsAggregationSystem.Common.DTOs.NotificationPreferences;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;
using NewsAggregationSystem.DAL.Repositories.NewsCategories;
using NewsAggregationSystem.DAL.Repositories.NotificationPreferences;
using NewsAggregationSystem.DAL.Repositories.Users;
using NewsAggregationSystem.Service.Interfaces;

namespace NewsAggregationSystem.Service.Services
{
    public class NotificationPreferenceService : INotificationPreferenceService
    {
        private readonly INotificationPreferenceRepository notificationPreferenceRepository;
        private readonly IUserRepository userRepository;
        private readonly INewsCategoryRepository newsCategoryRepository;
        private readonly IRepositoryBase<UserNewsKeyword> userNewsKeywordRepository;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();
        public NotificationPreferenceService(INotificationPreferenceRepository notificationPreferenceRepository, IUserRepository userRepository, INewsCategoryRepository newsCategoryRepository, IRepositoryBase<UserNewsKeyword> userNewsKeywordRepository)
        {
            this.notificationPreferenceRepository = notificationPreferenceRepository;
            this.userRepository = userRepository;
            this.newsCategoryRepository = newsCategoryRepository;
            this.userNewsKeywordRepository = userNewsKeywordRepository;
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
                .GetWhere(np => userIds.Contains(np.UserId))
                .Include(np => np.NewsCategory)
                .ToListAsync();

            var keywords = await userNewsKeywordRepository
                .GetWhere(k => userIds.Contains(k.UserId))
                .ToListAsync();

            var result = preferences
                .GroupBy(p => p.UserId)
                .Select(userGroup => new NotificationPreferenceDTO
                {
                    UserId = userGroup.Key,
                    NewsCategories = userGroup.Where(x => !x.NewsCategory.IsHidden)
                        .GroupBy(p => p.NewsCategoryId)
                        .Select(catGroup =>
                        {
                            var categoryId = catGroup.Key;
                            var userId = catGroup.First().UserId;
                            var keywordList = keywords
                                .Where(k => k.UserId == userId && k.NewsCategoryId == categoryId)
                                .Select(k => new NotificationPreferencesKeywordDTO
                                {
                                    Id = k.Id,
                                    Name = k.Name,
                                    IsEnabled = k.IsEnabled
                                })
                                .ToList();

                            return new NewsCategoryDTO
                            {
                                Name = catGroup.First().NewsCategory.Name,
                                CategoryId = categoryId,
                                IsEnabled = catGroup.First().IsEnabled,
                                Keywords = keywordList
                            };
                        })
                        .ToList()
                })
                .ToList();

            return result;
        }

        public async Task<int> AddKeyword(string keyword, int categoryId, int userId)
        {
            var isKeywordAlreadyExist = await userNewsKeywordRepository
                .GetWhere(newsKeyword =>
                    newsKeyword.UserId == userId &&
                    newsKeyword.Name.ToLower() == keyword.ToLower()
                ).AnyAsync();

            if (isKeywordAlreadyExist)
            {
                throw new AlreadyExistException(string.Format(ApplicationConstants.KeywordAlreadyExist, keyword));
            }

            return await userNewsKeywordRepository.AddAsync(new UserNewsKeyword
            {
                UserId = userId,
                Name = keyword,
                NewsCategoryId = categoryId,
                IsEnabled = true,
                CreatedById = userId,
                CreatedDate = dateTimeHelper.CurrentUtcDateTime
            });
        }

        public async Task<int> ChangeKeywordStatus(int keywordId, bool isEnable)
        {
            var existingKeyword = await userNewsKeywordRepository.GetWhere(newsKeyword => newsKeyword.Id == keywordId).FirstOrDefaultAsync();
            if (existingKeyword == null)
            {
                throw new NotFoundException(ApplicationConstants.KeywordNotFoundWithThisId);
            }

            existingKeyword.IsEnabled = isEnable;
            return await userNewsKeywordRepository.UpdateAsync(existingKeyword);
        }

        public async Task<int> ChangeCategoryStatus(int categoryId, bool isEnable, int userId)
        {
            var existingPreference = await notificationPreferenceRepository
                .GetWhere(preference =>
                    preference.NewsCategoryId == categoryId &&
                    preference.UserId == userId
                ).FirstOrDefaultAsync();

            if (existingPreference == null)
            {
                throw new NotFoundException(ApplicationConstants.KeywordNotFoundWithThisId);
            }

            existingPreference.IsEnabled = isEnable;
            return await notificationPreferenceRepository.UpdateAsync(existingPreference);
        }


        /*public async Task<int> UpdateNotificationConfigurations(List<NotificationConfigurationResponseDTO> notificationConfigurations)
        {
            var notificationConfigurations = await notificationPreferenceRepository.GetWhere(notificationConfiguration => notificationConfiguration.UserId == userId)
                .Include(notificationConfiguration => notificationConfiguration.NewsCategory)
                .Include(notificationConfiguration => notificationConfiguration.UserNewsKeyword)
                .GroupBy(notificationConfiguration => notificationConfiguration.NewsCategory.Name)
                .Select(group => new NotificationConfigurationResponseDTO
                {
                    Category = group.Key,
                    Keywords = string.Join("|", group.Select(p => p.UserNewsKeyword.Name)),
                    IsEnabled = group.First().IsEnabled
                }).ToListAsync();

            return notificationConfigurations;
        }*/
    }
}
