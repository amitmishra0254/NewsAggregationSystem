using NewsAggregationSystem.Common.DTOs.NewsArticles;

namespace NewsAggregationSystem.API.Services.Articles
{
    public interface IArticleService
    {
        Task<List<ArticleDTO>> GetAllArticles(NewsArticleRequestDTO newsArticleRequestDTO);
        Task<ArticleDTO> GetArticleById(int Id);
        Task<int> DeleteSavedArticles(int articleId, int userId);
        Task<List<ArticleDTO>> GetAllSavedArticles(int userId);
        Task<int> SaveArticle(int articleId, int userId);
    }
}
