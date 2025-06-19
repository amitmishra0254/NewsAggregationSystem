using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NotificationPreferences;
using NewsAggregationSystem.Common.DTOs.Notifications;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Notifications;
using System.Text;

namespace NewsAggregationSystem.API.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository notificationRepository;
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;

        public NotificationService(INotificationRepository notificationRepository, IConfiguration configuration, IMapper mapper)
        {
            this.notificationRepository = notificationRepository;
            this.configuration = configuration;
            this.mapper = mapper;
        }

        public List<Notification> GenerateNotificationsFromUserPreferences(List<Article> articles, List<NotificationPreferenceDTO> userPreferences)
        {
            var notifications = new List<Notification>();

            foreach (var user in userPreferences)
            {
                var matchedArticles = new List<Article>();

                foreach (var category in user.NewsCategories)
                {
                    var categoryName = category.Name.Trim().ToLower();

                    if (category.Keywords != null && category.Keywords.Any())
                    {
                        var keywords = category.Keywords
                            .Where(k => !string.IsNullOrWhiteSpace(k))
                            .Select(k => k.ToLower());

                        foreach (var article in articles)
                        {
                            var content = $"{article.Title} {article.Description}".ToLower();

                            if (keywords.Any(k => content.Contains(k)))
                            {
                                matchedArticles.Add(article);
                            }
                        }
                    }
                    else
                    {
                        var categoryArticles = articles
                            .Where(a => a.NewsCategory.Name.Trim().ToLower() == categoryName)
                            .ToList();
                        matchedArticles.AddRange(categoryArticles);
                    }
                }

                if (matchedArticles.Any())
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine($"<b>{ApplicationConstants.NewArticleNotificationTitle}:</b><br><br>");

                    foreach (var article in matchedArticles.DistinctBy(a => a.Id))
                    {
                        stringBuilder.AppendLine($"{article.Title}: {configuration["ArticleRequestUrl"]}{article.Id}");
                    }

                    stringBuilder.AppendLine("<br>Stay informed with the latest updates!");

                    notifications.Add(new Notification
                    {
                        UserId = user.UserId,
                        Title = ApplicationConstants.NewArticleNotificationTitle,
                        Message = stringBuilder.ToString(),
                        IsRead = false,
                        CreatedById = ApplicationConstants.SystemUserId,
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            return notifications;
        }

        public async Task<int> AddNotifications(List<Notification> notifications)
        {
            return await notificationRepository.AddRangeAsync(notifications);
        }

        public async Task<List<GetAllNotificationsDTO>> GetAllNotifications(int userId)
        {
            var notifications = await notificationRepository.GetWhere(notification => notification.IsRead == false && notification.UserId == userId).ToListAsync();\
            notifications.ForEach(notification => notification.IsRead = true);
            await notificationRepository.SaveAsync();
            return mapper.Map<List<GetAllNotificationsDTO>>(notifications);
        }
    }
}
