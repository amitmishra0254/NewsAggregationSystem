namespace NewsAggregationSystem.Common.DTOs.NewsCategories
{
    public class NewsCategoryDTO
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public List<NotificationPreferencesKeywordDTO> Keywords { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class NotificationPreferencesKeywordDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class CategoryRecommendationDTO
    {
        public int CategoryId { get; set; }
        public int Count { get; set; }
        public List<string> Keywords { get; set; }
    }

}
