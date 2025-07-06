using AutoMapper;
using Moq;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsSources;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.NewsSources;
using NewsAggregationSystem.Service.Services;
using NewsAggregationSystem.Service.Tests.Utilities;
using System.Linq.Expressions;

namespace NewsAggregationSystem.Service.Tests.Services
{
    [TestFixture]
    public class NewsSourceServiceTests
    {
        private Mock<INewsSourceRepository> mockRepository;
        private Mock<IMapper> mockMapper;
        private NewsSourceService newsSourceService;

        [SetUp]
        public void SetUp()
        {
            mockRepository = new Mock<INewsSourceRepository>();
            mockMapper = new Mock<IMapper>();
            newsSourceService = new NewsSourceService(mockRepository.Object, mockMapper.Object);
        }

        #region GetAll Tests

        [Test]
        public async Task GetAll_WhenSourcesExist_ReturnsMappedDTOs()
        {
            var newsSources = new List<NewsSource>
            {
                new NewsSource
                {
                    Id = 1,
                    Name = "NewsAPI",
                    BaseUrl = "https://newsapi.org/v2",
                    ApiKey = "abc123def456ghi789jkl012mno345pqr678stu901vwx234yz",
                    IsActive = true,
                    LastAccess = DateTime.UtcNow.AddHours(-2)
                },
                new NewsSource
                {
                    Id = 2,
                    Name = "TheNewsAPI",
                    BaseUrl = "https://api.thenewsapi.com/v1",
                    ApiKey = "xyz789abc123def456ghi789jkl012mno345pqr678stu901",
                    IsActive = false,
                    LastAccess = DateTime.UtcNow.AddDays(-1)
                }
            };

            var newsSourceDTO = new List<NewsSourceDTO>
            {
                new NewsSourceDTO
                {
                    Id = 1,
                    Name = "NewsAPI",
                    BaseUrl = "https://newsapi.org/v2",
                    ApiKey = "abc123def456ghi789jkl012mno345pqr678stu901vwx234yz",
                    IsActive = true,
                    LastAccess = DateTime.UtcNow.AddHours(-2)
                },
                new NewsSourceDTO
                {
                    Id = 2,
                    Name = "TheNewsAPI",
                    BaseUrl = "https://api.thenewsapi.com/v1",
                    ApiKey = "xyz789abc123def456ghi789jkl012mno345pqr678stu901",
                    IsActive = false,
                    LastAccess = DateTime.UtcNow.AddDays(-1)
                }
            };

            mockRepository.Setup(repo => repo.GetAll()).Returns(newsSources.AsQueryable().BuildMockDbSet().Object);
            mockMapper.Setup(m => m.Map<List<NewsSourceDTO>>(newsSources)).Returns(newsSourceDTO);

            var result = await newsSourceService.GetAll();

            Assert.AreEqual(2, result.Count);
            mockMapper.Verify(m => m.Map<List<NewsSourceDTO>>(newsSources), Times.Once);
        }

        [Test]
        public async Task GetAll_WhenNoSourcesExist_ReturnsEmptyList()
        {
            var newsSources = new List<NewsSource>();
            var newsSourceDTO = new List<NewsSourceDTO>();

            mockRepository.Setup(repo => repo.GetAll()).Returns(newsSources.AsQueryable().BuildMockDbSet().Object);
            mockMapper.Setup(m => m.Map<List<NewsSourceDTO>>(newsSources)).Returns(newsSourceDTO);

            var result = await newsSourceService.GetAll();

            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region GetById Tests

        [Test]
        public async Task GetById_WhenSourceExists_ReturnsMappedDTO()
        {
            var id = 1;
            var newsSources = new NewsSource
            {
                Id = id,
                Name = "NewsAPI",
                BaseUrl = "https://newsapi.org/v2",
                ApiKey = "abc123def456ghi789jkl012mno345pqr678stu901vwx234yz",
                IsActive = true,
                LastAccess = DateTime.UtcNow.AddHours(-1)
            };
            var newsSourceDTO = new NewsSourceDTO
            {
                Id = id,
                Name = "NewsAPI",
                BaseUrl = "https://newsapi.org/v2",
                ApiKey = "abc123def456ghi789jkl012mno345pqr678stu901vwx234yz",
                IsActive = true,
                LastAccess = DateTime.UtcNow.AddHours(-1)
            };

            mockRepository.Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<NewsSource, bool>>>()))
                .Returns(new List<NewsSource> { newsSources }.AsQueryable().BuildMockDbSet().Object);
            mockMapper.Setup(m => m.Map<NewsSourceDTO>(newsSources)).Returns(newsSourceDTO);

            var result = await newsSourceService.GetById(id);

            Assert.IsNotNull(result);
            Assert.AreEqual(id, result.Id);
        }

        [Test]
        public async Task GetById_WhenSourceDoesNotExist_ReturnsNull()
        {
            var id = 999;

            mockRepository.Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<NewsSource, bool>>>()))
                .Returns(new List<NewsSource>().AsQueryable().BuildMockDbSet().Object);

            var result = await newsSourceService.GetById(id);

            Assert.IsNull(result);
        }

        #endregion

        #region Add Tests

        [Test]
        public async Task Add_WhenValidRequest_AddsNewsSource()
        {
            var request = new CreateNewsSourceDTO
            {
                Name = "GuardianAPI",
                BaseUrl = "https://content.guardianapis.com/search",
                ApiKey = "guardian123def456ghi789jkl012mno345pqr678stu901vwx234yz"
            };
            var userId = 123;
            var newsSource = new NewsSource
            {
                Name = "GuardianAPI",
                BaseUrl = "https://content.guardianapis.com/search",
                ApiKey = "guardian123def456ghi789jkl012mno345pqr678stu901vwx234yz"
            };

            mockMapper.Setup(m => m.Map<NewsSource>(request)).Returns(newsSource);
            mockRepository.Setup(repo => repo.AddAsync(It.IsAny<NewsSource>())).ReturnsAsync(1);

            await newsSourceService.Add(request, userId);

            mockRepository.Verify(repo => repo.AddAsync(It.Is<NewsSource>(n =>
                n.Name == request.Name &&
                n.BaseUrl == request.BaseUrl &&
                n.ApiKey == request.ApiKey &&
                n.IsActive == false &&
                n.CreatedById == userId
            )), Times.Once);
        }

        #endregion

        #region Update Tests

        [Test]
        public async Task Update_WhenSourceExists_UpdatesSuccessfully()
        {
            var id = 1;
            var userId = 123;
            var request = new UpdateNewsSourceDTO
            {
                ApiKey = "newkey456def789ghi012jkl345mno678pqr901stu234vwx567yz",
                IsActive = true
            };
            var newsSource = new NewsSource
            {
                Id = id,
                Name = "NewsAPI",
                BaseUrl = "https://newsapi.org/v2",
                ApiKey = "oldkey123def456ghi789jkl012mno345pqr678stu901vwx234yz",
                IsActive = false
            };

            mockRepository.Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<NewsSource, bool>>>()))
                .Returns(new List<NewsSource> { newsSource }.AsQueryable().BuildMockDbSet().Object);
            mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<NewsSource>())).ReturnsAsync(1);

            await newsSourceService.Update(id, request, userId);

            mockRepository.Verify(repo => repo.UpdateAsync(It.Is<NewsSource>(n =>
                n.ApiKey == request.ApiKey &&
                n.IsActive == request.IsActive &&
                n.ModifiedById == userId
            )), Times.Once);
        }

        [Test]
        public void Update_WhenSourceDoesNotExist_ThrowsNotFoundException()
        {
            var id = 999;
            var userId = 123;
            var request = new UpdateNewsSourceDTO
            {
                ApiKey = "newkey456def789ghi012jkl345mno678pqr901stu234vwx567yz",
                IsActive = true
            };

            mockRepository.Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<NewsSource, bool>>>()))
                .Returns(new List<NewsSource>().AsQueryable().BuildMockDbSet().Object);

            var exception = Assert.ThrowsAsync<NotFoundException>(async () =>
                await newsSourceService.Update(id, request, userId));

            Assert.IsTrue(exception.Message.Contains(ApplicationConstants.NewsSourceNotFoundMessage));
        }

        #endregion

        #region Delete Tests

        [Test]
        public async Task Delete_WhenSourceExists_DeletesSuccessfully()
        {
            var id = 1;
            var newsSource = new NewsSource
            {
                Id = id,
                Name = "NewsAPI",
                BaseUrl = "https://newsapi.org/v2",
                ApiKey = "abc123def456ghi789jkl012mno345pqr678stu901vwx234yz",
                IsActive = true,
                LastAccess = DateTime.UtcNow.AddHours(-1)
            };

            mockRepository.Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<NewsSource, bool>>>()))
                .Returns(new List<NewsSource> { newsSource }.AsQueryable().BuildMockDbSet().Object);
            mockRepository.Setup(repo => repo.DeleteAsync(It.IsAny<NewsSource>())).ReturnsAsync(1);

            await newsSourceService.Delete(id);

            mockRepository.Verify(repo => repo.DeleteAsync(newsSource), Times.Once);
        }

        [Test]
        public void Delete_WhenSourceDoesNotExist_ThrowsNotFoundException()
        {
            var id = 999;

            mockRepository.Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<NewsSource, bool>>>()))
                .Returns(new List<NewsSource>().AsQueryable().BuildMockDbSet().Object);

            var exception = Assert.ThrowsAsync<NotFoundException>(async () =>
                await newsSourceService.Delete(id));

            Assert.IsTrue(exception.Message.Contains(ApplicationConstants.NewsSourceNotFoundMessage));
        }

        #endregion
    }
}