using Moq;
using Moq.Protected;
using NewsAggregationSystem.Client.Services;
using NewsAggregationSystem.Common.Constants;
using NUnit.Framework;
using System.Net;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Tests
{
    [TestFixture]
    public class ReportServiceTests
    {
        private Mock<HttpMessageHandler> mockHttpMessageHandler;
        private HttpClient httpClient;
        private ReportService reportService;
        private JsonSerializerOptions jsonOptions;

        [SetUp]
        public void Setup()
        {
            this.mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            this.httpClient = new HttpClient(mockHttpMessageHandler.Object);
            this.httpClient.BaseAddress = new Uri(ApplicationConstants.BaseUrl);
            this.reportService = new ReportService(httpClient);
            this.jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Test]
        public async Task CreateArticleReportAsync_ValidRequest_Success()
        {
            var articleId = 1;
            var reason = "Inappropriate content";

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Article reported successfully")
                });

            Assert.DoesNotThrowAsync(async () => await reportService.CreateArticleReportAsync(articleId, "Reason"));

            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith($"Reports") &&
                    req.Content.Headers.ContentType.MediaType == ApplicationConstants.JsonContentType),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task CreateArticleReportAsync_ServerError_HandlesError()
        {
            var articleId = 1;
            var reason = "Inappropriate content";

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Server error")
                });

            Assert.DoesNotThrowAsync(async () => await reportService.CreateArticleReportAsync(articleId, "Reason"));
        }

        [Test]
        public async Task CreateArticleReportAsync_NotFound_HandlesError()
        {
            var articleId = 999;
            var reason = "Inappropriate content";

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent("Article not found")
                });

            Assert.DoesNotThrowAsync(async () => await reportService.CreateArticleReportAsync(articleId, "Reason"));
        }

        [TearDown]
        public void TearDown()
        {
            httpClient?.Dispose();
        }

    }
}