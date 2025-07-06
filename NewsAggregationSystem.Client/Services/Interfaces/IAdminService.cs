namespace NewsAggregationSystem.Client.Services.Interfaces
{
    public interface IAdminService
    {
        Task AddHiddenArticleKeywordAsync(string keyword);
    }
}
