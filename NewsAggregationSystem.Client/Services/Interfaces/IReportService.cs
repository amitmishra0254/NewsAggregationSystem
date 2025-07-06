namespace NewsAggregationSystem.Client.Services.Interfaces
{
    public interface IReportService
    {
        Task ReportNewsArticle(int articleId);
    }
}
