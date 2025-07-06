using Moq;
using Moq.Protected;
using NewsAggregationSystem.Client.Services;
using NewsAggregationSystem.Common.Constants;
using NUnit.Framework;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Tests
{
    [TestFixture]
    public class AdminServiceTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private AdminService _adminService;
        private JsonSerializerOptions _jsonOptions;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _httpClient.BaseAddress = new Uri(ApplicationConstants.BaseUrl);
            _adminService = new AdminService(_httpClient);
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Test]
        public async Task AddKeywordToHideArticlesAsync_ValidKeyword_Success()
        {
            // Arrange
            var keyword = "spam";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Keyword added successfully")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _adminService.AddKeywordToHideArticlesAsync(keyword));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith("Admin/hidden-keywords") &&
                    req.Content.Headers.ContentType.MediaType == ApplicationConstants.JsonContentType),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task AddKeywordToHideArticlesAsync_ServerError_HandlesError()
        {
            // Arrange
            var keyword = "spam";

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
            Assert.DoesNotThrowAsync(async () => await _adminService.AddKeywordToHideArticlesAsync(keyword));
        }

        [Test]
        public async Task AddKeywordToHideArticlesAsync_Conflict_HandlesError()
        {
            // Arrange
            var keyword = "existing-keyword";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Content = new StringContent("Keyword already exists")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _adminService.AddKeywordToHideArticlesAsync(keyword));
        }

        [Test]
        public async Task AddKeywordToHideArticlesAsync_BadRequest_HandlesError()
        {
            // Arrange
            var keyword = ""; // Empty keyword

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Invalid keyword")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _adminService.AddKeywordToHideArticlesAsync(keyword));
        }

        [Test]
        public async Task AddKeywordToHideArticlesAsync_Unauthorized_HandlesError()
        {
            // Arrange
            var keyword = "spam";

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
            Assert.DoesNotThrowAsync(async () => await _adminService.AddKeywordToHideArticlesAsync(keyword));
        }

        [Test]
        public async Task AddKeywordToHideArticlesAsync_Forbidden_HandlesError()
        {
            // Arrange
            var keyword = "spam";

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
            Assert.DoesNotThrowAsync(async () => await _adminService.AddKeywordToHideArticlesAsync(keyword));
        }

        [Test]
        public async Task AddKeywordToHideArticlesAsync_ValidatesRequestContent()
        {
            // Arrange
            var keyword = "spam";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Keyword added successfully")
                });

            // Act
            await _adminService.AddKeywordToHideArticlesAsync(keyword);

            // Assert
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith("Admin/hidden-keywords") &&
                    req.Content.Headers.ContentType.MediaType == ApplicationConstants.JsonContentType),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task AddKeywordToHideArticlesAsync_HttpException_HandlesError()
        {
            // Arrange
            var keyword = "spam";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _adminService.AddKeywordToHideArticlesAsync(keyword));
        }

        [Test]
        public async Task AddKeywordToHideArticlesAsync_Timeout_HandlesError()
        {
            // Arrange
            var keyword = "spam";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException("Request timeout"));

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _adminService.AddKeywordToHideArticlesAsync(keyword));
        }

        [Test]
        public async Task AddKeywordToHideArticlesAsync_ServiceUnavailable_HandlesError()
        {
            // Arrange
            var keyword = "spam";

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
            Assert.DoesNotThrowAsync(async () => await _adminService.AddKeywordToHideArticlesAsync(keyword));
        }

        [Test]
        public async Task AddKeywordToHideArticlesAsync_LongKeyword_HandlesError()
        {
            // Arrange
            var keyword = new string('A', 101); // Very long keyword

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Keyword too long")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _adminService.AddKeywordToHideArticlesAsync(keyword));
        }

        [Test]
        public async Task AddKeywordToHideArticlesAsync_SpecialCharacters_HandlesSuccess()
        {
            // Arrange
            var keyword = "spam@#$%^&*()_+-=[]{}|;':\",./<>?";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Keyword added successfully")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _adminService.AddKeywordToHideArticlesAsync(keyword));
        }

        [Test]
        public async Task AddKeywordToHideArticlesAsync_WhitespaceOnly_HandlesError()
        {
            // Arrange
            var keyword = "   "; // Whitespace only

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Keyword cannot be empty")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _adminService.AddKeywordToHideArticlesAsync(keyword));
        }

        [Test]
        public async Task AddKeywordToHideArticlesAsync_NullKeyword_HandlesError()
        {
            // Arrange
            string keyword = null;

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Keyword cannot be null")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _adminService.AddKeywordToHideArticlesAsync(keyword));
        }

        [Test]
        public async Task AddKeywordToHideArticlesAsync_CaseSensitive_HandlesSuccess()
        {
            // Arrange
            var keyword = "SPAM"; // Uppercase

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Keyword added successfully")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _adminService.AddKeywordToHideArticlesAsync(keyword));
        }
    }
} 