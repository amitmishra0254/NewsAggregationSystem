using NewsAggregationSystem.Common.DTOs.NotificationPreferences;

namespace NewsAggregationSystem.API.Services.NotificationPreferences
{
    public interface INotificationPreferenceService
    {
        Task AddNotificationPreferencesPerCategory(int newsCategoryId);
        Task<List<NotificationPreferenceDTO>> GetNotificationPreferences(List<int> userIds);
        Task AddNotificationPreferencesPerUser(int userId);
    }
}
