using AutoMapper;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NewsAggregationSystem.Common.DTOs.NewsSources;
using NewsAggregationSystem.DAL.Entities;

namespace NewsAggregationSystem.API.MapperProfiles
{
    public class NewsSourceProfiles : Profile
    {
        public NewsSourceProfiles()
        {
            CreateMap<NewsSource, NewsSourceDTO>().ReverseMap();
            CreateMap<NewsSource, CreateNewsSourceDTO>().ReverseMap();
            CreateMap<NewsSource, UpdateNewsSourceDTO>().ReverseMap();
        }
    }
}
