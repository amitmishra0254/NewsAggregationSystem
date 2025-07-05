using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;
using NewsAggregationSystem.DAL.Repositories.Reports;
using NewsAggregationSystem.Service.Interfaces;

namespace NewsAggregationSystem.Service.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository reportRepository;
        private readonly IArticleService articleService;
        private readonly IConfiguration configuration;
        private readonly INotificationService notificationService;
        private readonly IRepositoryBase<UserRole> userRoleRepository;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();

        public ReportService(IReportRepository reportRepository, IArticleService articleService, IConfiguration configuration, INotificationService notificationService, IRepositoryBase<UserRole> userRoleRepository)
        {
            this.reportRepository = reportRepository;
            this.articleService = articleService;
            this.configuration = configuration;
            this.notificationService = notificationService;
            this.userRoleRepository = userRoleRepository;
        }

        public async Task<int> ReportNewsArticle(ReportRequestDTO reportRequest, int userId)
        {
            if (!await articleService.IsNewsArticleExist(reportRequest.ArticleId))
            {
                throw new NotFoundException(string.Format(ApplicationConstants.ArticleNotFoundWithThisId, reportRequest.ArticleId));
            }
            var existingReport = await reportRepository.GetWhere(report => report.UserId == userId && report.ArticleId == reportRequest.ArticleId).FirstOrDefaultAsync();

            if (existingReport == null)
            {
                var thresholdValue = Convert.ToInt32(configuration["ArticlesThresholdValue"]);
                var countOfReports = await reportRepository.GetWhere(report => report.ArticleId == report.ArticleId).CountAsync();
                if (countOfReports + 1 > thresholdValue)
                {
                    await articleService.HideArticle(reportRequest.ArticleId);
                }

                var admin = await userRoleRepository.GetWhere(userRole => userRole.RoleId == (int)UserRoles.Admin)
                    .Include(userRole => userRole.User)
                    .Select(userRole => userRole.User)
                    .FirstOrDefaultAsync();

                await reportRepository.AddAsync(new ReportedArticle
                {
                    ArticleId = reportRequest.ArticleId,
                    UserId = admin?.Id ?? userId,
                    Reason = reportRequest.Reason,
                    CreatedById = userId,
                    CreatedDate = dateTimeHelper.CurrentUtcDateTime
                });

                return await notificationService.NotifyAdminAboutReportedArticle(reportRequest, userId);
            }
            return 0;
        }
    }
}
