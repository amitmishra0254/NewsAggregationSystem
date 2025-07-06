using NewsAggregationSystem.Common.DTOs.NotificationPreferences;

namespace NewsAggregationSystem.Service.Interfaces
{
    public interface INotificationPreferenceService
    {
        Task AddNotificationPreferencesPerCategory(int newsCategoryId);
        Task<List<NotificationPreferenceDTO>> GetUserNotificationPreferencesAsync(List<int> userIds);
        Task AddNotificationPreferencesPerUser(int userId);
        Task<int> AddKeyword(string keyword, int categoryId, int userId);
        Task<int> ChangeKeywordStatus(int keywordId, bool isEnable);
        Task<int> ChangeCategoryStatus(int categoryId, bool isEnable, int userId);
    }
}
