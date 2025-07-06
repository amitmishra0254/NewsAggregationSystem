using Moq;
using Moq.Protected;
using NewsAggregationSystem.Client.Services;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsCategories;
using NUnit.Framework;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Tests
{
    [TestFixture]
    public class NewsCategoryServiceTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private NewsCategoryService _newsCategoryService;
        private JsonSerializerOptions _jsonOptions;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _httpClient.BaseAddress = new Uri(ApplicationConstants.BaseUrl);
            _newsCategoryService = new NewsCategoryService(_httpClient);
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Test]
        public async Task GetAllNewsCategoriesAsync_ValidResponse_ReturnsCategories()
        {
            // Arrange
            var expectedCategories = new List<NewsCategoryDTO>
            {
                new NewsCategoryDTO { Id = 1, Name = "Technology", IsEnabled = true },
                new NewsCategoryDTO { Id = 2, Name = "Sports", IsEnabled = false },
                new NewsCategoryDTO { Id = 3, Name = "Politics", IsEnabled = true }
            };

            var responseContent = JsonSerializer.Serialize(expectedCategories, _jsonOptions);

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
            var result = await _newsCategoryService.GetAllNewsCategoriesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].Name, Is.EqualTo(expectedCategories[0].Name));
            Assert.That(result[1].Name, Is.EqualTo(expectedCategories[1].Name));
            Assert.That(result[2].Name, Is.EqualTo(expectedCategories[2].Name));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.AddNewsCategoryPath)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task GetAllNewsCategoriesAsync_ServerError_ReturnsEmptyList()
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
            var result = await _newsCategoryService.GetAllNewsCategoriesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task CreateNewsCategoryAsync_ValidRequest_Success()
        {
            // Arrange
            var categoryName = "New Category";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Category created successfully")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _newsCategoryService.CreateNewsCategoryAsync(categoryName));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.AddNewsCategoryPath) &&
                    req.Content.Headers.ContentType.MediaType == ApplicationConstants.JsonContentType),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task CreateNewsCategoryAsync_ServerError_HandlesError()
        {
            // Arrange
            var categoryName = "New Category";

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
            Assert.DoesNotThrowAsync(async () => await _newsCategoryService.CreateNewsCategoryAsync(categoryName));
        }

        [Test]
        public async Task CreateNewsCategoryAsync_Conflict_HandlesError()
        {
            // Arrange
            var categoryName = "Existing Category";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Content = new StringContent("Category already exists")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _newsCategoryService.CreateNewsCategoryAsync(categoryName));
        }

        [Test]
        public async Task ToggleCategoryVisibilityAsync_ValidRequest_Success()
        {
            // Arrange
            var categoryId = 1;
            var isHidden = true;

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Category visibility updated")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _newsCategoryService.ToggleCategoryVisibilityAsync(categoryId, isHidden));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri.ToString().EndsWith($"{ApplicationConstants.AddNewsCategoryPath}/{categoryId}/visibility")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task ToggleCategoryVisibilityAsync_NotFound_HandlesError()
        {
            // Arrange
            var categoryId = 999;
            var isHidden = true;

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
            Assert.DoesNotThrowAsync(async () => await _newsCategoryService.ToggleCategoryVisibilityAsync(categoryId, isHidden));
        }

        [Test]
        public async Task ToggleCategoryVisibilityAsync_ServerError_HandlesError()
        {
            // Arrange
            var categoryId = 1;
            var isHidden = true;

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
            Assert.DoesNotThrowAsync(async () => await _newsCategoryService.ToggleCategoryVisibilityAsync(categoryId, isHidden));
        }

        [Test]
        public async Task CreateNewsCategoryAsync_ValidatesRequestContent()
        {
            // Arrange
            var categoryName = "New Category";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Category created successfully")
                });

            // Act
            await _newsCategoryService.CreateNewsCategoryAsync(categoryName);

            // Assert
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.AddNewsCategoryPath) &&
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
            var categories = await _newsCategoryService.GetAllNewsCategoriesAsync();
            Assert.That(categories, Is.Empty);

            Assert.DoesNotThrowAsync(async () => await _newsCategoryService.CreateNewsCategoryAsync("Test Category"));
            Assert.DoesNotThrowAsync(async () => await _newsCategoryService.ToggleCategoryVisibilityAsync(1, true));
        }

        [Test]
        public async Task CreateNewsCategoryAsync_EmptyName_HandlesError()
        {
            // Arrange
            var categoryName = "";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Category name cannot be empty")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _newsCategoryService.CreateNewsCategoryAsync(categoryName));
        }

        [Test]
        public async Task ToggleCategoryVisibilityAsync_InvalidId_HandlesError()
        {
            // Arrange
            var categoryId = -1;
            var isHidden = true;

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Invalid category ID")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _newsCategoryService.ToggleCategoryVisibilityAsync(categoryId, isHidden));
        }
    }
} 