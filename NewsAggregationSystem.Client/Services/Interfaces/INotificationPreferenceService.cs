using NewsAggregationSystem.Common.DTOs.NotificationPreferences;

namespace NewsAggregationSystem.Client.Services.Interfaces
{
    public interface INotificationPreferenceService
    {
        Task<List<NotificationPreferenceDTO>> GetUserNotificationPreferences();
        Task AddKeyword(string keyword, int categoryId);
        Task ChangeKeywordStatus(int keywordId, bool isEnable);
        Task ChangeCategoryStatus(int categoryId, bool isEnable);
    }
}
