using NewsAggregationSystem.Common.DTOs;

namespace NewsAggregationSystem.Service.Interfaces
{
    public interface IReportService
    {
        Task<int> ReportNewsArticle(ReportRequestDTO report, int userId);
    }
}
