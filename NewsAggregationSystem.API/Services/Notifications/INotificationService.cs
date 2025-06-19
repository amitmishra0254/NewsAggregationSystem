using NewsAggregationSystem.Common.DTOs.NotificationPreferences;
using NewsAggregationSystem.Common.DTOs.Notifications;
using NewsAggregationSystem.DAL.Entities;

namespace NewsAggregationSystem.API.Services.Notifications
{
    public interface INotificationService
    {
        List<Notification> GenerateNotificationsFromUserPreferences(List<Article> articles, List<NotificationPreferenceDTO> userPreferences);
        Task<int> AddNotifications(List<Notification> notifications);
        Task<List<GetAllNotificationsDTO>> GetAllNotifications(int userId);
        Task<int> MarkAllNotificationsAsRead(List<Notification> notifications);
    }
}
