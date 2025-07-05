using System.Net;

namespace NewsAggregationSystem.Common.DTOs
{
    public class ParsedErrorResponseDTO
    {
        public HttpStatusCode StatusCode { get; set; }
        public string? RawContent { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
