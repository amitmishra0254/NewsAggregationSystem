namespace NewsAggregationSystem.API.Services.NewsCategories
{
    public interface INewsCategoryService
    {
        Task<int> AddNewsCategory(string name, int userId);
    }
}
