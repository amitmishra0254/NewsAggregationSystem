using NewsAggregationSystem.Common.DTOs.NewsCategories;

namespace NewsAggregationSystem.Client.Services.Interfaces
{
    public interface INewsCategoryService
    {
        Task AddNewsCategory(string category);
        Task ToggleNewsCategoryVisibility(int categoryId, bool isHidden);
        Task<List<NotificationPreferencesKeywordDTO>> GetAllNewsCategories();
    }
}
