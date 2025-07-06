using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs;
using NewsAggregationSystem.Common.DTOs.NotificationPreferences;
using NewsAggregationSystem.Common.DTOs.Notifications;
using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Notifications;
using NewsAggregationSystem.Service.Interfaces;
using System.Text;

namespace NewsAggregationSystem.Service.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository notificationRepository;
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();

        public NotificationService(INotificationRepository notificationRepository, IConfiguration configuration, IMapper mapper, INotificationPreferenceService notificationPreferenceService)
        {
            this.notificationRepository = notificationRepository;
            this.configuration = configuration;
            this.mapper = mapper;
        }

        public async Task<int> CreateNotificationsAsync(List<Notification> notifications)
        {
            return await notificationRepository.AddRangeAsync(notifications);
        }

        public async Task<List<NotificationDTO>> GetUserNotificationsAsync(int userId)
        {
            var notifications = await notificationRepository.GetWhere(notification => notification.IsRead == false && notification.UserId == userId).ToListAsync();
            notifications.ForEach(notification => notification.IsRead = true);
            await notificationRepository.SaveAsync();
            return mapper.Map<List<NotificationDTO>>(notifications);
        }

        public List<Notification> GenerateNotificationsFromUserPreferences(List<Article> articles, List<NotificationPreferenceDTO> userPreferences)
        {
            var notifications = new List<Notification>();

            foreach (var user in userPreferences)
            {
                var matchedArticles = MatchArticlesForUser(user, articles);

                if (matchedArticles.Any())
                {
                    var message = BuildNotificationMessage(matchedArticles);
                    var notification = CreateNotification(user.UserId, message);
                    notifications.Add(notification);
                }
            }
            return notifications;
        }

        public async Task<int> NotifyAdminAboutReportedArticleAsync(ReportRequestDTO report, int userId)
        {
            var notification = new Notification
            {
                UserId = userId,
                IsRead = false,
                Title = ApplicationConstants.ReportTitle,
                Message = $"Article Id : {report.ArticleId} has been reported with the reason : " + report.Reason,
                CreatedById = userId,
                CreatedDate = dateTimeHelper.CurrentUtcDateTime
            };
            return await notificationRepository.AddAsync(notification);
        }

        private Notification CreateNotification(int userId, string message)
        {
            return new Notification
            {
                UserId = userId,
                Title = ApplicationConstants.NewArticleNotificationTitle,
                Message = message,
                IsRead = false,
                CreatedById = ApplicationConstants.SystemUserId,
                CreatedDate = DateTime.UtcNow
            };
        }

        private string BuildNotificationMessage(List<Article> articles)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{ApplicationConstants.NewArticleNotificationTitle}\n");

            foreach (var article in articles)
            {
                stringBuilder.AppendLine($"{article.Title}: {article.Url}\n");
            }

            stringBuilder.AppendLine("Stay informed with the latest updates!\n");

            return stringBuilder.ToString();
        }

        private List<Article> MatchArticlesForUser(NotificationPreferenceDTO preference, List<Article> articles)
        {
            var matchedArticles = new List<Article>();

            var enabledCategories = preference.NewsCategories?.Where(c => c.IsEnabled).ToList();
            if (enabledCategories == null || !enabledCategories.Any()) return matchedArticles;

            foreach (var category in enabledCategories)
            {
                var categoryName = category.Name.Trim().ToLower();

                if (category.Keywords != null && category.Keywords.Any(k => k.IsEnabled && !string.IsNullOrWhiteSpace(k.Name)))
                {
                    var keywords = category.Keywords
                        .Where(k => k.IsEnabled)
                        .Select(k => k.Name.ToLower());

                    var keywordMatches = articles
                        .Where(article => keywords.Any(k =>
                            $"{article.Title} {article.Description}".ToLower().Contains(k)))
                        .ToList();

                    matchedArticles.AddRange(keywordMatches);
                }
                else
                {
                    var categoryMatches = articles
                        .Where(article => article.NewsCategory.Name.Trim().ToLower() == categoryName)
                        .ToList();

                    matchedArticles.AddRange(categoryMatches);
                }
            }
            return matchedArticles.DistinctBy(a => a.Id).ToList();
        }
    }
}
