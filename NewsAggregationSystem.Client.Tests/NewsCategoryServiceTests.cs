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
        private Mock<HttpMessageHandler> mockHttpMessageHandler;
        private HttpClient httpClient;
        private NewsCategoryService newsCategoryService;
        private JsonSerializerOptions jsonOptions;

        [SetUp]
        public void Setup()
        {
            this.mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            this.httpClient = new HttpClient(mockHttpMessageHandler.Object);
            this.httpClient.BaseAddress = new Uri(ApplicationConstants.BaseUrl);
            this.newsCategoryService = new NewsCategoryService(httpClient);
            this.jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Test]
        public async Task GetAllNewsCategories_ValidResponse_ReturnsCategories()
        {
            var expectedCategories = new List<NewsCategoryDTO>
            {
                new NewsCategoryDTO { Name = "Technology", IsEnabled = true },
                new NewsCategoryDTO { Name = "Sports", IsEnabled = false },
                new NewsCategoryDTO { Name = "Politics", IsEnabled = true }
            };

            var responseContent = JsonSerializer.Serialize(expectedCategories, jsonOptions);

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

            var result = await newsCategoryService.GetAllNewsCategoriesAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].Name, Is.EqualTo(expectedCategories[0].Name));
            Assert.That(result[1].Name, Is.EqualTo(expectedCategories[1].Name));
            Assert.That(result[2].Name, Is.EqualTo(expectedCategories[2].Name));

            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.AddNewsCategoryPath)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task GetAllNewsCategories_ServerError_ReturnsEmptyList()
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

            var result = await newsCategoryService.GetAllNewsCategoriesAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task CreateNewsCategory_ServerError_HandlesError()
        {
            var categoryName = "New Category";

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

            Assert.DoesNotThrowAsync(async () => await newsCategoryService.CreateNewsCategoryAsync(categoryName));
        }

        [Test]
        public async Task CreateNewsCategory_Conflict_HandlesError()
        {
            var categoryName = "Existing Category";

            mockHttpMessageHandler
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

            Assert.DoesNotThrowAsync(async () => await newsCategoryService.CreateNewsCategoryAsync(categoryName));
        }

        [Test]
        public async Task ToggleNewsCategoryVisibility_NotFound_HandlesError()
        {
            var categoryId = 999;
            var isHidden = true;

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

            Assert.DoesNotThrowAsync(async () => await newsCategoryService.ToggleNewsCategoryVisibilityAsync(categoryId, isHidden));
        }

        [Test]
        public async Task ToggleNewsCategoryVisibility_ServerError_HandlesError()
        {
            var categoryId = 1;
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

            Assert.DoesNotThrowAsync(async () => await newsCategoryService.ToggleNewsCategoryVisibilityAsync(categoryId, isHidden));
        }
    }
}