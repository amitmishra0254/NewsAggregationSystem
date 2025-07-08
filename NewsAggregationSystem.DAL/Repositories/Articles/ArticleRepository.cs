using Microsoft.EntityFrameworkCore;
using NewsAggregationSystem.Common.DTOs.NewsCategories;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.DAL.DbContexts;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;

namespace NewsAggregationSystem.DAL.Repositories.Articles
{
    public class ArticleRepository : RepositoryBase<Article>, IArticleRepository
    {
        private readonly NewsAggregationSystemContext context;
        public ArticleRepository(NewsAggregationSystemContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<Article> GetArticleById(int Id, int userId)
        {
            var article = await context.Articles.Where(article => article.Id == Id && !article.IsHidden)
                            .Include(article => article.NewsCategory)
                            .FirstOrDefaultAsync();
            return article;
        }

        public async Task<CategoryRecommendationDTO?> GetMostLikedCategory(int userId)
        {
            return await context.ArticleReactions
                .Where(r => r.UserId == userId && r.ReactionId == (int)ReactionType.Like)
                .Select(r => r.Article.NewsCategory)
                .GroupBy(c => c.Id)
                .Select(g => new CategoryRecommendationDTO
                {
                    CategoryId = g.Key,
                    Count = g.Count() + 1,
                    Keywords = new()
                })
                .OrderByDescending(g => g.Count)
                .FirstOrDefaultAsync();
        }

        public async Task<CategoryRecommendationDTO?> GetMostSavedCategory(int userId)
        {
            return await context.SavedArticles
                .Where(s => s.UserId == userId)
                .Select(s => s.Article.NewsCategory)
                .GroupBy(c => c.Id)
                .Select(g => new CategoryRecommendationDTO
                {
                    CategoryId = g.Key,
                    Count = g.Count() + 1,
                    Keywords = new()
                })
                .OrderByDescending(g => g.Count)
                .FirstOrDefaultAsync();
        }

        public async Task<CategoryRecommendationDTO?> GetMostReadCategory(int userId)
        {
            return await context.ArticleReadHistories
                .Where(r => r.UserId == userId)
                .Select(r => r.Article.NewsCategory)
                .GroupBy(c => c.Id)
                .Select(g => new CategoryRecommendationDTO
                {
                    CategoryId = g.Key,
                    Count = g.Count() + 1,
                    Keywords = new()
                })
                .OrderByDescending(g => g.Count)
                .FirstOrDefaultAsync();
        }
    }
}
