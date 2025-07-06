using Moq;
using Moq.Protected;
using NewsAggregationSystem.Client.Services;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NUnit.Framework;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Tests
{
    [TestFixture]
    public class ReportServiceTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private ReportService _reportService;
        private JsonSerializerOptions _jsonOptions;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _httpClient.BaseAddress = new Uri(ApplicationConstants.BaseUrl);
            _reportService = new ReportService(_httpClient);
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Test]
        public async Task ReportArticleAsync_ValidRequest_Success()
        {
            // Arrange
            var articleId = 1;
            var reason = "Inappropriate content";

            _mockHttpMessageHandler
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

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _reportService.ReportArticleAsync(articleId, reason));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith($"Articles/{articleId}/report") &&
                    req.Content.Headers.ContentType.MediaType == ApplicationConstants.JsonContentType),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task ReportArticleAsync_ServerError_HandlesError()
        {
            // Arrange
            var articleId = 1;
            var reason = "Inappropriate content";

            _mockHttpMessageHandler
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

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _reportService.ReportArticleAsync(articleId, reason));
        }

        [Test]
        public async Task ReportArticleAsync_NotFound_HandlesError()
        {
            // Arrange
            var articleId = 999;
            var reason = "Inappropriate content";

            _mockHttpMessageHandler
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

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _reportService.ReportArticleAsync(articleId, reason));
        }

        [Test]
        public async Task ReportArticleAsync_Conflict_HandlesError()
        {
            // Arrange
            var articleId = 1;
            var reason = "Inappropriate content";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Content = new StringContent("Article already reported")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _reportService.ReportArticleAsync(articleId, reason));
        }

        [Test]
        public async Task ReportArticleAsync_BadRequest_HandlesError()
        {
            // Arrange
            var articleId = 1;
            var reason = ""; // Empty reason

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Invalid request")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _reportService.ReportArticleAsync(articleId, reason));
        }

        [Test]
        public async Task ReportArticleAsync_Unauthorized_HandlesError()
        {
            // Arrange
            var articleId = 1;
            var reason = "Inappropriate content";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent("Unauthorized access")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _reportService.ReportArticleAsync(articleId, reason));
        }

        [Test]
        public async Task ReportArticleAsync_ValidatesRequestContent()
        {
            // Arrange
            var articleId = 1;
            var reason = "Inappropriate content";

            _mockHttpMessageHandler
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

            // Act
            await _reportService.ReportArticleAsync(articleId, reason);

            // Assert
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith($"Articles/{articleId}/report") &&
                    req.Content.Headers.ContentType.MediaType == ApplicationConstants.JsonContentType),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task ReportArticleAsync_HttpException_HandlesError()
        {
            // Arrange
            var articleId = 1;
            var reason = "Inappropriate content";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _reportService.ReportArticleAsync(articleId, reason));
        }

        [Test]
        public async Task ReportArticleAsync_Timeout_HandlesError()
        {
            // Arrange
            var articleId = 1;
            var reason = "Inappropriate content";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException("Request timeout"));

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _reportService.ReportArticleAsync(articleId, reason));
        }

        [Test]
        public async Task ReportArticleAsync_ServiceUnavailable_HandlesError()
        {
            // Arrange
            var articleId = 1;
            var reason = "Inappropriate content";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.ServiceUnavailable,
                    Content = new StringContent("Service temporarily unavailable")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _reportService.ReportArticleAsync(articleId, reason));
        }

        [Test]
        public async Task ReportArticleAsync_Forbidden_HandlesError()
        {
            // Arrange
            var articleId = 1;
            var reason = "Inappropriate content";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Content = new StringContent("Access forbidden")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _reportService.ReportArticleAsync(articleId, reason));
        }

        [Test]
        public async Task ReportArticleAsync_InvalidArticleId_HandlesError()
        {
            // Arrange
            var articleId = -1; // Invalid ID
            var reason = "Inappropriate content";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Invalid article ID")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _reportService.ReportArticleAsync(articleId, reason));
        }

        [Test]
        public async Task ReportArticleAsync_LongReason_HandlesError()
        {
            // Arrange
            var articleId = 1;
            var reason = new string('A', 1001); // Very long reason

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Reason too long")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _reportService.ReportArticleAsync(articleId, reason));
        }

        [Test]
        public async Task ReportArticleAsync_SpecialCharacters_HandlesSuccess()
        {
            // Arrange
            var articleId = 1;
            var reason = "Content with special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?";

            _mockHttpMessageHandler
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

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _reportService.ReportArticleAsync(articleId, reason));
        }
    }
} 