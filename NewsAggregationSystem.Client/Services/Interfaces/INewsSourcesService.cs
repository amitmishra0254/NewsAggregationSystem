using NewsAggregationSystem.Common.DTOs.NewsSources;

namespace NewsAggregationSystem.Client.Services.Interfaces
{
    public interface INewsSourcesService
    {
        Task CreateNewsSourceAsync(CreateNewsSourceDTO newsSource);
        Task<List<NewsSourceDTO>> GetAllNewsSourcesAsync();
        Task UpdateNewsSourceAsync(int Id, UpdateNewsSourceDTO updateNewsSourceDTO);
    }
}
