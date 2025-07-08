using NewsAggregationSystem.Common.DTOs;
using NewsAggregationSystem.Common.DTOs.NotificationPreferences;
using NewsAggregationSystem.Common.DTOs.Notifications;
using NewsAggregationSystem.DAL.Entities;

namespace NewsAggregationSystem.Service.Interfaces
{
    public interface INotificationService
    {
        List<Notification> GenerateNotificationsFromUserPreferences(List<Article> articles, List<NotificationPreferenceDTO> userPreferences);
        Task<int> CreateNotificationsAsync(List<Notification> notifications);
        Task<List<NotificationDTO>> GetUserNotificationsAsync(int userId);
        Task<int> NotifyAdminAboutReportedArticleAsync(ReportRequestDTO report, int userId);
    }
}
