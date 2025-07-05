using NewsAggregationSystem.Common.DTOs.Notifications;

namespace NewsAggregationSystem.Client.Services.Notifications
{
    public interface INotificationServices
    {
        Task<List<NotificationDTO>> GetAllNotifications();
    }
}
