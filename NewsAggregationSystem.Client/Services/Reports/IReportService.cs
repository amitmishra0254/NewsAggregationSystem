namespace NewsAggregationSystem.Client.Services.Reports
{
    public interface IReportService
    {
        Task ReportNewsArticle(int articleId);
    }
}
