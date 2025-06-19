using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Articles;
using NewsAggregationSystem.DAL.Repositories.Generic;
using NewsAggregationSystem.DAL.Repositories.Users;
using System.Linq.Expressions;

namespace NewsAggregationSystem.API.Services.Articles
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository articleRepository;
        private readonly IMapper mapper;
        private readonly IUserRepository userRepository;
        private readonly IRepositoryBase<SavedArticle> savedArticleRepository;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();
        public ArticleService(IArticleRepository articleRepository, IMapper mapper, IUserRepository userRepository, IRepositoryBase<SavedArticle> savedArticleRepository)
        {
            this.articleRepository = articleRepository;
            this.mapper = mapper;
            this.userRepository = userRepository;
            this.savedArticleRepository = savedArticleRepository;
        }

        public async Task<List<ArticleDTO>> GetAllArticles(NewsArticleRequestDTO newsArticleRequestDTO)
        {
            Expression<Func<Article, bool>> expression = null;

            if (!string.IsNullOrEmpty(newsArticleRequestDTO.SearchText))
            {
                expression = article =>
                    article.Title.Contains(newsArticleRequestDTO.SearchText) ||
                    article.Description.Contains(newsArticleRequestDTO.SearchText) ||
                    article.Content.Contains(newsArticleRequestDTO.SearchText);
            }
            else if (newsArticleRequestDTO.IsRequestedForToday)
            {
                expression = article =>
                    article.PublishedAt.HasValue ? article.PublishedAt.Value.Date == dateTimeHelper.GetCurrentSystemDateTime.Date : false;
            }
            else
            {
                expression = article =>
                    article.PublishedAt.HasValue ? article.PublishedAt.Value >= newsArticleRequestDTO.FromDate.Value &&
                    article.PublishedAt.Value <= newsArticleRequestDTO.ToDate.Value : false &&
                    newsArticleRequestDTO.CategoryId == (int)CategoryType.All ? true : article.NewsCategoryId == newsArticleRequestDTO.CategoryId;
            }

            return await articleRepository.GetWhere(expression)
                .Include(a => a.NewsCategory)
                .ProjectTo<ArticleDTO>(mapper.ConfigurationProvider)
            .ToListAsync();
        }

        public async Task<ArticleDTO> GetArticleById(int Id)
        {
            return await articleRepository.GetWhere(a => a.Id == Id)
            .Include(a => a.NewsCategory)
                .ProjectTo<ArticleDTO>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }


        public async Task<int> DeleteSavedArticles(int articleId, int userId)
        {
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
            var savedArticle = await savedArticleRepository.GetWhere(savedArticle => savedArticle.UserId == userId)
                        .Include(savedArticle => savedArticle.Article)
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


        public async Task<int> SaveArticle(int articleId, int userId)
        {
            var articleToSave = new SavedArticle()
            {
                ArticleId = articleId,
                UserId = userId
            };

            return await savedArticleRepository.AddAsync(articleToSave);
        }
    }
}
