using NewsAggregationSystem.Common.DTOs.NewsArticles;

namespace NewsAggregationSystem.Client.Services.Interfaces
{
    public interface IArticleService
    {
        Task<List<ArticleDTO>> GetSavedArticles();
        Task<List<ArticleDTO>> GetAllArticles(NewsArticleRequestDTO request);
        Task ReactArticle(int articleId, int reactionId);
        Task SaveArticle(int articleId);
        Task DeleteSavedArticle(int articleId);
        Task ToggleArticleVisibility(int articleId, bool isHidden);
        Task<ArticleDTO?> GetArticleById(int articleId);
    }
}
