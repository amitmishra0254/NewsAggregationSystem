namespace NewsAggregationSystem.Common.DTOs.NewsArticles
{
    public class NewsArticleRequestDTO
    {
        public string? SearchText { get; set; } = null;
        public bool IsRequestedForToday { get; set; } = false;
        public DateTime? FromDate { get; set; } = null;
        public DateTime? ToDate { get; set; } = null;
        public int? CategoryId { get; set; } = null;
        public string? SortBy { get; set; } = null;
    }
}