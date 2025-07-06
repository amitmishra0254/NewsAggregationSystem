namespace NewsAggregationSystem.Service.Interfaces
{
    public interface IHiddenArticleKeywordService
    {
        Task<int> AddHiddenKeywordAsync(string keyword, int userId);
    }
}
