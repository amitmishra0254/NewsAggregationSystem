namespace NewsAggregationSystem.Service.Interfaces
{
    public interface IHiddenArticleKeywordService
    {
        Task<int> Add(string keyword, int userId);
    }
}
