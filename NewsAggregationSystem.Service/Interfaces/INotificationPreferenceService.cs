using NewsAggregationSystem.Common.DTOs.NotificationPreferences;

namespace NewsAggregationSystem.Service.Interfaces
{
    public interface INotificationPreferenceService
    {
        Task AddNotificationPreferencesPerCategory(int newsCategoryId);
        Task<List<NotificationPreferenceDTO>> GetUserNotificationPreferencesAsync(List<int> userIds);
        Task AddNotificationPreferencesPerUser(int userId);
        Task<int> AddKeywordToCategoryAsync(string keyword, int categoryId, int userId);
        Task<int> UpdateKeywordStatusAsync(int keywordId, bool isEnable);
        Task<int> UpdateCategoryStatusAsync(int categoryId, bool isEnable, int userId);
    }
}
