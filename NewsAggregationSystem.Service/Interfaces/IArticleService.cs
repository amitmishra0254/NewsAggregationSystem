using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.DAL.Entities;

namespace NewsAggregationSystem.Service.Interfaces
{
    public interface IArticleService
    {
        Task<List<ArticleDTO>> GetAllArticles(NewsArticleRequestDTO newsArticleRequestDTO, int userId);
        Task<ArticleDTO> GetArticleById(int Id, int userId);
        Task<int> DeleteSavedArticles(int articleId, int userId);
        Task<List<ArticleDTO>> GetAllSavedArticles(int userId);
        Task<int> SaveArticle(int articleId, int userId);
        Task<int> ReactArticle(int articleId, int userId, int reactionId);
        Task<bool> IsNewsArticleExist(int articleId);
        Task<int> HideArticle(int articleId);
        Task<int> ToggleVisibility(int articleId, int userId, bool IsHidden);
        Task<List<Article>> GetRecommendedArticles(int userId, List<Article> articles);
    }
}
