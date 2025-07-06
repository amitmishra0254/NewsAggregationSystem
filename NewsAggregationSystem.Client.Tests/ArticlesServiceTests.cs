using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NewsAggregationSystem.Client.Services;
using NewsAggregationSystem.Client.Services.Interfaces;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using System.Net;
using System.Text;
using System.Text.Json;
using NUnit.Framework;

namespace NewsAggregationSystem.Client.Tests
{
    [TestFixture]
    public class ArticlesServiceTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private ArticlesService _articlesService;
        private JsonSerializerOptions _jsonOptions;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _httpClient.BaseAddress = new Uri(ApplicationConstants.BaseUrl);
            _articlesService = new ArticlesService(_httpClient);
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Test]
        public async Task GetUserSavedArticlesAsync_ValidResponse_ReturnsArticles()
        {
            // Arrange
            var expectedArticles = new List<ArticleDTO>
            {
                new ArticleDTO { Id = 1, Title = "Test Article 1", Content = "Content 1" },
                new ArticleDTO { Id = 2, Title = "Test Article 2", Content = "Content 2" }
            };

            var responseContent = JsonSerializer.Serialize(expectedArticles, _jsonOptions);

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
            var result = await _articlesService.GetUserSavedArticlesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Title, Is.EqualTo(expectedArticles[0].Title));
            Assert.That(result[1].Title, Is.EqualTo(expectedArticles[1].Title));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.SavedArticlesPath)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task GetUserSavedArticlesAsync_ServerError_ReturnsEmptyList()
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
            var result = await _articlesService.GetUserSavedArticlesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetUserArticlesAsync_ValidRequest_ReturnsArticles()
        {
            // Arrange
            var request = new NewsArticleRequestDTO
            {
                CategoryId = 1,
                DateRange = new DateRangeDTO
                {
                    StartDate = DateTime.Now.AddDays(-7),
                    EndDate = DateTime.Now
                }
            };

            var expectedArticles = new List<ArticleDTO>
            {
                new ArticleDTO { Id = 1, Title = "News Article 1", Content = "Content 1" },
                new ArticleDTO { Id = 2, Title = "News Article 2", Content = "Content 2" }
            };

            var responseContent = JsonSerializer.Serialize(expectedArticles, _jsonOptions);

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
            var result = await _articlesService.GetUserArticlesAsync(request);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Title, Is.EqualTo(expectedArticles[0].Title));
            Assert.That(result[1].Title, Is.EqualTo(expectedArticles[1].Title));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.GetAllArticles)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task GetUserArticlesAsync_ServerError_ReturnsEmptyList()
        {
            // Arrange
            var request = new NewsArticleRequestDTO
            {
                CategoryId = 1,
                DateRange = new DateRangeDTO
                {
                    StartDate = DateTime.Now.AddDays(-7),
                    EndDate = DateTime.Now
                }
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

            // Act
            var result = await _articlesService.GetUserArticlesAsync(request);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task SaveArticleAsync_ValidArticleId_Success()
        {
            // Arrange
            var articleId = 1;

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Article saved successfully")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _articlesService.SaveArticleAsync(articleId));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith($"Articles/{articleId}/save")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task SaveArticleAsync_ServerError_HandlesError()
        {
            // Arrange
            var articleId = 1;

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
            Assert.DoesNotThrowAsync(async () => await _articlesService.SaveArticleAsync(articleId));
        }

        [Test]
        public async Task ReactToArticleAsync_ValidReaction_Success()
        {
            // Arrange
            var articleId = 1;
            var reactionId = 2; // Like reaction

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Reaction added successfully")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _articlesService.ReactToArticleAsync(articleId, reactionId));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith($"Articles/{articleId}/react/{reactionId}")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task ReactToArticleAsync_ServerError_HandlesError()
        {
            // Arrange
            var articleId = 1;
            var reactionId = 2;

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
            Assert.DoesNotThrowAsync(async () => await _articlesService.ReactToArticleAsync(articleId, reactionId));
        }

        [Test]
        public async Task RemoveSavedArticleAsync_ValidArticleId_Success()
        {
            // Arrange
            var articleId = 1;

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Article removed from saved list")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _articlesService.RemoveSavedArticleAsync(articleId));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri.ToString().EndsWith($"Articles/{articleId}/saved")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task RemoveSavedArticleAsync_ServerError_HandlesError()
        {
            // Arrange
            var articleId = 1;

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
            Assert.DoesNotThrowAsync(async () => await _articlesService.RemoveSavedArticleAsync(articleId));
        }

        [Test]
        public async Task ToggleArticleVisibilityAsync_ValidRequest_Success()
        {
            // Arrange
            var articleId = 1;
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
                    Content = new StringContent("Article visibility updated")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _articlesService.ToggleArticleVisibilityAsync(articleId, isHidden));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri.ToString().EndsWith($"Articles/{articleId}/visibility")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task ToggleArticleVisibilityAsync_ServerError_HandlesError()
        {
            // Arrange
            var articleId = 1;
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
            Assert.DoesNotThrowAsync(async () => await _articlesService.ToggleArticleVisibilityAsync(articleId, isHidden));
        }

        [Test]
        public async Task GetArticleByIdAsync_ValidArticleId_ReturnsArticle()
        {
            // Arrange
            var articleId = 1;
            var expectedArticle = new ArticleDTO
            {
                Id = articleId,
                Title = "Test Article",
                Content = "Test Content",
                Author = "Test Author",
                PublishedAt = DateTime.Now
            };

            var responseContent = JsonSerializer.Serialize(expectedArticle, _jsonOptions);

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
            var result = await _articlesService.GetArticleByIdAsync(articleId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(expectedArticle.Id));
            Assert.That(result.Title, Is.EqualTo(expectedArticle.Title));
            Assert.That(result.Content, Is.EqualTo(expectedArticle.Content));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().EndsWith($"Articles/{articleId}")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task GetArticleByIdAsync_ArticleNotFound_ReturnsNull()
        {
            // Arrange
            var articleId = 999;

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

            // Act
            var result = await _articlesService.GetArticleByIdAsync(articleId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetArticleByIdAsync_ServerError_ReturnsNull()
        {
            // Arrange
            var articleId = 1;

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
            var result = await _articlesService.GetArticleByIdAsync(articleId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetUserArticlesAsync_ValidatesRequestContent()
        {
            // Arrange
            var request = new NewsArticleRequestDTO
            {
                CategoryId = 1,
                DateRange = new DateRangeDTO
                {
                    StartDate = DateTime.Now.AddDays(-7),
                    EndDate = DateTime.Now
                }
            };

            var expectedArticles = new List<ArticleDTO>
            {
                new ArticleDTO { Id = 1, Title = "News Article 1", Content = "Content 1" }
            };

            var responseContent = JsonSerializer.Serialize(expectedArticles, _jsonOptions);

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
            await _articlesService.GetUserArticlesAsync(request);

            // Assert
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.GetAllArticles) &&
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
            var savedArticles = await _articlesService.GetUserSavedArticlesAsync();
            Assert.That(savedArticles, Is.Empty);

            var articles = await _articlesService.GetUserArticlesAsync(new NewsArticleRequestDTO());
            Assert.That(articles, Is.Empty);

            Assert.DoesNotThrowAsync(async () => await _articlesService.SaveArticleAsync(1));
            Assert.DoesNotThrowAsync(async () => await _articlesService.ReactToArticleAsync(1, 1));
            Assert.DoesNotThrowAsync(async () => await _articlesService.RemoveSavedArticleAsync(1));
            Assert.DoesNotThrowAsync(async () => await _articlesService.ToggleArticleVisibilityAsync(1, true));

            var article = await _articlesService.GetArticleByIdAsync(1);
            Assert.That(article, Is.Null);
        }
    }
} 