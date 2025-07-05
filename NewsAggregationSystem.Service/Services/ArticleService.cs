using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.Common.DTOs.NewsCategories;
using NewsAggregationSystem.Common.DTOs.NotificationPreferences;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Articles;
using NewsAggregationSystem.DAL.Repositories.Generic;
using NewsAggregationSystem.Service.Interfaces;
using System.Linq.Expressions;

namespace NewsAggregationSystem.Service.Services
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository articleRepository;
        private readonly IMapper mapper;
        private readonly IRepositoryBase<SavedArticle> savedArticleRepository;
        private readonly IRepositoryBase<ArticleReaction> articleReactionRepository;
        private readonly IRepositoryBase<HiddenArticleKeyword> hiddenArticleKeywordRepository;
        private readonly IRepositoryBase<ArticleReadHistory> articleReadHistoryRepository;
        private readonly INotificationPreferenceService notificationPreferenceService;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();

        public ArticleService(IArticleRepository articleRepository,
            IMapper mapper,
            IRepositoryBase<SavedArticle> savedArticleRepository,
            IRepositoryBase<ArticleReaction> articleReactionRepository,
            IRepositoryBase<HiddenArticleKeyword> hiddenArticleKeywordRepository,
            IRepositoryBase<ArticleReadHistory> articleReadHistoryRepository,
            INotificationPreferenceService notificationPreferenceService)
        {
            this.articleRepository = articleRepository;
            this.mapper = mapper;
            this.savedArticleRepository = savedArticleRepository;
            this.articleReactionRepository = articleReactionRepository;
            this.hiddenArticleKeywordRepository = hiddenArticleKeywordRepository;
            this.articleReadHistoryRepository = articleReadHistoryRepository;
            this.notificationPreferenceService = notificationPreferenceService;
        }

        public async Task<List<ArticleDTO>> GetAllArticles(NewsArticleRequestDTO newsArticleRequestDTO, int userId)
        {
            Expression<Func<Article, bool>> expression = article => true;

            if (!string.IsNullOrEmpty(newsArticleRequestDTO.SearchText))
            {
                expression = article =>
                    (!string.IsNullOrEmpty(article.Title) && article.Title.ToLower().Contains(newsArticleRequestDTO.SearchText.ToLower())) ||
                    (!string.IsNullOrEmpty(article.Content) && article.Content.Contains(newsArticleRequestDTO.SearchText.ToLower())) ||
                    (!string.IsNullOrEmpty(article.Description) && article.Description.Contains(newsArticleRequestDTO.SearchText.ToLower()));
            }
            else if (newsArticleRequestDTO.IsRequestedForToday)
            {
                expression = article =>
                    article.PublishedAt.HasValue ? article.PublishedAt.Value.Date == dateTimeHelper.GetCurrentSystemDateTime.Date : false;
            }
            else if (newsArticleRequestDTO.FromDate.HasValue && newsArticleRequestDTO.ToDate.HasValue)
            {
                expression = article =>
                    article.PublishedAt.HasValue ? article.PublishedAt.Value >= newsArticleRequestDTO.FromDate.Value &&
                    article.PublishedAt.Value <= newsArticleRequestDTO.ToDate.Value : false;
            }

            List<Article> articles = await articleRepository.GetWhere(expression)
                        .Where(article => !article.NewsCategory.IsHidden &&
                            !article.IsHidden)
                        .Include(article => article.NewsCategory)
                        .Include(article => article.ArticleReactions)
                        .Include(article => article.SavedArticles)
                        .Include(article => article.ArticleReactions)
                        .Include(article => article.ReportedArticles)
                        .ToListAsync();

            articles = SortArticlesByReaction(articles, newsArticleRequestDTO.SortBy);
            articles = await RecommendArticlesByCategory(articles, userId, newsArticleRequestDTO.CategoryId);
            articles = await ExcludeArticlesByKeyword(articles);

            if (!string.IsNullOrEmpty(newsArticleRequestDTO.SearchText) ||
                    (newsArticleRequestDTO.CategoryId != null && newsArticleRequestDTO.CategoryId != (int)CategoryType.All))
            {
                return mapper.Map<List<ArticleDTO>>(articles);
            }
            articles = await GetRecommendedArticles(userId, articles.Where(article => !article.IsHidden).ToList());

            return mapper.Map<List<ArticleDTO>>(articles);
        }

        public async Task<ArticleDTO> GetArticleById(int Id, int userId)
        {
            var article = await articleRepository.GetWhere(article => article.Id == Id)
                            .Include(article => article.NewsCategory)
                            .ProjectTo<ArticleDTO>(mapper.ConfigurationProvider)
                            .FirstOrDefaultAsync();

            if (article != null)
            {
                var articleReadHistory = new ArticleReadHistory
                {
                    ArticleId = Id,
                    UserId = userId,
                    CreatedById = userId,
                    CreatedDate = dateTimeHelper.CurrentUtcDateTime
                };

                await articleReadHistoryRepository.AddAsync(articleReadHistory);
            }
            return article;
        }

        public async Task<int> HideArticle(int articleId)
        {
            var article = await articleRepository.GetWhere(article => article.Id == articleId).FirstAsync();
            article.IsHidden = true;
            return await articleRepository.UpdateAsync(article);
        }

        public async Task<int> DeleteSavedArticles(int articleId, int userId)
        {
            if (!await articleRepository.GetWhere(article => article.Id == articleId).AnyAsync())
            {
                throw new NotFoundException(string.Format(ApplicationConstants.ArticleNotFoundWithThisId, articleId));
            }
            var savedArticle = await savedArticleRepository.GetWhere(savedArticle => savedArticle.UserId == userId && savedArticle.ArticleId == articleId)
                                        .FirstOrDefaultAsync();
            if (savedArticle != null)
            {
                return await savedArticleRepository.DeleteAsync(savedArticle);
            }
            return 0;
        }

        public async Task<List<ArticleDTO>> GetAllSavedArticles(int userId)
        {
            var savedArticle = await savedArticleRepository.GetWhere(savedArticle => savedArticle.UserId == userId &&
                !savedArticle.Article.NewsCategory.IsHidden &&
                !savedArticle.Article.IsHidden)
                        .Include(savedArticle => savedArticle.Article)
                            .ThenInclude(article => article.NewsCategory)
                        .Include(savedArticle => savedArticle.Article)
                            .ThenInclude(article => article.ArticleReactions)
                        .Select(savedArticle => savedArticle.Article)
                        .ToListAsync();
            if (savedArticle.Any())
            {
                return mapper.Map<List<ArticleDTO>>(savedArticle);
            }
            else
            {
                return new List<ArticleDTO>();
            }
        }

        public async Task<int> ReactArticle(int articleId, int userId, int reactionId)
        {
            if (!await articleRepository.GetWhere(article => article.Id == articleId).AnyAsync())
            {
                throw new NotFoundException(string.Format(ApplicationConstants.ArticleNotFoundWithThisId, articleId));
            }
            var existingArticleReaction = await articleReactionRepository.GetWhere(article => article.UserId == userId && article.ArticleId == articleId).FirstOrDefaultAsync();
            if (existingArticleReaction != null)
            {
                if (existingArticleReaction.ReactionId == reactionId)
                {
                    return 0;
                }
                existingArticleReaction.ReactionId = reactionId;
                return await articleReactionRepository.UpdateAsync(existingArticleReaction);
            }

            var articleReaction = new ArticleReaction()
            {
                ArticleId = articleId,
                UserId = userId,
                ReactionId = reactionId,
                CreatedById = userId,
                CreatedDate = dateTimeHelper.CurrentUtcDateTime
            };

            return await articleReactionRepository.AddAsync(articleReaction);
        }

        public async Task<int> ToggleVisibility(int articleId, int userId, bool IsHidden)
        {
            if (!await articleRepository.GetWhere(article => article.Id == articleId).AnyAsync())
            {
                throw new NotFoundException(string.Format(ApplicationConstants.ArticleNotFoundWithThisId, articleId));
            }
            var existingArticle = await articleRepository.GetWhere(article => article.Id == articleId).FirstOrDefaultAsync();
            if (existingArticle != null && existingArticle.IsHidden != IsHidden)
            {
                existingArticle.IsHidden = IsHidden;
                existingArticle.ModifiedById = userId;
                existingArticle.ModifiedDate = dateTimeHelper.CurrentUtcDateTime;
                return await articleRepository.UpdateAsync(existingArticle);
            }
            return 0;
        }

        public async Task<int> SaveArticle(int articleId, int userId)
        {
            if (!await IsNewsArticleExist(articleId))
            {
                throw new NotFoundException(string.Format(ApplicationConstants.ArticleNotFoundWithThisId, articleId));
            }
            var existingSavedArticle = await savedArticleRepository.GetWhere(article => article.UserId == userId && article.ArticleId == articleId).FirstOrDefaultAsync();
            if (existingSavedArticle == null)
            {
                var articleToSave = new SavedArticle()
                {
                    ArticleId = articleId,
                    UserId = userId,
                    CreatedById = userId,
                    CreatedDate = dateTimeHelper.CurrentUtcDateTime
                };
                return await savedArticleRepository.AddAsync(articleToSave);
            }
            return 0;
        }

        public async Task<bool> IsNewsArticleExist(int articleId)
        {
            return await articleRepository.GetWhere(article => article.Id == articleId).AnyAsync();
        }

        public async Task<List<Article>> GetRecommendedArticles(int userId, List<Article> articles)
        {
            var recommendedArticles = new List<Article>();
            var recommendedCategories = new List<CategoryRecommendationDTO>();

            await AddIfNotNull(articleRepository.GetMostLikedCategory(userId), recommendedCategories);
            await AddIfNotNull(articleRepository.GetMostSavedCategory(userId), recommendedCategories);
            await AddIfNotNull(articleRepository.GetMostReadCategory(userId), recommendedCategories);

            var notificationPreferences = await notificationPreferenceService.GetNotificationPreferences(new List<int> { userId });

            await AddRecommendationIfNotPresent(notificationPreferences, recommendedCategories);

            recommendedCategories = recommendedCategories.OrderByDescending(category => category.Count).ToList();

            foreach (var category in recommendedCategories)
            {
                recommendedArticles.AddRange(await GetArticlesByCategory(category.CategoryId, notificationPreferences, articles));
            }
            return recommendedArticles;
        }

        public async Task AddIfNotNull(Task<CategoryRecommendationDTO> categoryRecommendation, List<CategoryRecommendationDTO> categoryRecommendations)
        {
            var result = await categoryRecommendation;
            if (result != null)
                await AddRecommendationIfNotPresent(result, categoryRecommendations);
        }

        private async Task AddRecommendationIfNotPresent(CategoryRecommendationDTO categoryRecommendation, List<CategoryRecommendationDTO> categoryRecommendations)
        {
            if (!categoryRecommendations.Any(recommendation => recommendation.CategoryId == categoryRecommendation.CategoryId))
            {
                categoryRecommendations.Add(categoryRecommendation);
            }
        }

        private async Task AddRecommendationIfNotPresent(List<NotificationPreferenceDTO> notificationPreference, List<CategoryRecommendationDTO> recommendedCategories)
        {
            foreach (var category in notificationPreference.First().NewsCategories)
            {
                var existingRecommendedCategory = recommendedCategories.Where(recommendedCategory => recommendedCategory.CategoryId == category.CategoryId).FirstOrDefault();
                if (existingRecommendedCategory != null)
                {
                    existingRecommendedCategory.Keywords = category.Keywords.Select(keyword => keyword.Name).ToList();
                }
                else
                {
                    recommendedCategories.Add(new CategoryRecommendationDTO
                    {
                        CategoryId = category.CategoryId,
                        Count = 1,
                        Keywords = category.Keywords.Select(keyword => keyword.Name).ToList()
                    });
                }
            }
        }

        private async Task<List<Article>> RecommendArticlesByCategory(List<Article> articles, int userId, int? categoryId)
        {
            if (categoryId != null && categoryId != (int)CategoryType.All)
            {
                articles = articles.Where(article => article.NewsCategoryId == categoryId).ToList();
                var savedArticles = articles
                    .Where(article => article.SavedArticles.Any(sa => sa.UserId == userId))
                    .ToList();

                var readArticles = articles
                    .Where(article => article.ArticleReadHistory.Any(ra => ra.UserId == userId))
                    .ToList();

                var likedArticles = articles
                    .Where(article => article.ArticleReactions.Any(ar => ar.UserId == userId && ar.ReactionId == (int)ReactionType.Like))
                    .ToList();

                savedArticles.AddRange(readArticles);
                savedArticles.AddRange(likedArticles);

                var notificationPreferences = await notificationPreferenceService.GetNotificationPreferences(new List<int> { userId });
                var category = notificationPreferences.First()?.NewsCategories.Where(category => category.CategoryId == categoryId).FirstOrDefault();

                if (category != null && category.Keywords.Any())
                {
                    var keywords = category.Keywords.Where(keyword => keyword.IsEnabled)
                        .Select(keyword => keyword?.Name?.ToLower()).ToList();

                    articles = savedArticles
                        .OrderByDescending(article =>
                            keywords.Count(keyword =>
                                (!string.IsNullOrEmpty(article.Title) && article.Title.ToLower().Contains(keyword)) ||
                                (!string.IsNullOrEmpty(article.Description) && article.Description.ToLower().Contains(keyword)) ||
                                (!string.IsNullOrEmpty(article.Content) && article.Content.ToLower().Contains(keyword))
                            )
                        ).ToList();
                }

                var excludedArticles = articles.Where(article => !savedArticles.Select(x => x.Id).Contains(article.Id)).ToList();
                if (excludedArticles.Any())
                {
                    articles = savedArticles;
                }
                articles.AddRange(excludedArticles);
            }
            return articles;
        }

        private List<Article> SortArticlesByReaction(List<Article> articles, string SortBy)
        {
            if (!string.IsNullOrEmpty(SortBy))
            {
                switch (SortBy.ToLower())
                {
                    case nameof(ReactionType.Like):
                        articles = articles.OrderByDescending(a => a.ArticleReactions.Count(r => r.ReactionId == (int)ReactionType.Like)).ToList();
                        break;
                    case nameof(ReactionType.Dislike):
                        articles = articles.OrderByDescending(a => a.ArticleReactions.Count(r => r.ReactionId == (int)ReactionType.Dislike)).ToList();
                        break;
                }
            }
            return articles;
        }

        private async Task<List<Article>> ExcludeArticlesByKeyword(List<Article> articles)
        {
            var keywordsToHideArticles = await hiddenArticleKeywordRepository.GetAll().ToListAsync();
            if (keywordsToHideArticles.Any())
            {
                var keywordList = keywordsToHideArticles.Select(k => k.Name).ToList();

                articles = articles.Where(article =>
                    !keywordList.Any(keyword =>
                        (!string.IsNullOrEmpty(article.Title) && article.Title.ToLower().Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(article.Content) && article.Content.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(article.Description) && article.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    )
                ).ToList();
            }
            return articles;
        }

        private async Task<List<Article>> GetArticlesByCategory(int categoryId, List<NotificationPreferenceDTO> notificationPreferences, List<Article> articles)
        {
            var filteredArticles = articles
                .Where(a => !a.IsHidden && a.NewsCategoryId == categoryId)
                .ToList();

            var preference = notificationPreferences.First()?.NewsCategories.Where(category => category.IsEnabled)
                .FirstOrDefault(preference => preference.CategoryId == categoryId);

            if (preference != null)
            {
                var keywords = preference.Keywords.Where(keyword => keyword.IsEnabled)
                    .Select(keyword => keyword.Name.ToLower()).ToList();

                filteredArticles = filteredArticles
                    .OrderByDescending(article =>
                        keywords.Count(keyword =>
                            (!string.IsNullOrEmpty(article.Title) && article.Title.ToLower().Contains(keyword)) ||
                            (!string.IsNullOrEmpty(article.Description) && article.Description.ToLower().Contains(keyword)) ||
                            (!string.IsNullOrEmpty(article.Content) && article.Content.ToLower().Contains(keyword))
                        )
                    ).ToList();

                notificationPreferences.First().NewsCategories.Remove(preference);
            }

            return filteredArticles;
        }
    }
}
