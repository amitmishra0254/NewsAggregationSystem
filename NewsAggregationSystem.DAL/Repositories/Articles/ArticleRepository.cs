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

        /*public async Task<List<Article>> GetRecommendedArticles(int userId)
        {
            var maxLikedCategory = await context.ArticleReactions
                .Where(articleReaction => articleReaction.UserId == userId)
                .Include(articleReaction => articleReaction.Article)
                    .ThenInclude(article => article.NewsCategory)
                .Select(articleReaction => articleReaction.Article.NewsCategory)
                .GroupBy(category => category.Id)
                .Select(categoryGroup => new CategoryRecommendationDTO
                {
                    CategoryId = categoryGroup.Key,
                    Count = categoryGroup.Count()
                })
                .OrderByDescending(categoryGroup => categoryGroup.Count)
                .FirstOrDefaultAsync();

            var maxSavedCategory = await context.SavedArticles
                .Where(savedArticle => savedArticle.UserId == userId)
                .Include(savedArticle => savedArticle.Article)
                    .ThenInclude(article => article.NewsCategory)
                .Select(savedArticle => savedArticle.Article.NewsCategory)
                .GroupBy(category => category.Id)
                .Select(categoryGroup => new CategoryRecommendationDTO
                {
                    CategoryId = categoryGroup.Key,
                    Count = categoryGroup.Count()
                })
                .OrderByDescending(categoryGroup => categoryGroup.Count)
                .FirstOrDefaultAsync();

            var maxReadCategory = await context.ArticleReadHistories
                .Where(articleRead => articleRead.UserId == userId)
                .Include(articleRead => articleRead.Article)
                    .ThenInclude(article => article.NewsCategory)
                .Select(articleRead => articleRead.Article.NewsCategory)
                .GroupBy(category => category.Id)
                .Select(categoryGroup => new CategoryRecommendationDTO
                {
                    CategoryId = categoryGroup.Key,
                    Count = categoryGroup.Count()
                })
                .OrderByDescending(categoryGroup => categoryGroup.Count)
                .FirstOrDefaultAsync();

            var notificationPreference = await context.NotificationPreferences
                .Include(preference => preference.NewsCategory)
                .Where(preference => !preference.NewsCategory.IsHidden && preference.IsEnabled)
                .Select(preference => preference.NewsCategory)
                .GroupBy(category => category.Id)
                .Select(category => new
                {
                    CategoryId = category.Key,
                    Keywords = context.UserNewsKeywords
                        .Where(keyword => keyword.UserId == userId && keyword.NewsCategoryId == category.Key && keyword.IsEnabled)
                        .Select(keyword => keyword.Name)
                        .ToList()
                })
                .ToListAsync();

            if (maxLikedCategory != null)
            {
                var likedArticles = await context.Articles
                    .Where(article => !article.IsHidden && article.NewsCategoryId == maxLikedCategory.CategoryId)
                    .ToListAsync();
                var preference = notificationPreference
                    .FirstOrDefault(pref => pref.CategoryId == maxLikedCategory.CategoryId && pref.Keywords.Any());

                if (preference != null)
                {
                    var keywords = preference.Keywords
                        .Select(keyword => keyword.ToLower())
                        .ToList();

                    likedArticles = likedArticles
                        .OrderByDescending(article =>
                            keywords.Count(keyword =>
                                (!string.IsNullOrEmpty(article.Title) && article.Title.ToLower().Contains(keyword)) ||
                                (!string.IsNullOrEmpty(article.Description) && article.Description.ToLower().Contains(keyword)) ||
                                (!string.IsNullOrEmpty(article.Content) && article.Content.ToLower().Contains(keyword))
                            )
                        ).ToList();
                    notificationPreference.Remove(preference);
                }
            }
        }*/

        public async Task<CategoryRecommendationDTO?> GetMostLikedCategory(int userId)
        {
            return await context.ArticleReactions
                .Where(r => r.UserId == userId && r.ReactionId == (int)ReactionType.Like)
                .Select(r => r.Article.NewsCategory)
                .GroupBy(c => c.Id)
                .Select(g => new CategoryRecommendationDTO
                {
                    CategoryId = g.Key,
                    Count = g.Count(),
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
                    Count = g.Count(),
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
                    Count = g.Count(),
                    Keywords = new()
                })
                .OrderByDescending(g => g.Count)
                .FirstOrDefaultAsync();
        }


        /*private async Task<List<Article>> AddArticlesByCategory(int categoryId)
        {
            var articles = await context.Articles
                .Where(a => !a.IsHidden && a.NewsCategoryId == categoryId)
                .ToListAsync();

            var preference = notificationPreference
                .FirstOrDefault(p => p.CategoryId == categoryId && p.Keywords.Any());

            if (preference != null)
            {
                var keywords = preference.Keywords.Select(k => k.ToLower()).ToList();

                articles = articles
                    .OrderByDescending(article =>
                        keywords.Count(keyword =>
                            (!string.IsNullOrEmpty(article.Title) && article.Title.ToLower().Contains(keyword)) ||
                            (!string.IsNullOrEmpty(article.Description) && article.Description.ToLower().Contains(keyword)) ||
                            (!string.IsNullOrEmpty(article.Content) && article.Content.ToLower().Contains(keyword))
                        )
                    ).ToList();

                notificationPreference.Remove(preference); // Avoid reuse
            }

            recommendedArticles.AddRange(articles);
        }*/
    }
}
