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
    public class AdminServiceTests
    {
        private Mock<HttpMessageHandler> mockHttpMessageHandler;
        private HttpClient httpClient;
        private AdminService adminService;
        private JsonSerializerOptions jsonOptions;

        [SetUp]
        public void Setup()
        {
            this.mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            this.httpClient = new HttpClient(mockHttpMessageHandler.Object);
            this.httpClient.BaseAddress = new Uri(ApplicationConstants.BaseUrl);
            this.adminService = new AdminService(httpClient);
            this.jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Test]
        public async Task AddKeywordToHideArticles_ServerError_HandlesError()
        {
            var keyword = "spam";
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

            Assert.DoesNotThrowAsync(async () => await adminService.AddKeywordToHideArticlesAsync(keyword));
        }

        [Test]
        public async Task AddKeywordToHideArticles_Conflict_HandlesError()
        {
            var keyword = "existing-keyword";
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


            Assert.DoesNotThrowAsync(async () => await adminService.AddKeywordToHideArticlesAsync(keyword));
        }

        [Test]
        public async Task AddKeywordToHideArticles_BadRequest_HandlesError()
        {
            var keyword = "";
            mockHttpMessageHandler
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

            Assert.DoesNotThrowAsync(async () => await adminService.AddKeywordToHideArticlesAsync(keyword));
        }

        [Test]
        public async Task AddKeywordToHideArticles_Unauthorized_HandlesError()
        {
            var keyword = "spam";
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

            Assert.DoesNotThrowAsync(async () => await adminService.AddKeywordToHideArticlesAsync(keyword));
        }

        [Test]
        public async Task AddKeywordToHideArticles_Forbidden_HandlesError()
        {
            var keyword = "spam";
            mockHttpMessageHandler
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

            Assert.DoesNotThrowAsync(async () => await adminService.AddKeywordToHideArticlesAsync(keyword));
        }

        [Test]
        public async Task AddKeywordToHideArticles_NullKeyword_HandlesError()
        {
            string keyword = null;

            mockHttpMessageHandler
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

            Assert.DoesNotThrowAsync(async () => await adminService.AddKeywordToHideArticlesAsync(keyword));
        }

        [TearDown]
        public void TearDown()
        {
            httpClient?.Dispose();
        }
    }
}