namespace NewsAggregationSystem.Common.DTOs.NewsArticles
{
    public class ArticleDTO
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string Description { get; set; }
        public string? Url { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? SourceName { get; set; }
        public string? Author { get; set; }
        public string Content { get; set; }
        public string? NewsCategoryName { get; set; }
        public int SavedCount { get; set; }
        public int LikedCount { get; set; }
        public int DislikedCount { get; set; }
        public string Link => $"https://localhost:7122/api/Articles/{Id}";
    }
}
