using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NewsAggregationSystem.Common.DTOs.NewsSources;
using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.NewsSources;

namespace NewsAggregationSystem.API.Services.NewsSources
{
    public class NewsSourceService : INewsSourceService
    {
        private readonly INewsSourceRepository repository;
        private readonly IMapper mapper;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();

        public NewsSourceService(INewsSourceRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public async Task<List<NewsSourceDTO>> GetAll()
        {
            var entities = await repository.GetAll().ToListAsync();
            return mapper.Map<List<NewsSourceDTO>>(entities);
        }

        public async Task<NewsSourceDTO> GetById(int Id)
        {
            var entity = await repository.GetWhere(newsSource => newsSource.Id == Id).FirstOrDefaultAsync();
            return entity == null ? null : mapper.Map<NewsSourceDTO>(entity);
        }

        public async Task Add(CreateNewsSourceDTO newsSourceRequest, int UserId)
        {
            var newsSource = mapper.Map<NewsSource>(newsSourceRequest);
            newsSource.IsActive = true;
            newsSource.CreatedDate = dateTimeHelper.CurrentUtcDateTime;
            newsSource.CreatedById = UserId;
            newsSource.LastAccess = dateTimeHelper.GetMinUtcDate;
            await repository.AddAsync(newsSource);
        }

        public async Task Update(int Id, UpdateNewsSourceDTO newsSource, int UserId)
        {
            var entity = await repository.GetWhere(newsSource => newsSource.Id == Id).FirstOrDefaultAsync();
            if (entity == null) throw new KeyNotFoundException("News source not found");

            mapper.Map(newsSource, entity);
            entity.ModifiedDate = DateTime.UtcNow;
            entity.ModifiedById = UserId;
            await repository.UpdateAsync(entity);
        }

        public async Task Delete(int Id)
        {
            var entity = await repository.GetWhere(newsSource => newsSource.Id == Id).FirstOrDefaultAsync();
            if (entity == null) throw new KeyNotFoundException("News source not found");

            await repository.DeleteAsync(entity);
        }
    }
}
