using AutoMapper;
using Microsoft.Extensions.Configuration;
using Moq;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs;
using NewsAggregationSystem.Common.DTOs.NewsCategories;
using NewsAggregationSystem.Common.DTOs.NotificationPreferences;
using NewsAggregationSystem.Common.DTOs.Notifications;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Notifications;
using NewsAggregationSystem.Service.Interfaces;
using NewsAggregationSystem.Service.Services;
using NewsAggregationSystem.Service.Tests.Utilities;

namespace NewsAggregationSystem.Service.Tests.Services
{
    [TestFixture]
    public class NotificationServiceTests
    {
        private Mock<INotificationRepository> mockNotificationRepository;
        private Mock<IConfiguration> mockConfiguration;
        private Mock<IMapper> mockMapper;
        private Mock<INotificationPreferenceService> mockNotificationPreferenceService;
        private NotificationService notificationService;

        [SetUp]
        public void SetUp()
        {
            mockNotificationRepository = new Mock<INotificationRepository>();
            mockConfiguration = new Mock<IConfiguration>();
            mockMapper = new Mock<IMapper>();
            mockNotificationPreferenceService = new Mock<INotificationPreferenceService>();

            notificationService = new NotificationService(
                mockNotificationRepository.Object,
                mockConfiguration.Object,
                mockMapper.Object,
                mockNotificationPreferenceService.Object
            );
        }

        [Test]
        public async Task AddNotifications_WhenNotificationsProvided_AddsAllNotifications()
        {
            var notifications = new List<Notification>
            {
                new Notification
                {
                    Id = 1,
                    UserId = 1,
                    Title = "New Article",
                    Message = "New article available",
                    IsRead = false
                },
                new Notification
                {
                    Id = 2,
                    UserId = 2,
                    Title = "New Article",
                    Message = "New article available",
                    IsRead = false
                }
            };

            mockNotificationRepository
                .Setup(repo => repo.AddRangeAsync(It.IsAny<List<Notification>>()))
                .ReturnsAsync(2);

            var result = await notificationService.AddNotifications(notifications);

            Assert.AreEqual(2, result);
            mockNotificationRepository.Verify(repo => repo.AddRangeAsync(notifications), Times.Once);
        }

        [Test]
        public async Task GetAllNotifications_WhenUnreadNotificationsExist_ReturnsAndMarksAsRead()
        {
            var userId = 1;
            var notifications = new List<Notification>
            {
                new Notification
                {
                    Id = 1,
                    UserId = userId,
                    Title = "New Article",
                    Message = "New article available",
                    IsRead = false
                },
                new Notification
                {
                    Id = 2,
                    UserId = userId,
                    Title = "New Article",
                    Message = "Another article available",
                    IsRead = false
                }
            };

            var notificationDtos = new List<NotificationDTO>
            {
                new NotificationDTO { Id = 1, Title = "New Article", Message = "New article available" },
                new NotificationDTO { Id = 2, Title = "New Article", Message = "Another article available" }
            };

            mockNotificationRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<Notification, bool>>>()))
                .Returns(notifications.AsQueryable().BuildMockDbSet().Object);

            mockNotificationRepository
                .Setup(repo => repo.SaveAsync())
                .ReturnsAsync(1);

            mockMapper
                .Setup(m => m.Map<List<NotificationDTO>>(It.IsAny<List<Notification>>()))
                .Returns(notificationDtos);

            var result = await notificationService.GetAllNotifications(userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(notifications.All(n => n.IsRead));
            mockNotificationRepository.Verify(repo => repo.SaveAsync(), Times.Once);
        }

        [Test]
        public void GenerateNotificationsFromUserPreferences_WhenArticlesMatchPreferences_GeneratesNotifications()
        {
            var articles = new List<Article>
            {
                new Article
                {
                    Id = 1,
                    Title = "AI Technology Breakthrough",
                    Description = "Latest developments in artificial intelligence",
                    NewsCategory = new NewsCategory { Name = "Technology" }
                },
                new Article
                {
                    Id = 2,
                    Title = "Football Championship",
                    Description = "Latest sports news",
                    NewsCategory = new NewsCategory { Name = "Sports" }
                }
            };

            var userPreferences = new List<NotificationPreferenceDTO>
            {
                new NotificationPreferenceDTO
                {
                    UserId = 1,
                    NewsCategories = new List<NewsCategoryDTO>
                    {
                        new NewsCategoryDTO
                        {
                            CategoryId = 1,
                            Name = "Technology",
                            IsEnabled = true,
                            Keywords = new List<NotificationPreferencesKeywordDTO>
                            {
                                new NotificationPreferencesKeywordDTO { Name = "AI", IsEnabled = true }
                            }
                        }
                    }
                }
            };

            mockConfiguration
                .Setup(c => c["ArticleRequestUrl"])
                .Returns("https://example.com/article/");

            var result = notificationService.GenerateNotificationsFromUserPreferences(articles, userPreferences);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].UserId);
            Assert.AreEqual(ApplicationConstants.NewArticleNotificationTitle, result[0].Title);
            Assert.IsTrue(result[0].Message.Contains("AI Technology Breakthrough"));
        }

        [Test]
        public async Task NotifyAdminAboutReportedArticle_WhenReportProvided_CreatesAdminNotification()
        {
            var report = new ReportRequestDTO
            {
                ArticleId = 123,
                Reason = "Inappropriate content"
            };
            var userId = 1;

            mockNotificationRepository
                .Setup(repo => repo.AddAsync(It.IsAny<Notification>()))
                .ReturnsAsync(1);

            var result = await notificationService.NotifyAdminAboutReportedArticle(report, userId);

            Assert.AreEqual(1, result);
            mockNotificationRepository.Verify(repo => repo.AddAsync(It.Is<Notification>(n =>
                n.UserId == userId &&
                n.Title == ApplicationConstants.ReportTitle &&
                n.Message.Contains(report.ArticleId.ToString()) &&
                n.Message.Contains(report.Reason) &&
                n.IsRead == false &&
                n.CreatedById == userId
            )), Times.Once);
        }
    }
}