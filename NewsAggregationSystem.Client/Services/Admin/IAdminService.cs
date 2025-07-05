namespace NewsAggregationSystem.Client.Services.Admin
{
    public interface IAdminService
    {
        Task AddKeywordToHideArticles(string keyword);
    }
}
