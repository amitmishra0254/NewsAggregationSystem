namespace NewsAggregationSystem.Common.DTOs.NewsArticles
{
    public class NewsArticleRequestDTO
    {
        public string? SearchText { get; set; }
        public bool IsRequestedForToday { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? CategoryId { get; set; }
    }
}