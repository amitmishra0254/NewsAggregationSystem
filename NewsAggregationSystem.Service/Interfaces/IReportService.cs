using NewsAggregationSystem.Common.DTOs;

namespace NewsAggregationSystem.Service.Interfaces
{
    public interface IReportService
    {
        Task<int> CreateArticleReportAsync(ReportRequestDTO report, int userId);
    }
}
