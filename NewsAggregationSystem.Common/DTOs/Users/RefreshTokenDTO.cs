namespace NewsAggregationSystem.Common.DTOs.Users
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime Expires { get; set; }
    }
}
