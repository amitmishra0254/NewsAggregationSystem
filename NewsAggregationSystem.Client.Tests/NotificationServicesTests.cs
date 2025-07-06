using Moq;
using Moq.Protected;
using NewsAggregationSystem.Client.Services;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.Notifications;
using NUnit.Framework;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Tests
{
    [TestFixture]
    public class NotificationServicesTests
    {
        private Mock<HttpMessageHandler> mockHttpMessageHandler;
        private HttpClient httpClient;
        private NotificationServices notificationServices;
        private JsonSerializerOptions jsonOptions;

        [SetUp]
        public void Setup()
        {
            this.mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            this.httpClient = new HttpClient(mockHttpMessageHandler.Object);
            this.httpClient.BaseAddress = new Uri(ApplicationConstants.BaseUrl);
            this.notificationServices = new NotificationServices(httpClient);
            this.jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Test]
        public async Task GetUserNotifications_ValidResponse_ReturnsNotifications()
        {
            var expectedNotifications = new List<NotificationDTO>
            {
                new NotificationDTO
                {
                    Id = 1,
                    Title = "New Article Alert",
                    Message = "New technology article available",
                },
                new NotificationDTO
                {
                    Id = 2,
                    Title = "Category Update",
                    Message = "Sports category has new articles",
                }
            };

            var responseContent = JsonSerializer.Serialize(expectedNotifications, jsonOptions);

            mockHttpMessageHandler
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

            var result = await notificationServices.GetUserNotificationsAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Title, Is.EqualTo(expectedNotifications[0].Title));
            Assert.That(result[1].Title, Is.EqualTo(expectedNotifications[1].Title));

            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.Notification)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task GetUserNotifications_ServerError_ReturnsEmptyList()
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

            var result = await notificationServices.GetUserNotificationsAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetUserNotifications_Unauthorized_ReturnsEmptyList()
        {
            mockHttpMessageHandler
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

            var result = await notificationServices.GetUserNotificationsAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetUserNotifications_NotFound_ReturnsEmptyList()
        {
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent("No notifications found")
                });

            var result = await notificationServices.GetUserNotificationsAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetUserNotifications_EmptyResponse_ReturnsEmptyList()
        {
            var emptyNotifications = new List<NotificationDTO>();

            var responseContent = JsonSerializer.Serialize(emptyNotifications, jsonOptions);

            mockHttpMessageHandler
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

            var result = await notificationServices.GetUserNotificationsAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [TearDown]
        public void TearDown()
        {
            httpClient?.Dispose();
        }
    }
}