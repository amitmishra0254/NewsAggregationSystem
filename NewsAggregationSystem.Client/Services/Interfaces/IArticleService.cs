using NewsAggregationSystem.Common.DTOs.NewsArticles;

namespace NewsAggregationSystem.Client.Services.Interfaces
{
    public interface IArticleService
    {
        Task<List<ArticleDTO>> GetUserSavedArticlesAsync();
        Task<List<ArticleDTO>> GetUserArticlesAsync(NewsArticleRequestDTO request);
        Task ReactToArticleAsync(int articleId, int reactionId);
        Task SaveArticleAsync(int articleId);
        Task RemoveSavedArticleAsync(int articleId);
        Task ToggleArticleVisibilityAsync(int articleId, bool isHidden);
        Task<ArticleDTO?> GetArticleByIdAsync(int articleId);
    }
}
