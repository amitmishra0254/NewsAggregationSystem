using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.NewsCategories;

namespace NewsAggregationSystem.API.Services.NewsCategories
{
    public class NewsCategoryService : INewsCategoryService
    {
        private readonly INewsCategoryRepository newsCategoryRepository;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();
        public NewsCategoryService(INewsCategoryRepository newsCategoryRepository)
        {
            this.newsCategoryRepository = newsCategoryRepository;
        }

        public async Task<int> AddNewsCategory(string name, int userId)
        {
            var newsCategory = new NewsCategory
            {
                Name = name,
                CreatedById = userId,
                CreatedDate = dateTimeHelper.CurrentUtcDateTime,
            };
            await newsCategoryRepository.AddAsync(newsCategory);
            return newsCategory.Id;
        }
    }
}
