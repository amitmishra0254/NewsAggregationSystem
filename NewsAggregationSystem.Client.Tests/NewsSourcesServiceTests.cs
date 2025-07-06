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
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private NewsSourcesService _newsSourcesService;
        private JsonSerializerOptions _jsonOptions;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _httpClient.BaseAddress = new Uri(ApplicationConstants.BaseUrl);
            _newsSourcesService = new NewsSourcesService(_httpClient);
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Test]
        public async Task GetAllNewsSourcesAsync_ValidResponse_ReturnsNewsSources()
        {
            // Arrange
            var expectedNewsSources = new List<NewsSourceDTO>
            {
                new NewsSourceDTO { Id = 1, Name = "Test Source 1", Url = "https://test1.com", IsEnabled = true },
                new NewsSourceDTO { Id = 2, Name = "Test Source 2", Url = "https://test2.com", IsEnabled = false }
            };

            var responseContent = JsonSerializer.Serialize(expectedNewsSources, _jsonOptions);

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
            var result = await _newsSourcesService.GetAllNewsSourcesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo(expectedNewsSources[0].Name));
            Assert.That(result[1].Name, Is.EqualTo(expectedNewsSources[1].Name));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.AddNewsSource)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task GetAllNewsSourcesAsync_ServerError_ReturnsEmptyList()
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
            var result = await _newsSourcesService.GetAllNewsSourcesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetNewsSourceByIdAsync_ValidId_ReturnsNewsSource()
        {
            // Arrange
            var newsSourceId = 1;
            var expectedNewsSource = new NewsSourceDTO
            {
                Id = newsSourceId,
                Name = "Test Source",
                Url = "https://test.com",
                IsEnabled = true,
                LastAccess = DateTime.Now
            };

            var responseContent = JsonSerializer.Serialize(expectedNewsSource, _jsonOptions);

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
            var result = await _newsSourcesService.GetNewsSourceByIdAsync(newsSourceId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(expectedNewsSource.Id));
            Assert.That(result.Name, Is.EqualTo(expectedNewsSource.Name));
            Assert.That(result.Url, Is.EqualTo(expectedNewsSource.Url));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().EndsWith($"{ApplicationConstants.AddNewsSource}/{newsSourceId}")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task GetNewsSourceByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            var newsSourceId = 999;

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent("News source not found")
                });

            // Act
            var result = await _newsSourcesService.GetNewsSourceByIdAsync(newsSourceId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CreateNewsSourceAsync_ValidRequest_Success()
        {
            // Arrange
            var createRequest = new CreateNewsSourceDTO
            {
                Name = "New Test Source",
                Url = "https://newtest.com",
                ApiKey = "test-api-key",
                IsEnabled = true
            };

            _mockHttpMessageHandler
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

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _newsSourcesService.CreateNewsSourceAsync(createRequest));

            _mockHttpMessageHandler.Protected().Verify(
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
            // Arrange
            var createRequest = new CreateNewsSourceDTO
            {
                Name = "New Test Source",
                Url = "https://newtest.com",
                ApiKey = "test-api-key",
                IsEnabled = true
            };

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
            Assert.DoesNotThrowAsync(async () => await _newsSourcesService.CreateNewsSourceAsync(createRequest));
        }

        [Test]
        public async Task UpdateNewsSourceAsync_ValidRequest_Success()
        {
            // Arrange
            var newsSourceId = 1;
            var updateRequest = new UpdateNewsSourceDTO
            {
                Name = "Updated Test Source",
                Url = "https://updatedtest.com",
                ApiKey = "updated-api-key",
                IsEnabled = false
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("News source updated successfully")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _newsSourcesService.UpdateNewsSourceAsync(newsSourceId, updateRequest));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri.ToString().EndsWith($"{ApplicationConstants.AddNewsSource}/{newsSourceId}") &&
                    req.Content.Headers.ContentType.MediaType == ApplicationConstants.JsonContentType),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task UpdateNewsSourceAsync_NotFound_HandlesError()
        {
            // Arrange
            var newsSourceId = 999;
            var updateRequest = new UpdateNewsSourceDTO
            {
                Name = "Updated Test Source",
                Url = "https://updatedtest.com",
                ApiKey = "updated-api-key",
                IsEnabled = false
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent("News source not found")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _newsSourcesService.UpdateNewsSourceAsync(newsSourceId, updateRequest));
        }

        [Test]
        public async Task UpdateNewsSourceAsync_ServerError_HandlesError()
        {
            // Arrange
            var newsSourceId = 1;
            var updateRequest = new UpdateNewsSourceDTO
            {
                Name = "Updated Test Source",
                Url = "https://updatedtest.com",
                ApiKey = "updated-api-key",
                IsEnabled = false
            };

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
            Assert.DoesNotThrowAsync(async () => await _newsSourcesService.UpdateNewsSourceAsync(newsSourceId, updateRequest));
        }

        [Test]
        public async Task DeleteNewsSourceAsync_ValidId_Success()
        {
            // Arrange
            var newsSourceId = 1;

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("News source deleted successfully")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _newsSourcesService.DeleteNewsSourceAsync(newsSourceId));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri.ToString().EndsWith($"{ApplicationConstants.AddNewsSource}/{newsSourceId}")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task DeleteNewsSourceAsync_NotFound_HandlesError()
        {
            // Arrange
            var newsSourceId = 999;

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent("News source not found")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _newsSourcesService.DeleteNewsSourceAsync(newsSourceId));
        }

        [Test]
        public async Task DeleteNewsSourceAsync_ServerError_HandlesError()
        {
            // Arrange
            var newsSourceId = 1;

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
            Assert.DoesNotThrowAsync(async () => await _newsSourcesService.DeleteNewsSourceAsync(newsSourceId));
        }

        [Test]
        public async Task CreateNewsSourceAsync_ValidatesRequestContent()
        {
            // Arrange
            var createRequest = new CreateNewsSourceDTO
            {
                Name = "New Test Source",
                Url = "https://newtest.com",
                ApiKey = "test-api-key",
                IsEnabled = true
            };

            _mockHttpMessageHandler
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

            // Act
            await _newsSourcesService.CreateNewsSourceAsync(createRequest);

            // Assert
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.AddNewsSource) &&
                    req.Content.Headers.ContentType.MediaType == ApplicationConstants.JsonContentType),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task UpdateNewsSourceAsync_ValidatesRequestContent()
        {
            // Arrange
            var newsSourceId = 1;
            var updateRequest = new UpdateNewsSourceDTO
            {
                Name = "Updated Test Source",
                Url = "https://updatedtest.com",
                ApiKey = "updated-api-key",
                IsEnabled = false
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("News source updated successfully")
                });

            // Act
            await _newsSourcesService.UpdateNewsSourceAsync(newsSourceId, updateRequest);

            // Assert
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri.ToString().EndsWith($"{ApplicationConstants.AddNewsSource}/{newsSourceId}") &&
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
            var newsSources = await _newsSourcesService.GetAllNewsSourcesAsync();
            Assert.That(newsSources, Is.Empty);

            var newsSource = await _newsSourcesService.GetNewsSourceByIdAsync(1);
            Assert.That(newsSource, Is.Null);

            Assert.DoesNotThrowAsync(async () => await _newsSourcesService.CreateNewsSourceAsync(new CreateNewsSourceDTO()));
            Assert.DoesNotThrowAsync(async () => await _newsSourcesService.UpdateNewsSourceAsync(1, new UpdateNewsSourceDTO()));
            Assert.DoesNotThrowAsync(async () => await _newsSourcesService.DeleteNewsSourceAsync(1));
        }
    }
} 