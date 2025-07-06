using Moq;
using Moq.Protected;
using NewsAggregationSystem.Client.Services;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NotificationPreferences;
using NUnit.Framework;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Tests
{
    [TestFixture]
    public class NotificationPreferenceServiceTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private NotificationPreferenceService _notificationPreferenceService;
        private JsonSerializerOptions _jsonOptions;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _httpClient.BaseAddress = new Uri(ApplicationConstants.BaseUrl);
            _notificationPreferenceService = new NotificationPreferenceService(_httpClient);
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Test]
        public async Task GetUserNotificationPreferencesAsync_ValidResponse_ReturnsPreferences()
        {
            // Arrange
            var expectedPreferences = new List<NotificationPreferenceDTO>
            {
                new NotificationPreferenceDTO 
                { 
                    Id = 1, 
                    CategoryId = 1,
                    CategoryName = "Technology",
                    IsEnabled = true,
                    Keywords = new List<string> { "AI", "Machine Learning" }
                },
                new NotificationPreferenceDTO 
                { 
                    Id = 2, 
                    CategoryId = 2,
                    CategoryName = "Sports",
                    IsEnabled = false,
                    Keywords = new List<string> { "Football", "Basketball" }
                }
            };

            var responseContent = JsonSerializer.Serialize(expectedPreferences, _jsonOptions);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, ApplicationConstants.JsonContentType)
                });

            // Act
            var result = await _notificationPreferenceService.GetUserNotificationPreferencesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].CategoryName, Is.EqualTo(expectedPreferences[0].CategoryName));
            Assert.That(result[1].CategoryName, Is.EqualTo(expectedPreferences[1].CategoryName));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.NotificationPreferences)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task GetUserNotificationPreferencesAsync_ServerError_ReturnsEmptyList()
        {
            // Arrange
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

            // Act
            var result = await _notificationPreferenceService.GetUserNotificationPreferencesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task AddKeywordToCategoryAsync_ValidRequest_Success()
        {
            // Arrange
            var categoryId = 1;
            var keyword = "Artificial Intelligence";

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
            Assert.DoesNotThrowAsync(async () => await _notificationPreferenceService.AddKeywordToCategoryAsync(categoryId, keyword));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith($"{ApplicationConstants.NotificationPreferences}/{categoryId}/keywords") &&
                    req.Content.Headers.ContentType.MediaType == ApplicationConstants.JsonContentType),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task AddKeywordToCategoryAsync_ServerError_HandlesError()
        {
            // Arrange
            var categoryId = 1;
            var keyword = "Artificial Intelligence";

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
            Assert.DoesNotThrowAsync(async () => await _notificationPreferenceService.AddKeywordToCategoryAsync(categoryId, keyword));
        }

        [Test]
        public async Task AddKeywordToCategoryAsync_NotFound_HandlesError()
        {
            // Arrange
            var categoryId = 999;
            var keyword = "Artificial Intelligence";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent("Category not found")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _notificationPreferenceService.AddKeywordToCategoryAsync(categoryId, keyword));
        }

        [Test]
        public async Task AddKeywordToCategoryAsync_Conflict_HandlesError()
        {
            // Arrange
            var categoryId = 1;
            var keyword = "Existing Keyword";

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
            Assert.DoesNotThrowAsync(async () => await _notificationPreferenceService.AddKeywordToCategoryAsync(categoryId, keyword));
        }

        [Test]
        public async Task ToggleKeywordStatusAsync_ValidRequest_Success()
        {
            // Arrange
            var keywordId = 1;
            var isEnabled = true;

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Keyword status updated")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _notificationPreferenceService.ToggleKeywordStatusAsync(keywordId, isEnabled));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri.ToString().EndsWith($"{ApplicationConstants.NotificationPreferences}/keywords/{keywordId}/status")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task ToggleKeywordStatusAsync_NotFound_HandlesError()
        {
            // Arrange
            var keywordId = 999;
            var isEnabled = true;

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent("Keyword not found")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _notificationPreferenceService.ToggleKeywordStatusAsync(keywordId, isEnabled));
        }

        [Test]
        public async Task ToggleKeywordStatusAsync_ServerError_HandlesError()
        {
            // Arrange
            var keywordId = 1;
            var isEnabled = true;

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
            Assert.DoesNotThrowAsync(async () => await _notificationPreferenceService.ToggleKeywordStatusAsync(keywordId, isEnabled));
        }

        [Test]
        public async Task ToggleCategoryStatusAsync_ValidRequest_Success()
        {
            // Arrange
            var categoryId = 1;
            var isEnabled = true;

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Category status updated")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _notificationPreferenceService.ToggleCategoryStatusAsync(categoryId, isEnabled));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri.ToString().EndsWith($"{ApplicationConstants.NotificationPreferences}/{categoryId}/status")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task ToggleCategoryStatusAsync_NotFound_HandlesError()
        {
            // Arrange
            var categoryId = 999;
            var isEnabled = true;

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent("Category not found")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _notificationPreferenceService.ToggleCategoryStatusAsync(categoryId, isEnabled));
        }

        [Test]
        public async Task ToggleCategoryStatusAsync_ServerError_HandlesError()
        {
            // Arrange
            var categoryId = 1;
            var isEnabled = true;

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
            Assert.DoesNotThrowAsync(async () => await _notificationPreferenceService.ToggleCategoryStatusAsync(categoryId, isEnabled));
        }

        [Test]
        public async Task AddKeywordToCategoryAsync_ValidatesRequestContent()
        {
            // Arrange
            var categoryId = 1;
            var keyword = "Artificial Intelligence";

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
            await _notificationPreferenceService.AddKeywordToCategoryAsync(categoryId, keyword);

            // Assert
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith($"{ApplicationConstants.NotificationPreferences}/{categoryId}/keywords") &&
                    req.Content.Headers.ContentType.MediaType == ApplicationConstants.JsonContentType),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task AllMethods_HttpException_HandlesError()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act & Assert
            var preferences = await _notificationPreferenceService.GetUserNotificationPreferencesAsync();
            Assert.That(preferences, Is.Empty);

            Assert.DoesNotThrowAsync(async () => await _notificationPreferenceService.AddKeywordToCategoryAsync(1, "Test Keyword"));
            Assert.DoesNotThrowAsync(async () => await _notificationPreferenceService.ToggleKeywordStatusAsync(1, true));
            Assert.DoesNotThrowAsync(async () => await _notificationPreferenceService.ToggleCategoryStatusAsync(1, true));
        }

        [Test]
        public async Task AddKeywordToCategoryAsync_EmptyKeyword_HandlesError()
        {
            // Arrange
            var categoryId = 1;
            var keyword = "";

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
            Assert.DoesNotThrowAsync(async () => await _notificationPreferenceService.AddKeywordToCategoryAsync(categoryId, keyword));
        }

        [Test]
        public async Task GetUserNotificationPreferencesAsync_Unauthorized_ReturnsEmptyList()
        {
            // Arrange
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

            // Act
            var result = await _notificationPreferenceService.GetUserNotificationPreferencesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }
    }
} 