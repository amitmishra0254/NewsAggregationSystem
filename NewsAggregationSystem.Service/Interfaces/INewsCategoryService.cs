using NewsAggregationSystem.Common.DTOs.NewsCategories;

namespace NewsAggregationSystem.Service.Interfaces
{
    public interface INewsCategoryService
    {
        Task<int> AddNewsCategory(string name, int userId);
        Task<int> ToggleVisibility(int categoryId, bool IsHidden);
        Task<List<NotificationPreferencesKeywordDTO>> GetAllCategories(string userRole);
    }
}
