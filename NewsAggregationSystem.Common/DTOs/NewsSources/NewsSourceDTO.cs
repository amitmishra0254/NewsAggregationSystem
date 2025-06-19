namespace NewsAggregationSystem.Common.DTOs.NewsSources
{
    public class NewsSourceDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastAccess { get; set; }
    }
}
