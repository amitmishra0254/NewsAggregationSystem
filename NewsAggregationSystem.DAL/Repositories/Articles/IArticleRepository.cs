using NewsAggregationSystem.Common.DTOs.NewsCategories;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;

namespace NewsAggregationSystem.DAL.Repositories.Articles
{
    public interface IArticleRepository : IRepositoryBase<Article>
    {
        Task<CategoryRecommendationDTO?> GetMostLikedCategory(int userId);
        Task<CategoryRecommendationDTO?> GetMostSavedCategory(int userId);
        Task<CategoryRecommendationDTO?> GetMostReadCategory(int userId);
        Task<Article> GetArticleById(int Id, int userId);
    }
}
