using NewsAggregationSystem.Common.DTOs.Notifications;

namespace NewsAggregationSystem.Client.Services.Interfaces
{
    public interface INotificationServices
    {
        Task<List<NotificationDTO>> GetUserNotificationsAsync();
    }
}
