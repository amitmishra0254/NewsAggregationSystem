using Microsoft.Extensions.Configuration;
using Moq;
using NewsAggregationSystem.Common.DTOs;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;
using NewsAggregationSystem.DAL.Repositories.Reports;
using NewsAggregationSystem.Service.Interfaces;
using NewsAggregationSystem.Service.Services;
using NewsAggregationSystem.Service.Tests.Utilities;

namespace NewsAggregationSystem.Service.Tests.Services
{
    [TestFixture]
    public class ReportServiceTests
    {
        private Mock<IReportRepository> mockReportRepository;
        private Mock<IArticleService> mockArticleService;
        private Mock<IConfiguration> mockConfiguration;
        private Mock<INotificationService> mockNotificationService;
        private Mock<IRepositoryBase<UserRole>> mockUserRoleRepository;
        private ReportService reportService;

        [SetUp]
        public void SetUp()
        {
            mockReportRepository = new Mock<IReportRepository>();
            mockArticleService = new Mock<IArticleService>();
            mockConfiguration = new Mock<IConfiguration>();
            mockNotificationService = new Mock<INotificationService>();
            mockUserRoleRepository = new Mock<IRepositoryBase<UserRole>>();

            reportService = new ReportService(
                mockReportRepository.Object,
                mockArticleService.Object,
                mockConfiguration.Object,
                mockNotificationService.Object,
                mockUserRoleRepository.Object
            );
        }

        [Test]
        public void ReportNewsArticle_WhenArticleDoesNotExist_ThrowsNotFoundException()
        {
            var reportRequest = new ReportRequestDTO
            {
                ArticleId = 999,
                Reason = "Inappropriate content"
            };
            var userId = 1;

            mockArticleService
                .Setup(service => service.IsNewsArticleExist(reportRequest.ArticleId))
                .ReturnsAsync(false);

            var exception = Assert.ThrowsAsync<NotFoundException>(async () =>
                await reportService.CreateArticleReportAsync(reportRequest, userId));

            Assert.IsTrue(exception.Message.Contains(reportRequest.ArticleId.ToString()));
            mockReportRepository.Verify(repo => repo.AddAsync(It.IsAny<ReportedArticle>()), Times.Never);
            mockNotificationService.Verify(service => service.NotifyAdminAboutReportedArticleAsync(It.IsAny<ReportRequestDTO>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task ReportNewsArticle_WhenArticleAlreadyReportedByUser_ReturnsZero()
        {
            var reportRequest = new ReportRequestDTO
            {
                ArticleId = 123,
                Reason = "Inappropriate content"
            };
            var userId = 1;

            var existingReport = new ReportedArticle
            {
                Id = 1,
                ArticleId = reportRequest.ArticleId,
                UserId = userId,
                Reason = "Previous report"
            };

            mockArticleService
                .Setup(service => service.IsNewsArticleExist(reportRequest.ArticleId))
                .ReturnsAsync(true);

            mockReportRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<ReportedArticle, bool>>>()))
                .Returns(new List<ReportedArticle> { existingReport }.AsQueryable().BuildMockDbSet().Object);

            var result = await reportService.CreateArticleReportAsync(reportRequest, userId);

            Assert.AreEqual(0, result);
            mockReportRepository.Verify(repo => repo.AddAsync(It.IsAny<ReportedArticle>()), Times.Never);
            mockNotificationService.Verify(service => service.NotifyAdminAboutReportedArticleAsync(It.IsAny<ReportRequestDTO>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task ReportNewsArticle_WhenBelowThreshold_DoesNotHideArticle()
        {
            var reportRequest = new ReportRequestDTO
            {
                ArticleId = 123,
                Reason = "Inappropriate content"
            };
            var userId = 1;
            var adminUser = new User { Id = 999, FirstName = "Admin" };

            mockArticleService
                .Setup(service => service.IsNewsArticleExist(reportRequest.ArticleId))
                .ReturnsAsync(true);

            mockReportRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<ReportedArticle, bool>>>()))
                .Returns(new List<ReportedArticle>().AsQueryable().BuildMockDbSet().Object);

            mockConfiguration
                .Setup(c => c["ArticlesThresholdValue"])
                .Returns("5");

            var existingReports = new List<ReportedArticle>
            {
                new ReportedArticle { ArticleId = reportRequest.ArticleId, UserId = 2 },
                new ReportedArticle { ArticleId = reportRequest.ArticleId, UserId = 3 }
            };

            mockReportRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<ReportedArticle, bool>>>()))
                .Returns(existingReports.AsQueryable().BuildMockDbSet().Object);

            mockUserRoleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<UserRole, bool>>>()))
                .Returns(new List<UserRole>
                {
                    new UserRole { RoleId = (int)UserRoles.Admin, User = adminUser }
                }.AsQueryable().BuildMockDbSet().Object);

            mockReportRepository
                .Setup(repo => repo.AddAsync(It.IsAny<ReportedArticle>()))
                .ReturnsAsync(1);

            mockNotificationService
                .Setup(service => service.NotifyAdminAboutReportedArticleAsync(reportRequest, userId))
                .ReturnsAsync(1);

            var result = await reportService.CreateArticleReportAsync(reportRequest, userId);

            Assert.AreEqual(0, result);
            mockArticleService.Verify(service => service.HideArticle(It.IsAny<int>()), Times.Never);
        }
    }
}