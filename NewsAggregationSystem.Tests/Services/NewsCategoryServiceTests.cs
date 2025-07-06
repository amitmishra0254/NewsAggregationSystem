using Moq;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.NewsCategories;
using NewsAggregationSystem.Service.Services;
using NewsAggregationSystem.Service.Tests.Utilities;
using System.Linq.Expressions;

namespace NewsAggregationSystem.Service.Tests.Services
{
    [TestFixture]
    public class NewsCategoryServiceTests
    {
        private Mock<INewsCategoryRepository> mockNewsCategoryRepository;
        private NewsCategoryService newsCategoryService;

        [SetUp]
        public void SetUp()
        {
            mockNewsCategoryRepository = new Mock<INewsCategoryRepository>();
            newsCategoryService = new NewsCategoryService(mockNewsCategoryRepository.Object);
        }

        #region AddNewsCategory Tests

        [Test]
        public async Task AddNewsCategory_WhenValidNameProvided_ReturnsCategoryId()
        {
            var name = "Technology";
            var userId = 123;

            mockNewsCategoryRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<NewsCategory, bool>>>()))
                .Returns(new List<NewsCategory>().AsQueryable().BuildMockDbSet().Object);

            mockNewsCategoryRepository
                .Setup(repo => repo.AddAsync(It.IsAny<NewsCategory>()))
                .ReturnsAsync(1);

            var result = await newsCategoryService.AddNewsCategory(name, userId);

            Assert.AreEqual(0, result);
            mockNewsCategoryRepository.Verify(repo => repo.AddAsync(It.Is<NewsCategory>(c =>
                c.Name == name &&
                c.CreatedById == userId &&
                c.CreatedDate != default
            )), Times.Once);
        }

        [Test]
        public void AddNewsCategory_WhenCategoryAlreadyExists_ThrowsAlreadyExistException()
        {
            var name = "Technology";
            var userId = 123;

            var existingCategory = new NewsCategory { Id = 1, Name = name };
            mockNewsCategoryRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<NewsCategory, bool>>>()))
                .Returns(new List<NewsCategory> { existingCategory }.AsQueryable().BuildMockDbSet().Object);

            var exception = Assert.ThrowsAsync<AlreadyExistException>(async () =>
                await newsCategoryService.AddNewsCategory(name, userId));

            Assert.IsTrue(exception.Message.Contains(name));
        }

        #endregion

        #region ToggleVisibility Tests

        [Test]
        public async Task ToggleVisibility_WhenCategoryExistsAndVisibilityChanged_ReturnsOne()
        {
            var categoryId = 1;
            var isHidden = true;

            var existingCategory = new NewsCategory
            {
                Id = categoryId,
                IsHidden = false
            };

            mockNewsCategoryRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<NewsCategory, bool>>>()))
                .Returns(new List<NewsCategory> { existingCategory }.AsQueryable().BuildMockDbSet().Object);

            mockNewsCategoryRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<NewsCategory>()))
                .ReturnsAsync(1);

            var result = await newsCategoryService.ToggleVisibility(categoryId, isHidden);

            Assert.AreEqual(1, result);
            mockNewsCategoryRepository.Verify(repo => repo.UpdateAsync(It.Is<NewsCategory>(c =>
                c.Id == categoryId &&
                c.IsHidden == isHidden
            )), Times.Once);
        }

        [Test]
        public void ToggleVisibility_WhenCategoryDoesNotExist_ThrowsNotFoundException()
        {
            var categoryId = 999;
            var isHidden = true;

            mockNewsCategoryRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<NewsCategory, bool>>>()))
                .Returns(new List<NewsCategory>().AsQueryable().BuildMockDbSet().Object);

            var exception = Assert.ThrowsAsync<NotFoundException>(async () =>
                await newsCategoryService.ToggleVisibility(categoryId, isHidden));

            Assert.IsTrue(exception.Message.Contains(categoryId.ToString()));
        }

        [Test]
        public async Task ToggleVisibility_WhenVisibilityIsSame_ReturnsZero()
        {
            var categoryId = 1;
            var isHidden = true;

            var existingCategory = new NewsCategory
            {
                Id = categoryId,
                IsHidden = true
            };

            mockNewsCategoryRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<NewsCategory, bool>>>()))
                .Returns(new List<NewsCategory> { existingCategory }.AsQueryable().BuildMockDbSet().Object);

            var result = await newsCategoryService.ToggleVisibility(categoryId, isHidden);

            Assert.AreEqual(0, result);
            mockNewsCategoryRepository.Verify(repo => repo.UpdateAsync(It.IsAny<NewsCategory>()), Times.Never);
        }

        #endregion

        #region GetAllCategories Tests

        [Test]
        public async Task GetAllCategories_WhenUserRoleIsUser_ReturnsOnlyVisibleCategories()
        {
            var userRole = UserRoles.User.ToString();

            var categories = new List<NewsCategory>
            {
                new NewsCategory { Id = 1, Name = "Technology", IsHidden = false },
                new NewsCategory { Id = 2, Name = "Sports", IsHidden = false },
                new NewsCategory { Id = 3, Name = "Politics", IsHidden = false }
            };

            mockNewsCategoryRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<NewsCategory, bool>>>()))
                .Returns(categories.AsQueryable().BuildMockDbSet().Object);

            var result = await newsCategoryService.GetAllCategories(userRole);

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public async Task GetAllCategories_WhenUserRoleIsAdmin_ReturnsAllCategories()
        {
            var userRole = UserRoles.Admin.ToString();

            var categories = new List<NewsCategory>
            {
                new NewsCategory { Id = 1, Name = "Technology", IsHidden = false },
                new NewsCategory { Id = 2, Name = "Sports", IsHidden = false },
                new NewsCategory { Id = 3, Name = "Politics", IsHidden = true }
            };

            mockNewsCategoryRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<NewsCategory, bool>>>()))
                .Returns(categories.AsQueryable().BuildMockDbSet().Object);

            var result = await newsCategoryService.GetAllCategories(userRole);

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public async Task GetAllCategories_WhenUserRoleIsInvalid_ReturnsAllCategories()
        {
            var userRole = "InvalidRole";

            var categories = new List<NewsCategory>
            {
                new NewsCategory { Id = 1, Name = "Technology", IsHidden = false },
                new NewsCategory { Id = 2, Name = "Sports", IsHidden = true }
            };

            mockNewsCategoryRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<NewsCategory, bool>>>()))
                .Returns(categories.AsQueryable().BuildMockDbSet().Object);

            var result = await newsCategoryService.GetAllCategories(userRole);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        #endregion
    }
}