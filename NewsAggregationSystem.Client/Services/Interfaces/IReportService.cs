namespace NewsAggregationSystem.Client.Services.Interfaces
{
    public interface IReportService
    {
        Task CreateArticleReportAsync(int articleId, string reason);
    }
}
