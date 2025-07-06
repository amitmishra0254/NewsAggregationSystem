using Microsoft.EntityFrameworkCore;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsCategories;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.NewsCategories;
using NewsAggregationSystem.Service.Interfaces;
using System.Linq.Expressions;

namespace NewsAggregationSystem.Service.Services
{
    public class NewsCategoryService : INewsCategoryService
    {
        private readonly INewsCategoryRepository newsCategoryRepository;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();

        public NewsCategoryService(INewsCategoryRepository newsCategoryRepository)
        {
            this.newsCategoryRepository = newsCategoryRepository;
        }

        public async Task<int> CreateNewsCategoryAsync(string name, int userId)
        {
            if (await newsCategoryRepository.GetWhere(newsCategory => newsCategory.Name.ToLower() == name.ToLower()).AnyAsync())
            {
                throw new AlreadyExistException(string.Format(ApplicationConstants.CategoryAlreadyExist, name));
            }
            var newsCategory = new NewsCategory
            {
                Name = name,
                CreatedById = userId,
                CreatedDate = dateTimeHelper.CurrentUtcDateTime,
            };
            await newsCategoryRepository.AddAsync(newsCategory);
            return newsCategory.Id;
        }

        public async Task<int> ToggleCategoryVisibilityAsync(int categoryId, bool IsHidden)
        {
            var existingCategory = await newsCategoryRepository.GetWhere(newsCategory => newsCategory.Id == categoryId)
                .FirstOrDefaultAsync();
            if (existingCategory != null)
            {
                if (existingCategory.IsHidden == IsHidden)
                {
                    return 0;
                }
                existingCategory.IsHidden = IsHidden;
                return await newsCategoryRepository.UpdateAsync(existingCategory);
            }
            else
            {
                throw new NotFoundException(string.Format(ApplicationConstants.CategoryNotFoundWithThisId, categoryId));
            }
        }

        public async Task<List<NotificationPreferencesKeywordDTO>> GetAllNewsCategoriesAsync(string userRole)
        {
            Expression<Func<NewsCategory, bool>> expression = expression => true;

            if (string.Equals(userRole, UserRoles.User.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                expression = expression => !expression.IsHidden;
            }

            return await newsCategoryRepository.GetWhere(expression).Select(category => new NotificationPreferencesKeywordDTO
            {
                Id = category.Id,
                Name = category.Name,
                IsEnabled = category.IsHidden
            }).ToListAsync();
        }
    }
}
