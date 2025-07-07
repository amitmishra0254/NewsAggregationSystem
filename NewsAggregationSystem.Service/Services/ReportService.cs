using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Common.Providers.SmtpProvider;
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
        private readonly IEmailProvider emailProvider;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();

        public ReportService(IReportRepository reportRepository, IArticleService articleService, IConfiguration configuration, INotificationService notificationService, IRepositoryBase<UserRole> userRoleRepository, IEmailProvider emailProvider)
        {
            this.reportRepository = reportRepository;
            this.articleService = articleService;
            this.configuration = configuration;
            this.notificationService = notificationService;
            this.userRoleRepository = userRoleRepository;
            this.emailProvider = emailProvider;
        }

        public async Task<int> CreateArticleReportAsync(ReportRequestDTO reportRequest, int userId)
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
                    .FirstAsync();

                await reportRepository.AddAsync(new ReportedArticle
                {
                    ArticleId = reportRequest.ArticleId,
                    UserId = userId,
                    Reason = reportRequest.Reason,
                    CreatedById = userId,
                    CreatedDate = dateTimeHelper.CurrentUtcDateTime
                });

                await NotifyAdminAboutReportedArticleAsync(admin, reportRequest);
                return await notificationService.NotifyAdminAboutReportedArticleAsync(reportRequest, admin.Id);
            }
            return 0;
        }

        private async Task NotifyAdminAboutReportedArticleAsync(User admin, ReportRequestDTO reportRequest)
        {
            var emailBody = $"Article Id : {reportRequest.ArticleId} has been reported with the reason : " + reportRequest.Reason;
            await emailProvider.SendEmailAsync(admin.Email, ApplicationConstants.ReportTitle, emailBody);
        }
    }
}
