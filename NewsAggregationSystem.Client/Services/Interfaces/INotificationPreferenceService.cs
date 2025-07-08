using NewsAggregationSystem.Common.DTOs.NotificationPreferences;

namespace NewsAggregationSystem.Client.Services.Interfaces
{
    public interface INotificationPreferenceService
    {
        Task<List<NotificationPreferenceDTO>> GetUserNotificationPreferencesAsync();
        Task AddKeywordToCategoryAsync(string keyword, int categoryId);
        Task UpdateKeywordStatusAsync(int keywordId, bool isEnable);
        Task UpdateCategoryStatusAsync(int categoryId, bool isEnable);
    }
}
