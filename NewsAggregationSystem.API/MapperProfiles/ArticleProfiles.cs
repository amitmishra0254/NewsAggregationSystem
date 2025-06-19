using AutoMapper;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.DAL.Entities;

namespace NewsAggregationSystem.API.MapperProfiles
{
    public class ArticleProfiles : Profile
    {
        public ArticleProfiles()
        {
            CreateMap<Article, ArticleDTO>()
                .ForMember(dest => dest.NewsCategoryName, opt => opt.MapFrom(src => src.NewsCategory.Name));
        }
    }
}
