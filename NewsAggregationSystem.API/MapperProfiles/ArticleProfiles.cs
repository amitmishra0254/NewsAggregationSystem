using AutoMapper;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.DAL.Entities;

namespace NewsAggregationSystem.API.MapperProfiles
{
    public class ArticleProfiles : Profile
    {
        public ArticleProfiles()
        {
            CreateMap<Article, ArticleDTO>()
                .ForMember(dest => dest.NewsCategoryName, opt => opt.MapFrom(src => src.NewsCategory.Name))
                .ForMember(dest => dest.SavedCount, opt => opt.MapFrom(src => src.SavedArticles.Count()))
                .ForMember(dest => dest.LikedCount, opt => opt.MapFrom(src => src.ArticleReactions.Select(reaction => reaction.ReactionId == (int)ReactionType.Like).Count()))
                .ForMember(dest => dest.DislikedCount, opt => opt.MapFrom(src => src.ArticleReactions.Select(reaction => reaction.ReactionId == (int)ReactionType.Dislike).Count()));
        }
    }
}
