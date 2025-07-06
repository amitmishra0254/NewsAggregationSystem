using Moq;
using Moq.Protected;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.NewsCategories;
using NewsAggregationSystem.Service.Interfaces;
using NewsAggregationSystem.Service.Services;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NewsAggregationSystem.Service.Tests.Services
{
    [TestFixture]
    public class TopicPredictionAdapterTests
    {
        private Mock<IHttpClientFactory> mockHttpClientFactory;
        private Mock<HttpMessageHandler> mockHttpMessageHandler;
        private Mock<INewsCategoryRepository> mockNewsCategoryRepository;
        private Mock<INewsCategoryService> mockNewsCategoryService;
        private Mock<INotificationPreferenceService> mockNotificationPreferenceService;
        private TopicPredictionAdapter topicPredictionAdapter;

        [SetUp]
        public void SetUp()
        {
            mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockNewsCategoryRepository = new Mock<INewsCategoryRepository>();
            mockNewsCategoryService = new Mock<INewsCategoryService>();
            mockNotificationPreferenceService = new Mock<INotificationPreferenceService>();

            topicPredictionAdapter = new TopicPredictionAdapter(
                mockHttpClientFactory.Object,
                mockNewsCategoryRepository.Object,
                mockNewsCategoryService.Object,
                mockNotificationPreferenceService.Object
            );
        }

        #region PredictTopicAsync Tests

        [Test]
        public async Task PredictTopicAsync_WhenSuccessfulResponse_ReturnsPredictedTopic()
        {
            var text = "Artificial Intelligence breakthrough in healthcare";
            var expectedTopic = "Technology";

            var responseDto = new TopicPredictionResponseDTO { Topic = expectedTopic };
            var responseJson = JsonSerializer.Serialize(responseDto);
            var responseContent = new StringContent(responseJson, Encoding.UTF8, ApplicationConstants.JsonContentType);

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            mockHttpClientFactory
                .Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var result = await topicPredictionAdapter.PredictTopicAsync(text);

            Assert.AreEqual(expectedTopic, result);
            mockHttpMessageHandler.Protected().Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().Contains(ApplicationConstants.TopicPredictionUrl)
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Test]
        public async Task PredictTopicAsync_WhenUnsuccessfulResponse_ReturnsAllCategory()
        {
            var text = "Some text for prediction";

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Error")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            mockHttpClientFactory
                .Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var result = await topicPredictionAdapter.PredictTopicAsync(text);

            Assert.AreEqual(CategoryType.All.ToString(), result);
        }

        #endregion

        #region ResolveCategory Tests

        [Test]
        public async Task ResolveCategory_WhenCategoryExists_ReturnsExistingCategoryId()
        {
            var categoryName = "Technology";
            var existingCategory = new NewsCategory
            {
                Id = 1,
                Name = "Technology",
                IsHidden = false
            };

            mockNewsCategoryRepository
                .Setup(repo => repo.GetSingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<NewsCategory, bool>>>()))
                .ReturnsAsync(existingCategory);

            var result = await topicPredictionAdapter.ResolveCategory(categoryName);

            Assert.AreEqual(existingCategory.Id, result);
            mockNewsCategoryService.Verify(service => service.AddNewsCategory(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            mockNotificationPreferenceService.Verify(service => service.AddNotificationPreferencesPerCategory(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task ResolveCategory_WhenCategoryDoesNotExist_CreatesNewCategory()
        {
            var categoryName = "newcategory";
            var titleCaseCategoryName = "Newcategory";
            var newCategoryId = 5;

            mockNewsCategoryRepository
                .Setup(repo => repo.GetSingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<NewsCategory, bool>>>()))
                .ReturnsAsync((NewsCategory)null);

            mockNewsCategoryService
                .Setup(service => service.AddNewsCategory(titleCaseCategoryName, ApplicationConstants.SystemUserId))
                .ReturnsAsync(newCategoryId);

            mockNotificationPreferenceService
                .Setup(service => service.AddNotificationPreferencesPerCategory(newCategoryId))
                .Returns(Task.CompletedTask);

            var result = await topicPredictionAdapter.ResolveCategory(categoryName);

            Assert.AreEqual(newCategoryId, result);
            mockNewsCategoryService.Verify(service => service.AddNewsCategory(titleCaseCategoryName, ApplicationConstants.SystemUserId), Times.Once);
            mockNotificationPreferenceService.Verify(service => service.AddNotificationPreferencesPerCategory(newCategoryId), Times.Once);
        }

        #endregion
    }
}