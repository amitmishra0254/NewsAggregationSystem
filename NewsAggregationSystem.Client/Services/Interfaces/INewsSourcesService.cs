using NewsAggregationSystem.Common.DTOs.NewsSources;

namespace NewsAggregationSystem.Client.Services.Interfaces
{
    public interface INewsSourcesService
    {
        Task AddNewsSource(CreateNewsSourceDTO newsSource);
        Task<List<NewsSourceDTO>> GetAllNewsSource();
        Task UpdateNewsSource(int Id, UpdateNewsSourceDTO updateNewsSourceDTO);
    }
}
