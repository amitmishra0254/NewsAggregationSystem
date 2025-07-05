using NewsAggregationSystem.Common.DTOs;
using NewsAggregationSystem.Common.DTOs.NotificationPreferences;
using NewsAggregationSystem.Common.DTOs.Notifications;
using NewsAggregationSystem.DAL.Entities;

namespace NewsAggregationSystem.Service.Interfaces
{
    public interface INotificationService
    {
        List<Notification> GenerateNotificationsFromUserPreferences(List<Article> articles, List<NotificationPreferenceDTO> userPreferences);
        Task<int> AddNotifications(List<Notification> notifications);
        Task<List<NotificationDTO>> GetAllNotifications(int userId);
        Task<int> NotifyAdminAboutReportedArticle(ReportRequestDTO report, int userId);
    }
}
