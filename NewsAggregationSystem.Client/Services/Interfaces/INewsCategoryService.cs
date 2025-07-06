using NewsAggregationSystem.Common.DTOs.NewsCategories;

namespace NewsAggregationSystem.Client.Services.Interfaces
{
    public interface INewsCategoryService
    {
        Task CreateNewsCategoryAsync(string category);
        Task ToggleCategoryVisibilityAsync(int categoryId, bool isHidden);
        Task<List<NotificationPreferencesKeywordDTO>> GetAllNewsCategoriesAsync();
    }
}
