using Moq;
using Moq.Protected;
using NewsAggregationSystem.Client.Services;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsSources;
using NUnit.Framework;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Tests
{
    [TestFixture]
    public class NewsSourcesServiceTests
    {
        private Mock<HttpMessageHandler> mockHttpMessageHandler;
        private HttpClient httpClient;
        private NewsSourcesService newsSourcesService;
        private JsonSerializerOptions jsonOptions;

        [SetUp]
        public void Setup()
        {
            this.mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            this.httpClient = new HttpClient(mockHttpMessageHandler.Object);
            this.httpClient.BaseAddress = new Uri(ApplicationConstants.BaseUrl);
            this.newsSourcesService = new NewsSourcesService(httpClient);
            this.jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Test]
        public async Task GetAllNewsSources_ValidResponse_ReturnsNewsSources()
        {
            var expectedNewsSources = new List<NewsSourceDTO>
            {
                new NewsSourceDTO { Id = 1, Name = "Test Source 1", BaseUrl = "https://test1.com"},
                new NewsSourceDTO { Id = 2, Name = "Test Source 2", BaseUrl = "https://test2.com"}
            };

            var responseContent = JsonSerializer.Serialize(expectedNewsSources, jsonOptions);

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

            var result = await newsSourcesService.GetAllNewsSourcesAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo(expectedNewsSources[0].Name));
            Assert.That(result[1].Name, Is.EqualTo(expectedNewsSources[1].Name));

            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.AddNewsSource)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task GetAllNewsSources_ServerError_ReturnsEmptyList()
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

            var result = await newsSourcesService.GetAllNewsSourcesAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task CreateNewsSourceAsync_ValidRequest_Success()
        {
            var createRequest = new CreateNewsSourceDTO
            {
                Name = "New Test Source",
                BaseUrl = "https://newtest.com",
                ApiKey = "test-api-key"
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("News source created successfully")
                });

            Assert.DoesNotThrowAsync(async () => await newsSourcesService.CreateNewsSourceAsync(createRequest));

            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.AddNewsSource) &&
                    req.Content.Headers.ContentType.MediaType == ApplicationConstants.JsonContentType),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task CreateNewsSourceAsync_ServerError_HandlesError()
        {
            var createRequest = new CreateNewsSourceDTO
            {
                Name = "New Test Source",
                BaseUrl = "https://newtest.com",
                ApiKey = "test-api-key"
            };

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

            Assert.DoesNotThrowAsync(async () => await newsSourcesService.CreateNewsSourceAsync(createRequest));
        }

        [Test]
        public async Task UpdateNewsSource_ServerError_HandlesError()
        {
            var newsSourceId = 1;
            var updateRequest = new UpdateNewsSourceDTO
            {
                ApiKey = "updated-api-key",
                IsActive = false
            };

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

            Assert.DoesNotThrowAsync(async () => await newsSourcesService.UpdateNewsSourceAsync(newsSourceId, updateRequest));
        }

        [Test]
        public async Task CreateNewsSourceAsync_ValidatesRequestContent()
        {
            var createRequest = new CreateNewsSourceDTO
            {
                Name = "New Test Source",
                BaseUrl = "https://newtest.com",
                ApiKey = "test-api-key",
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("News source created successfully")
                });

            await newsSourcesService.CreateNewsSourceAsync(createRequest);

            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.AddNewsSource) &&
                    req.Content.Headers.ContentType.MediaType == ApplicationConstants.JsonContentType),
                ItExpr.IsAny<CancellationToken>());
        }

        [TearDown]
        public void TearDown()
        {
            httpClient?.Dispose();
        }
    }
}