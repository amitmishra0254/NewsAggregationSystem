using Moq;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;
using NewsAggregationSystem.Service.Services;

namespace NewsAggregationSystem.Service.Tests.Services
{
    [TestFixture]
    public class HiddenArticleKeywordServiceTests
    {
        private Mock<IRepositoryBase<HiddenArticleKeyword>> mockHiddenArticleKeywordRepository;
        private HiddenArticleKeywordService hiddenArticleKeywordService;

        [SetUp]
        public void SetUp()
        {
            mockHiddenArticleKeywordRepository = new Mock<IRepositoryBase<HiddenArticleKeyword>>();
            hiddenArticleKeywordService = new HiddenArticleKeywordService(mockHiddenArticleKeywordRepository.Object);
        }

        [Test]
        public async Task Add_WhenValidKeywordProvided_ReturnsOne()
        {
            var keyword = "politics";
            var userId = 123;

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.AddAsync(It.IsAny<HiddenArticleKeyword>()))
                .ReturnsAsync(1);

            var result = await hiddenArticleKeywordService.AddHiddenKeywordAsync(keyword, userId);

            Assert.AreEqual(1, result);
            mockHiddenArticleKeywordRepository.Verify(repo => repo.AddAsync(It.Is<HiddenArticleKeyword>(k =>
                k.Name == keyword.ToLower() &&
                k.CreatedById == userId &&
                k.CreatedDate != default
            )), Times.Once);
        }

        [Test]
        public async Task Add_WhenEmptyKeyword_ConvertsToLowerCase()
        {
            var keyword = "";
            var userId = 123;

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.AddAsync(It.IsAny<HiddenArticleKeyword>()))
                .ReturnsAsync(1);

            var result = await hiddenArticleKeywordService.AddHiddenKeywordAsync(keyword, userId);

            Assert.AreEqual(1, result);
            mockHiddenArticleKeywordRepository.Verify(repo => repo.AddAsync(It.Is<HiddenArticleKeyword>(k =>
                k.Name == keyword.ToLower() &&
                k.CreatedById == userId
            )), Times.Once);
        }

        [Test]
        public async Task Add_WhenNullKeyword_ConvertsToLowerCase()
        {
            string keyword = null;
            var userId = 123;

            string expectedKeyword = keyword?.ToLower();

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.AddAsync(It.IsAny<HiddenArticleKeyword>()))
                .ReturnsAsync(1);

            var result = await hiddenArticleKeywordService.AddHiddenKeywordAsync(keyword, userId);

            Assert.AreEqual(1, result);
            mockHiddenArticleKeywordRepository.Verify(repo => repo.AddAsync(It.Is<HiddenArticleKeyword>(k =>
                k.Name == expectedKeyword &&
                k.CreatedById == userId
            )), Times.Once);
        }

        [Test]
        public async Task Add_WhenRepositoryReturnsZero_ReturnsZero()
        {
            var keyword = "politics";
            var userId = 123;

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.AddAsync(It.IsAny<HiddenArticleKeyword>()))
                .ReturnsAsync(0);

            var result = await hiddenArticleKeywordService.AddHiddenKeywordAsync(keyword, userId);

            Assert.AreEqual(0, result);
            mockHiddenArticleKeywordRepository.Verify(repo => repo.AddAsync(It.IsAny<HiddenArticleKeyword>()), Times.Once);
        }

        [Test]
        public async Task Add_WhenRepositoryReturnsMultiple_ReturnsCorrectCount()
        {
            var keyword = "politics";
            var userId = 123;

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.AddAsync(It.IsAny<HiddenArticleKeyword>()))
                .ReturnsAsync(5);

            var result = await hiddenArticleKeywordService.AddHiddenKeywordAsync(keyword, userId);

            Assert.AreEqual(5, result);
            mockHiddenArticleKeywordRepository.Verify(repo => repo.AddAsync(It.IsAny<HiddenArticleKeyword>()), Times.Once);
        }

        [Test]
        public async Task Add_WhenUserIdIsZero_SetsCreatedByIdToZero()
        {
            var keyword = "politics";
            var userId = 0;

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.AddAsync(It.IsAny<HiddenArticleKeyword>()))
                .ReturnsAsync(1);

            var result = await hiddenArticleKeywordService.AddHiddenKeywordAsync(keyword, userId);

            Assert.AreEqual(1, result);
            mockHiddenArticleKeywordRepository.Verify(repo => repo.AddAsync(It.Is<HiddenArticleKeyword>(k =>
                k.Name == keyword.ToLower() &&
                k.CreatedById == userId
            )), Times.Once);
        }

        [Test]
        public async Task Add_WhenUserIdIsNegative_SetsCreatedByIdToNegative()
        {
            var keyword = "politics";
            var userId = -1;

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.AddAsync(It.IsAny<HiddenArticleKeyword>()))
                .ReturnsAsync(1);

            var result = await hiddenArticleKeywordService.AddHiddenKeywordAsync(keyword, userId);

            Assert.AreEqual(1, result);
            mockHiddenArticleKeywordRepository.Verify(repo => repo.AddAsync(It.Is<HiddenArticleKeyword>(k =>
                k.Name == keyword.ToLower() &&
                k.CreatedById == userId
            )), Times.Once);
        }

        [Test]
        public async Task Add_WhenUserIdIsLargeNumber_SetsCreatedByIdCorrectly()
        {
            var keyword = "politics";
            var userId = int.MaxValue;

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.AddAsync(It.IsAny<HiddenArticleKeyword>()))
                .ReturnsAsync(1);

            var result = await hiddenArticleKeywordService.AddHiddenKeywordAsync(keyword, userId);

            Assert.AreEqual(1, result);
            mockHiddenArticleKeywordRepository.Verify(repo => repo.AddAsync(It.Is<HiddenArticleKeyword>(k =>
                k.Name == keyword.ToLower() &&
                k.CreatedById == userId
            )), Times.Once);
        }

        [Test]
        public async Task Add_WhenKeywordIsVeryLong_ConvertsToLowerCase()
        {
            var keyword = new string('A', 1000);
            var userId = 123;

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.AddAsync(It.IsAny<HiddenArticleKeyword>()))
                .ReturnsAsync(1);

            var result = await hiddenArticleKeywordService.AddHiddenKeywordAsync(keyword, userId);

            Assert.AreEqual(1, result);
            mockHiddenArticleKeywordRepository.Verify(repo => repo.AddAsync(It.Is<HiddenArticleKeyword>(k =>
                k.Name == keyword.ToLower() &&
                k.CreatedById == userId
            )), Times.Once);
        }


        [Test]
        public async Task Add_WhenRepositoryThrowsException_PropagatesException()
        {
            var keyword = "politics";
            var userId = 123;
            var expectedException = new InvalidOperationException("Database error");

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.AddAsync(It.IsAny<HiddenArticleKeyword>()))
                .ThrowsAsync(expectedException);

            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await hiddenArticleKeywordService.AddHiddenKeywordAsync(keyword, userId));

            Assert.AreEqual(expectedException, exception);
        }

        [Test]
        public async Task Add_WhenCalledMultipleTimes_CreatesMultipleKeywords()
        {
            var keyword1 = "politics";
            var keyword2 = "sports";
            var userId = 123;

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.AddAsync(It.IsAny<HiddenArticleKeyword>()))
                .ReturnsAsync(1);

            var result1 = await hiddenArticleKeywordService.AddHiddenKeywordAsync(keyword1, userId);
            var result2 = await hiddenArticleKeywordService.AddHiddenKeywordAsync(keyword2, userId);

            Assert.AreEqual(1, result1);
            Assert.AreEqual(1, result2);
            mockHiddenArticleKeywordRepository.Verify(repo => repo.AddAsync(It.Is<HiddenArticleKeyword>(k =>
                k.Name == keyword1.ToLower() &&
                k.CreatedById == userId
            )), Times.Once);
            mockHiddenArticleKeywordRepository.Verify(repo => repo.AddAsync(It.Is<HiddenArticleKeyword>(k =>
                k.Name == keyword2.ToLower() &&
                k.CreatedById == userId
            )), Times.Once);
        }

        [Test]
        public async Task Add_WhenKeywordIsSingleCharacter_ConvertsToLowerCase()
        {
            var keyword = "A";
            var userId = 123;

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.AddAsync(It.IsAny<HiddenArticleKeyword>()))
                .ReturnsAsync(1);

            var result = await hiddenArticleKeywordService.AddHiddenKeywordAsync(keyword, userId);

            Assert.AreEqual(1, result);
            mockHiddenArticleKeywordRepository.Verify(repo => repo.AddAsync(It.Is<HiddenArticleKeyword>(k =>
                k.Name == keyword.ToLower() &&
                k.CreatedById == userId
            )), Times.Once);
        }
    }
}