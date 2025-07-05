using NewsAggregationSystem.Common.DTOs.NewsCategories;

namespace NewsAggregationSystem.Client.Services.NewsCategory
{
    public interface INewsCategoryService
    {
        Task AddNewsCategory(string category);
        Task ToggleNewsCategoryVisibility(int categoryId, bool isHidden);
        Task<List<NotificationPreferencesKeywordDTO>> GetAllNewsCategories();
    }
}
