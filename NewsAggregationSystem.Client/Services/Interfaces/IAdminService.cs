namespace NewsAggregationSystem.Client.Services.Interfaces
{
    public interface IAdminService
    {
        Task AddKeywordToHideArticlesAsync(string keyword);
    }
}
