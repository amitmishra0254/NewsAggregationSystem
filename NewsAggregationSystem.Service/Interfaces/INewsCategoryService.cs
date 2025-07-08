using NewsAggregationSystem.Common.DTOs.NewsCategories;

namespace NewsAggregationSystem.Service.Interfaces
{
    public interface INewsCategoryService
    {
        Task<int> CreateNewsCategoryAsync(string name, int userId);
        Task<int> ToggleCategoryVisibilityAsync(int categoryId, bool IsHidden);
        Task<List<NotificationPreferencesKeywordDTO>> GetAllNewsCategoriesAsync(string userRole);
    }
}
