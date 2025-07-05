using NewsAggregationSystem.Common.DTOs.NewsCategories;

namespace NewsAggregationSystem.Common.DTOs.NotificationPreferences
{
    public class NotificationPreferenceDTO
    {
        public int UserId { get; set; }
        public List<NewsCategoryDTO> NewsCategories { get; set; }
    }
}
