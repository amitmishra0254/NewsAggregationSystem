using Moq;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;
using NewsAggregationSystem.DAL.Repositories.NewsCategories;
using NewsAggregationSystem.DAL.Repositories.NotificationPreferences;
using NewsAggregationSystem.DAL.Repositories.Users;
using NewsAggregationSystem.Service.Services;
using NewsAggregationSystem.Service.Tests.Utilities;

namespace NewsAggregationSystem.Service.Tests.Services
{
    [TestFixture]
    public class NotificationPreferenceServiceTests
    {
        private Mock<INotificationPreferenceRepository> mockNotificationPreferenceRepository;
        private Mock<IUserRepository> mockUserRepository;
        private Mock<INewsCategoryRepository> mockNewsCategoryRepository;
        private Mock<IRepositoryBase<UserNewsKeyword>> mockUserNewsKeywordRepository;
        private NotificationPreferenceService notificationPreferenceService;

        [SetUp]
        public void SetUp()
        {
            mockNotificationPreferenceRepository = new Mock<INotificationPreferenceRepository>();
            mockUserRepository = new Mock<IUserRepository>();
            mockNewsCategoryRepository = new Mock<INewsCategoryRepository>();
            mockUserNewsKeywordRepository = new Mock<IRepositoryBase<UserNewsKeyword>>();

            notificationPreferenceService = new NotificationPreferenceService(
                mockNotificationPreferenceRepository.Object,
                mockUserRepository.Object,
                mockNewsCategoryRepository.Object,
                mockUserNewsKeywordRepository.Object
            );
        }

        #region AddNotificationPreferencesPerCategory Tests

        [Test]
        public async Task AddNotificationPreferencesPerCategory_WhenUsersExist_AddsPreferencesForAllUsers()
        {
            var newsCategoryId = 1;
            var users = new List<User>
            {
                new User { Id = 1, IsActive = true },
                new User { Id = 2, IsActive = true },
                new User { Id = 3, IsActive = true }
            };

            mockUserRepository
                .Setup(repo => repo.GetAll())
                .Returns(users.AsQueryable().BuildMockDbSet().Object);

            mockNotificationPreferenceRepository
                .Setup(repo => repo.AddRangeAsync(It.IsAny<List<NotificationPreference>>()))
                .ReturnsAsync(3);

            await notificationPreferenceService.AddNotificationPreferencesPerCategory(newsCategoryId);

            mockNotificationPreferenceRepository.Verify(repo => repo.AddRangeAsync(It.Is<List<NotificationPreference>>(list =>
                list.Count == 3 &&
                list.All(p => p.NewsCategoryId == newsCategoryId &&
                              p.IsEnabled == false &&
                              p.CreatedById == ApplicationConstants.SystemUserId)
            )), Times.Once);
        }

        [Test]
        public async Task AddNotificationPreferencesPerCategory_WhenNoUsersExist_DoesNotAddPreferences()
        {
            var newsCategoryId = 1;
            var users = new List<User>();

            mockUserRepository
                .Setup(repo => repo.GetAll())
                .Returns(users.AsQueryable().BuildMockDbSet().Object);

            await notificationPreferenceService.AddNotificationPreferencesPerCategory(newsCategoryId);

            mockNotificationPreferenceRepository.Verify(repo => repo.AddRangeAsync(It.IsAny<List<NotificationPreference>>()), Times.Never);
        }

        #endregion

        #region AddNotificationPreferencesPerUser Tests

        [Test]
        public async Task AddNotificationPreferencesPerUser_WhenUserExists_AddsPreferencesForAllCategories()
        {
            var userId = 1;
            var user = new User { Id = userId, IsActive = true };
            var categories = new List<NewsCategory>
            {
                new NewsCategory { Id = 1, Name = "Technology" },
                new NewsCategory { Id = 2, Name = "Sports" },
                new NewsCategory { Id = 3, Name = "Politics" }
            };

            mockUserRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .Returns(new List<User> { user }.AsQueryable().BuildMockDbSet().Object);

            mockNewsCategoryRepository
                .Setup(repo => repo.GetAll())
                .Returns(categories.AsQueryable().BuildMockDbSet().Object);

            mockNotificationPreferenceRepository
                .Setup(repo => repo.AddRangeAsync(It.IsAny<List<NotificationPreference>>()))
                .ReturnsAsync(3);

            await notificationPreferenceService.AddNotificationPreferencesPerUser(userId);

            mockNotificationPreferenceRepository.Verify(repo => repo.AddRangeAsync(It.Is<List<NotificationPreference>>(list =>
                list.Count == 3 &&
                list.All(p => p.UserId == userId &&
                              p.IsEnabled == false &&
                              p.CreatedById == ApplicationConstants.SystemUserId)
            )), Times.Once);
        }

        [Test]
        public void AddNotificationPreferencesPerUser_WhenUserDoesNotExist_ThrowsNotFoundException()
        {
            var userId = 999;

            mockUserRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .Returns(new List<User>().AsQueryable().BuildMockDbSet().Object);

            var exception = Assert.ThrowsAsync<NotFoundException>(async () =>
                await notificationPreferenceService.AddNotificationPreferencesPerUser(userId));

            Assert.IsTrue(exception.Message.Contains(userId.ToString()));
        }

        #endregion

        #region GetNotificationPreferences Tests

        [Test]
        public async Task GetNotificationPreferences_WhenUserIdsProvided_ReturnsPreferencesForSpecificUsers()
        {
            var userIds = new List<int> { 1, 2 };
            var preferences = new List<NotificationPreference>
            {
                new NotificationPreference
                {
                    UserId = 1,
                    NewsCategoryId = 1,
                    IsEnabled = true,
                    NewsCategory = new NewsCategory { Id = 1, Name = "Technology", IsHidden = false }
                },
                new NotificationPreference
                {
                    UserId = 2,
                    NewsCategoryId = 2,
                    IsEnabled = false,
                    NewsCategory = new NewsCategory { Id = 2, Name = "Sports", IsHidden = false }
                }
            };

            var keywords = new List<UserNewsKeyword>
            {
                new UserNewsKeyword { Id = 1, UserId = 1, NewsCategoryId = 1, Name = "AI", IsEnabled = true },
                new UserNewsKeyword { Id = 2, UserId = 2, NewsCategoryId = 2, Name = "Football", IsEnabled = true }
            };

            mockNotificationPreferenceRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<NotificationPreference, bool>>>()))
                .Returns(preferences.AsQueryable().BuildMockDbSet().Object);

            mockUserNewsKeywordRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<UserNewsKeyword, bool>>>()))
                .Returns(keywords.AsQueryable().BuildMockDbSet().Object);

            var result = await notificationPreferenceService.GetUserNotificationPreferencesAsync(userIds);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].UserId);
            Assert.AreEqual(2, result[1].UserId);
        }

        [Test]
        public async Task GetNotificationPreferences_WhenNoUserIdsProvided_ReturnsPreferencesForAllActiveUsers()
        {
            var userIds = new List<int>();
            var activeUsers = new List<User>
            {
                new User { Id = 1, IsActive = true },
                new User { Id = 2, IsActive = true }
            };

            var preferences = new List<NotificationPreference>
            {
                new NotificationPreference
                {
                    UserId = 1,
                    NewsCategoryId = 1,
                    IsEnabled = true,
                    NewsCategory = new NewsCategory { Id = 1, Name = "Technology", IsHidden = false }
                }
            };

            var keywords = new List<UserNewsKeyword>();

            mockUserRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .Returns(activeUsers.AsQueryable().BuildMockDbSet().Object);

            mockNotificationPreferenceRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<NotificationPreference, bool>>>()))
                .Returns(preferences.AsQueryable().BuildMockDbSet().Object);

            mockUserNewsKeywordRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<UserNewsKeyword, bool>>>()))
                .Returns(keywords.AsQueryable().BuildMockDbSet().Object);

            var result = await notificationPreferenceService.GetUserNotificationPreferencesAsync(userIds);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        #endregion

        #region AddKeyword Tests

        [Test]
        public async Task AddKeyword_WhenKeywordDoesNotExist_AddsNewKeyword()
        {
            var keyword = "Artificial Intelligence";
            var categoryId = 1;
            var userId = 1;

            mockUserNewsKeywordRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<UserNewsKeyword, bool>>>()))
                .Returns(new List<UserNewsKeyword>().AsQueryable().BuildMockDbSet().Object);

            mockUserNewsKeywordRepository
                .Setup(repo => repo.AddAsync(It.IsAny<UserNewsKeyword>()))
                .ReturnsAsync(1);

            var result = await notificationPreferenceService.AddKeywordToCategoryAsync(keyword, categoryId, userId);

            Assert.AreEqual(1, result);
            mockUserNewsKeywordRepository.Verify(repo => repo.AddAsync(It.Is<UserNewsKeyword>(k =>
                k.Name == keyword &&
                k.NewsCategoryId == categoryId &&
                k.UserId == userId &&
                k.IsEnabled == true &&
                k.CreatedById == userId
            )), Times.Once);
        }

        [Test]
        public void AddKeyword_WhenKeywordAlreadyExists_ThrowsAlreadyExistException()
        {
            var keyword = "AI";
            var categoryId = 1;
            var userId = 1;

            var existingKeyword = new UserNewsKeyword
            {
                Id = 1,
                Name = "AI",
                UserId = userId,
                NewsCategoryId = categoryId
            };

            mockUserNewsKeywordRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<UserNewsKeyword, bool>>>()))
                .Returns(new List<UserNewsKeyword> { existingKeyword }.AsQueryable().BuildMockDbSet().Object);

            var exception = Assert.ThrowsAsync<AlreadyExistException>(async () =>
                await notificationPreferenceService.AddKeywordToCategoryAsync(keyword, categoryId, userId));

            Assert.IsTrue(exception.Message.Contains(keyword));
        }

        #endregion

        #region ChangeKeywordStatus Tests

        [Test]
        public async Task ChangeKeywordStatus_WhenKeywordExists_UpdatesStatus()
        {
            var keywordId = 1;
            var isEnable = true;

            var existingKeyword = new UserNewsKeyword
            {
                Id = keywordId,
                Name = "AI",
                IsEnabled = false,
                UserId = 1,
                NewsCategoryId = 1
            };

            mockUserNewsKeywordRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<UserNewsKeyword, bool>>>()))
                .Returns(new List<UserNewsKeyword> { existingKeyword }.AsQueryable().BuildMockDbSet().Object);

            mockUserNewsKeywordRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<UserNewsKeyword>()))
                .ReturnsAsync(1);

            var result = await notificationPreferenceService.UpdateKeywordStatusAsync(keywordId, isEnable);

            Assert.AreEqual(1, result);
            Assert.AreEqual(isEnable, existingKeyword.IsEnabled);
            mockUserNewsKeywordRepository.Verify(repo => repo.UpdateAsync(existingKeyword), Times.Once);
        }

        [Test]
        public void ChangeKeywordStatus_WhenKeywordDoesNotExist_ThrowsNotFoundException()
        {
            var keywordId = 999;
            var isEnable = true;

            mockUserNewsKeywordRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<UserNewsKeyword, bool>>>()))
                .Returns(new List<UserNewsKeyword>().AsQueryable().BuildMockDbSet().Object);

            var exception = Assert.ThrowsAsync<NotFoundException>(async () =>
                await notificationPreferenceService.UpdateKeywordStatusAsync(keywordId, isEnable));

            Assert.IsTrue(exception.Message.Contains(ApplicationConstants.KeywordNotFoundWithThisId));
        }

        #endregion

        #region ChangeCategoryStatus Tests

        [Test]
        public async Task ChangeCategoryStatus_WhenPreferenceExists_UpdatesStatus()
        {
            var categoryId = 1;
            var userId = 1;
            var isEnable = true;

            var existingPreference = new NotificationPreference
            {
                Id = 1,
                UserId = userId,
                NewsCategoryId = categoryId,
                IsEnabled = false
            };

            mockNotificationPreferenceRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<NotificationPreference, bool>>>()))
                .Returns(new List<NotificationPreference> { existingPreference }.AsQueryable().BuildMockDbSet().Object);

            mockNotificationPreferenceRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<NotificationPreference>()))
                .ReturnsAsync(1);

            var result = await notificationPreferenceService.UpdateCategoryStatusAsync(categoryId, isEnable, userId);

            Assert.AreEqual(1, result);
            Assert.AreEqual(isEnable, existingPreference.IsEnabled);
            mockNotificationPreferenceRepository.Verify(repo => repo.UpdateAsync(existingPreference), Times.Once);
        }

        [Test]
        public void ChangeCategoryStatus_WhenPreferenceDoesNotExist_ThrowsNotFoundException()
        {
            var categoryId = 999;
            var userId = 1;
            var isEnable = true;

            mockNotificationPreferenceRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<NotificationPreference, bool>>>()))
                .Returns(new List<NotificationPreference>().AsQueryable().BuildMockDbSet().Object);

            var exception = Assert.ThrowsAsync<NotFoundException>(async () =>
                await notificationPreferenceService.UpdateCategoryStatusAsync(categoryId, isEnable, userId));

            Assert.IsTrue(exception.Message.Contains(ApplicationConstants.KeywordNotFoundWithThisId));
        }

        #endregion
    }
}