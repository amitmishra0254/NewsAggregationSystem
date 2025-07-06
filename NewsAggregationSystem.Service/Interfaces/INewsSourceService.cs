using NewsAggregationSystem.Common.DTOs.NewsSources;

namespace NewsAggregationSystem.Service.Interfaces
{
    public interface INewsSourceService
    {
        Task<List<NewsSourceDTO>> GetAllNewsSourcesAsync();
        Task<NewsSourceDTO> GetNewsSourceByIdAsync(int newsSourceId);
        Task CreateNewsSourceAsync(CreateNewsSourceDTO newsSource, int userId);
        Task UpdateNewsSourceAsync(int newsSourceId, UpdateNewsSourceDTO newsSource, int userId);
        Task DeleteNewsSourceAsync(int newsSourceId);
    }
}
