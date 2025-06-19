using NewsAggregationSystem.Common.DTOs.NewsSources;

namespace NewsAggregationSystem.API.Services.NewsSources
{
    public interface INewsSourceService
    {
        Task<List<NewsSourceDTO>> GetAll();
        Task<NewsSourceDTO> GetById(int Id);
        Task Add(CreateNewsSourceDTO newsSource, int UserId);
        Task Update(int Id, UpdateNewsSourceDTO newsSource, int UserId);
        Task Delete(int Id);
    }
}
