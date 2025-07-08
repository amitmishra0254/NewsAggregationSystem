using NewsAggregationSystem.Common.DTOs.NewsCategories;

namespace NewsAggregationSystem.Client.Services.Interfaces
{
    public interface INewsCategoryService
    {
        Task CreateNewsCategoryAsync(string category);
        Task ToggleNewsCategoryVisibilityAsync(int categoryId, bool isHidden);
        Task<List<NotificationPreferencesKeywordDTO>> GetAllNewsCategoriesAsync();
    }
}
