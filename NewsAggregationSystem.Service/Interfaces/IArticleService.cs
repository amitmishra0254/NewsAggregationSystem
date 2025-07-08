using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.DAL.Entities;

namespace NewsAggregationSystem.Service.Interfaces
{
    public interface IArticleService
    {
        Task<List<ArticleDTO>> GetUserArticlesAsync(NewsArticleRequestDTO newsArticleRequestDTO, int userId);
        Task<ArticleDTO> GetUserArticleByIdAsync(int articleId, int userId);
        Task<int> DeleteUserSavedArticleAsync(int articleId, int userId);
        Task<List<ArticleDTO>> GetUserSavedArticlesAsync(int userId);
        Task<int> SaveUserArticleAsync(int articleId, int userId);
        Task<int> ReactToArticleAsync(int articleId, int userId, int reactionId);
        Task<bool> IsNewsArticleExist(int articleId);
        Task<int> HideArticle(int articleId);
        Task<int> ToggleArticleVisibilityAsync(int articleId, int userId, bool isHidden);
        Task<List<Article>> GetRecommendedArticles(int userId, List<Article> articles);
    }
}
