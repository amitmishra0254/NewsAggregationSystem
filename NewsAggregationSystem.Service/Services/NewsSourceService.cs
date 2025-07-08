using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsSources;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.NewsSources;
using NewsAggregationSystem.Service.Interfaces;

namespace NewsAggregationSystem.Service.Services
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

        public async Task<List<NewsSourceDTO>> GetAllNewsSourcesAsync()
        {
            var entities = await repository.GetAll().ToListAsync();
            return mapper.Map<List<NewsSourceDTO>>(entities);
        }

        public async Task<NewsSourceDTO> GetNewsSourceByIdAsync(int newsSourceId)
        {
            var entity = await repository.GetWhere(newsSource => newsSource.Id == newsSourceId).FirstOrDefaultAsync();
            return entity == null ? null : mapper.Map<NewsSourceDTO>(entity);
        }

        public async Task CreateNewsSourceAsync(CreateNewsSourceDTO newsSourceRequest, int userId)
        {
            var newsSource = mapper.Map<NewsSource>(newsSourceRequest);
            newsSource.IsActive = false;
            newsSource.CreatedDate = dateTimeHelper.CurrentUtcDateTime;
            newsSource.CreatedById = userId;
            newsSource.LastAccess = dateTimeHelper.GetMinUtcDate;
            await repository.AddAsync(newsSource);
        }

        public async Task UpdateNewsSourceAsync(int newsSourceId, UpdateNewsSourceDTO newsSource, int userId)
        {
            var entity = await repository.GetWhere(newsSource => newsSource.Id == newsSourceId).FirstOrDefaultAsync();
            if (entity == null) throw new NotFoundException(ApplicationConstants.NewsSourceNotFoundMessage);

            entity.ApiKey = newsSource.ApiKey;
            entity.IsActive = newsSource.IsActive;
            entity.ModifiedById = userId;
            entity.ModifiedDate = dateTimeHelper.CurrentUtcDateTime;
            await repository.UpdateAsync(entity);
        }

        public async Task DeleteNewsSourceAsync(int newsSourceId)
        {
            var entity = await repository.GetWhere(newsSource => newsSource.Id == newsSourceId).FirstOrDefaultAsync();
            if (entity == null) throw new NotFoundException(ApplicationConstants.NewsSourceNotFoundMessage);

            await repository.DeleteAsync(entity);
        }
    }
}
