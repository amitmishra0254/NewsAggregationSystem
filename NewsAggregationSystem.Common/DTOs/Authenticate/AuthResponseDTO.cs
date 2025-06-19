namespace NewsAggregationSystem.Common.DTOs.Authenticate
{
    public class AuthResponseDTO
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public string ExpiresIn { get; set; }
        public string UserName { get; set; }
        public string Roles { get; set; }
        public string RedirectTo { get; set; }
        public string IssuedAt { get; set; }
    }
}