using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;
using NewsAggregationSystem.Service.Interfaces;

namespace NewsAggregationSystem.Service.Services
{
    public class HiddenArticleKeywordService : IHiddenArticleKeywordService
    {
        private readonly IRepositoryBase<HiddenArticleKeyword> hiddenArticleKeywordRepository;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();

        public HiddenArticleKeywordService(IRepositoryBase<HiddenArticleKeyword> hiddenArticleKeywordRepository)
        {
            this.hiddenArticleKeywordRepository = hiddenArticleKeywordRepository;
        }

        public async Task<int> Add(string keyword, int userId)
        {
            var hiddenArticleKeyword = new HiddenArticleKeyword
            {
                Name = keyword?.ToLower(),
                CreatedById = userId,
                CreatedDate = dateTimeHelper.CurrentUtcDateTime
            };
            return await hiddenArticleKeywordRepository.AddAsync(hiddenArticleKeyword);
        }
    }
}
