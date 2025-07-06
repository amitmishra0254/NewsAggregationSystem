namespace NewsAggregationSystem.Client.Services.Interfaces
{
    public interface IAdminService
    {
        Task AddKeywordToHideArticles(string keyword);
    }
}
