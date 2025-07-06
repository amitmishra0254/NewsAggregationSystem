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
    public class NotificationPreferenceServiceTests
    {
        private Mock<HttpMessageHandler> mockHttpMessageHandler;
        private HttpClient httpClient;
        private NotificationPreferenceService notificationPreferenceService;
        private JsonSerializerOptions jsonOptions;

        [SetUp]
        public void Setup()
        {
            this.mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            this.httpClient = new HttpClient(mockHttpMessageHandler.Object);
            this.httpClient.BaseAddress = new Uri(ApplicationConstants.BaseUrl);
            this.notificationPreferenceService = new NotificationPreferenceService(httpClient);
            this.jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Test]
        public async Task GetUserNotificationPreferences_ServerError_ReturnsEmptyList()
        {
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

            var result = await notificationPreferenceService.GetUserNotificationPreferencesAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task AddKeywordToCategory_ServerError_HandlesError()
        {
            var categoryId = 1;
            var keyword = "Artificial Intelligence";

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

            Assert.DoesNotThrowAsync(async () => await notificationPreferenceService.AddKeywordToCategoryAsync(keyword, categoryId));
        }

        [Test]
        public async Task AddKeywordToCategory_NotFound_HandlesError()
        {
            var categoryId = 999;
            var keyword = "Artificial Intelligence";

            mockHttpMessageHandler
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

            Assert.DoesNotThrowAsync(async () => await notificationPreferenceService.AddKeywordToCategoryAsync(keyword, categoryId));
        }

        [Test]
        public async Task AddKeywordToCategory_Conflict_HandlesError()
        {
            var categoryId = 1;
            var keyword = "Existing Keyword";

            mockHttpMessageHandler
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

            Assert.DoesNotThrowAsync(async () => await notificationPreferenceService.AddKeywordToCategoryAsync(keyword, categoryId));
        }

        [Test]
        public async Task UpdateKeywordStatus_NotFound_HandlesError()
        {
            var keywordId = 999;
            var isEnabled = true;

            mockHttpMessageHandler
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

            Assert.DoesNotThrowAsync(async () => await notificationPreferenceService.UpdateKeywordStatusAsync(keywordId, isEnabled));
        }

        [Test]
        public async Task UpdateKeywordStatus_ServerError_HandlesError()
        {
            var keywordId = 1;
            var isEnabled = true;

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

            Assert.DoesNotThrowAsync(async () => await notificationPreferenceService.UpdateKeywordStatusAsync(keywordId, isEnabled));
        }

        [Test]
        public async Task UpdateCategoryStatus_NotFound_HandlesError()
        {
            var categoryId = 999;
            var isEnabled = true;

            mockHttpMessageHandler
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

            Assert.DoesNotThrowAsync(async () => await notificationPreferenceService.UpdateCategoryStatusAsync(categoryId, isEnabled));
        }

        [Test]
        public async Task UpdateCategoryStatus_ServerError_HandlesError()
        {
            var categoryId = 1;
            var isEnabled = true;

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

            Assert.DoesNotThrowAsync(async () => await notificationPreferenceService.UpdateCategoryStatusAsync(categoryId, isEnabled));
        }
    }
}