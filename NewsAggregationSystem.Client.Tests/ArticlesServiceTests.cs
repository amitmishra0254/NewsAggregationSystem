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
    public class ArticlesServiceTests
    {
        private Mock<HttpMessageHandler> mockHttpMessageHandler;
        private HttpClient httpClient;
        private ArticleService articlesService;
        private JsonSerializerOptions jsonOptions;

        [SetUp]
        public void Setup()
        {
            this.mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            this.httpClient = new HttpClient(mockHttpMessageHandler.Object);
            this.httpClient.BaseAddress = new Uri(ApplicationConstants.BaseUrl);
            this.articlesService = new ArticleService(httpClient);
            this.jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Test]
        public async Task GetUserSavedArticles_ValidResponse_ReturnsArticles()
        {
            var expectedArticles = new List<ArticleDTO>
            {
                new ArticleDTO { Id = 1, Title = "Test Article 1", Content = "Content 1" },
                new ArticleDTO { Id = 2, Title = "Test Article 2", Content = "Content 2" }
            };

            var responseContent = JsonSerializer.Serialize(expectedArticles, jsonOptions);

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

            var result = await articlesService.GetUserSavedArticlesAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Title, Is.EqualTo(expectedArticles[0].Title));
            Assert.That(result[1].Title, Is.EqualTo(expectedArticles[1].Title));

            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.SavedArticlesPath)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task GetUserSavedArticles_ServerError_ReturnsEmptyList()
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

            var result = await articlesService.GetUserSavedArticlesAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task SaveArticle_ValidArticleId_Success()
        {
            var articleId = 1;

            mockHttpMessageHandler
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

            Assert.DoesNotThrowAsync(async () => await articlesService.SaveArticleAsync(articleId));

            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith($"Articles/save-article/{articleId}")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task SaveArticle_ServerError_HandlesError()
        {
            var articleId = 1;

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

            Assert.DoesNotThrowAsync(async () => await articlesService.SaveArticleAsync(articleId));
        }

        [Test]
        public async Task RemoveSavedArticle_ValidArticleId_Success()
        {
            var articleId = 1;

            mockHttpMessageHandler
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

            Assert.DoesNotThrowAsync(async () => await articlesService.RemoveSavedArticleAsync(articleId));

            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Delete &&
                    req.RequestUri.ToString().EndsWith($"Articles/delete-saved-article/{articleId}")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task RemoveSavedArticle_ServerError_HandlesError()
        {
            var articleId = 1;

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

            Assert.DoesNotThrowAsync(async () => await articlesService.RemoveSavedArticleAsync(articleId));
        }

        [Test]
        public async Task ToggleArticleVisibility_ServerError_HandlesError()
        {
            var articleId = 1;
            var isHidden = true;

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

            Assert.DoesNotThrowAsync(async () => await articlesService.ToggleArticleVisibilityAsync(articleId, isHidden));
        }

        [Test]
        public async Task GetArticleById_ServerError_ReturnsNull()
        {
            var articleId = 1;

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

            var result = await articlesService.GetArticleByIdAsync(articleId);

            Assert.That(result, Is.Null);
        }

        [TearDown]
        public void TearDown()
        {
            httpClient?.Dispose();
        }
    }
}