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
            newsSource.IsActive = false;
            newsSource.CreatedDate = dateTimeHelper.CurrentUtcDateTime;
            newsSource.CreatedById = UserId;
            newsSource.LastAccess = dateTimeHelper.GetMinUtcDate;
            await repository.AddAsync(newsSource);
        }

        public async Task Update(int Id, UpdateNewsSourceDTO newsSource, int UserId)
        {
            var entity = await repository.GetWhere(newsSource => newsSource.Id == Id).FirstOrDefaultAsync();
            if (entity == null) throw new NotFoundException(ApplicationConstants.NewsSourceNotFoundMessage);

            entity.ApiKey = newsSource.ApiKey;
            entity.IsActive = newsSource.IsActive;
            entity.ModifiedById = UserId;
            entity.ModifiedDate = dateTimeHelper.CurrentUtcDateTime;
            await repository.UpdateAsync(entity);
        }

        public async Task Delete(int Id)
        {
            var entity = await repository.GetWhere(newsSource => newsSource.Id == Id).FirstOrDefaultAsync();
            if (entity == null) throw new NotFoundException(ApplicationConstants.NewsSourceNotFoundMessage);

            await repository.DeleteAsync(entity);
        }
    }
}
