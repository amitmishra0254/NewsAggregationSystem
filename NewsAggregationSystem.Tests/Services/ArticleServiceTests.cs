using AutoMapper;
using Moq;
using Moq.Protected;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.Common.DTOs.NewsCategories;
using NewsAggregationSystem.Common.DTOs.NotificationPreferences;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Articles;
using NewsAggregationSystem.DAL.Repositories.Generic;
using NewsAggregationSystem.Service.Interfaces;
using NewsAggregationSystem.Service.Services;
using NewsAggregationSystem.Service.Tests.Utilities;
using System.Linq.Expressions;

namespace NewsAggregationSystem.Service.Tests.Services
{
    [TestFixture]
    public class ArticleServiceTests
    {
        private Mock<IArticleRepository> mockArticleRepository;
        private Mock<IMapper> mockMapper;
        private Mock<IRepositoryBase<SavedArticle>> mockSavedArticleRepository;
        private Mock<IRepositoryBase<ArticleReaction>> mockArticleReactionRepository;
        private Mock<IRepositoryBase<HiddenArticleKeyword>> mockHiddenArticleKeywordRepository;
        private Mock<IRepositoryBase<ArticleReadHistory>> mockArticleReadHistoryRepository;
        private Mock<INotificationPreferenceService> mockNotificationPreferenceService;
        private Mock<ArticleService> mockArticleService;
        private ArticleService articleService;

        [SetUp]
        public void SetUp()
        {
            mockArticleRepository = new Mock<IArticleRepository>();
            mockMapper = new Mock<IMapper>();
            mockSavedArticleRepository = new Mock<IRepositoryBase<SavedArticle>>();
            mockArticleReactionRepository = new Mock<IRepositoryBase<ArticleReaction>>();
            mockHiddenArticleKeywordRepository = new Mock<IRepositoryBase<HiddenArticleKeyword>>();
            mockArticleReadHistoryRepository = new Mock<IRepositoryBase<ArticleReadHistory>>();
            mockNotificationPreferenceService = new Mock<INotificationPreferenceService>();

            mockArticleService = new Mock<ArticleService>(
                mockArticleRepository.Object,
                mockMapper.Object,
                mockSavedArticleRepository.Object,
                mockArticleReactionRepository.Object,
                mockHiddenArticleKeywordRepository.Object,
                mockArticleReadHistoryRepository.Object,
                mockNotificationPreferenceService.Object
            );

            mockArticleService.CallBase = true;
            articleService = mockArticleService.Object;

            mockArticleService
                .Protected()
                .Setup<Task<List<Article>>>("GetArticlesByCategory",
                    ItExpr.IsAny<int>(),
                    ItExpr.IsAny<List<NotificationPreferenceDTO>>(),
                    ItExpr.IsAny<List<Article>>())
                .ReturnsAsync((int catId, List<NotificationPreferenceDTO> prefs, List<Article> articles) =>
                    articles.Where(a => a.NewsCategoryId == catId).ToList());
        }

        [Test]
        public async Task HideArticle_WhenArticleExists_SetsIsHiddenToTrueAndUpdatesArticle()
        {
            int articleId = 1;
            var article = CreateVisibleArticle(articleId);

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(new List<Article> { article }.AsQueryable().BuildMockDbSet().Object);

            mockArticleRepository
                .Setup(repo => repo.UpdateAsync(article))
                .ReturnsAsync(1);

            var result = await articleService.HideArticle(articleId);

            Assert.AreEqual(1, result);
            Assert.IsTrue(article.IsHidden);
            mockArticleRepository.Verify(repo => repo.UpdateAsync(article), Times.Once);
        }

        [Test]
        public void HideArticle_WhenArticleDoesNotExist_ThrowsNotFoundException()
        {
            int articleId = 1;

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(new List<Article>().AsQueryable().BuildMockDbSet().Object);

            Assert.ThrowsAsync<NotFoundException>(async () => await articleService.HideArticle(articleId));
        }


        [Test]
        public async Task DeleteSavedArticles_WhenSavedArticleExists_DeletesAndReturnsOne()
        {
            int articleId = 1;
            int userId = 123;

            var article = new Article { Id = articleId };
            var savedArticle = new SavedArticle { ArticleId = articleId, UserId = userId };

            var articleQueryable = new List<Article> { article }.AsQueryable();
            var savedArticleQueryable = new List<SavedArticle> { savedArticle }.AsQueryable();

            var mockArticleDbSet = articleQueryable.AsMockDbSet();
            var mockSavedArticleDbSet = savedArticleQueryable.AsMockDbSet();

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(mockArticleDbSet.Object);

            mockSavedArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<SavedArticle, bool>>>()))
                .Returns(mockSavedArticleDbSet.Object);

            mockSavedArticleRepository
                .Setup(repo => repo.DeleteAsync(savedArticle))
                .ReturnsAsync(1);

            var result = await articleService.DeleteUserSavedArticleAsync(articleId, userId);

            Assert.AreEqual(1, result);
            mockSavedArticleRepository.Verify(r => r.DeleteAsync(savedArticle), Times.Once);
        }

        [Test]
        public async Task DeleteSavedArticles_WhenSavedArticleDoesNotExist_ReturnsZero()
        {
            int articleId = 1;
            int userId = 123;

            var article = new Article { Id = articleId };

            var articleQueryable = new List<Article> { article }.AsQueryable();
            var savedArticleQueryable = new List<SavedArticle>().AsQueryable();

            var mockArticleDbSet = articleQueryable.AsMockDbSet();
            var mockSavedArticleDbSet = savedArticleQueryable.AsMockDbSet();

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(mockArticleDbSet.Object);

            mockSavedArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<SavedArticle, bool>>>()))
                .Returns(mockSavedArticleDbSet.Object);

            var result = await articleService.DeleteUserSavedArticleAsync(articleId, userId);

            Assert.AreEqual(0, result);
            mockSavedArticleRepository.Verify(r => r.DeleteAsync(It.IsAny<SavedArticle>()), Times.Never);
        }


        [Test]
        public async Task GetAllSavedArticles_WhenArticlesExist_ReturnsMappedArticles()
        {
            int userId = 123;

            var articles = new List<Article>
            {
                new Article
                {
                    Id = 1,
                    Title = "Test Article",
                    IsHidden = false,
                    NewsCategory = new NewsCategory { IsHidden = false },
                    ArticleReactions = new List<ArticleReaction>()
                }
            };

            var savedArticles = new List<SavedArticle>
            {
                new SavedArticle
                {
                    UserId = userId,
                    ArticleId = 1,
                    Article = articles[0]
                }
            }.AsQueryable();

            var mockDbSet = savedArticles.BuildMockDbSet();

            mockSavedArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<SavedArticle, bool>>>()))
                .Returns(mockDbSet.Object);

            mockMapper
                .Setup(m => m.Map<List<ArticleDTO>>(It.IsAny<List<Article>>()))
                .Returns(new List<ArticleDTO>
                {
                    new ArticleDTO
                    {
                        Id = 1,
                        Title = "Test Article",
                        Description = "Test Desc",
                        Content = "Test Content"
                    }
                });

            var result = await articleService.GetUserSavedArticlesAsync(userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Test Article", result[0].Title);
        }

        [Test]
        public async Task GetAllSavedArticles_WhenNoSavedArticlesExist_ReturnsEmptyList()
        {
            int userId = 123;

            var savedArticles = new List<SavedArticle>()
                .AsQueryable();

            var mockDbSet = savedArticles.BuildMockDbSet();

            mockSavedArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<SavedArticle, bool>>>()))
                .Returns(mockDbSet.Object);

            var result = await articleService.GetUserSavedArticlesAsync(userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);

            mockMapper.Verify(m => m.Map<List<ArticleDTO>>(It.IsAny<List<Article>>()), Times.Never);
        }


        [Test]
        public async Task GetArticleById_WhenArticleExists_AddsReadHistoryAndReturnsMappedDTO()
        {
            int articleId = 1;
            int userId = 123;
            var article = CreateTestArticle(articleId);
            var expectedDto = CreateExpectedArticleDTO(articleId);

            mockArticleRepository
                .Setup(repo => repo.GetArticleById(articleId, userId))
                .ReturnsAsync(article);

            mockMapper
                .Setup(mapper => mapper.Map<ArticleDTO>(article))
                .Returns(expectedDto);

            var actualResult = await articleService.GetUserArticleByIdAsync(articleId, userId);

            AssertArticleDTOProperties(actualResult, expectedDto);
            VerifyReadHistoryWasAdded(articleId, userId);
        }

        [Test]
        public void GetArticleById_WhenArticleDoesNotExist_ThrowsNotFoundException()
        {
            int articleId = 1;
            int userId = 123;

            mockArticleRepository
                .Setup(repo => repo.GetArticleById(articleId, userId))
                .ReturnsAsync((Article)null);

            Assert.ThrowsAsync<NotFoundException>(() => articleService.GetUserArticleByIdAsync(articleId, userId));

            mockMapper.Verify(m => m.Map<ArticleDTO>(It.IsAny<Article>()), Times.Never);
            mockArticleReadHistoryRepository.Verify(r => r.AddAsync(It.IsAny<ArticleReadHistory>()), Times.Never);
        }


        [Test]
        public async Task ReactArticle_WhenNoExistingReaction_AddsNewReaction()
        {
            int articleId = 1;
            int userId = 123;
            int reactionId = 2;

            var articleList = new List<Article>
            {
                new Article { Id = articleId }
            }.AsQueryable().BuildMockDbSet();

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(articleList.Object);

            var emptyReactionList = new List<ArticleReaction>().AsQueryable().BuildMockDbSet();

            mockArticleReactionRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<ArticleReaction, bool>>>()))
                .Returns(emptyReactionList.Object);

            mockArticleReactionRepository
                .Setup(repo => repo.AddAsync(It.IsAny<ArticleReaction>()))
                .ReturnsAsync(1);

            var result = await articleService.ReactToArticleAsync(articleId, userId, reactionId);

            Assert.AreEqual(1, result);

            mockArticleReactionRepository.Verify(repo => repo.AddAsync(It.Is<ArticleReaction>(r =>
                r.ArticleId == articleId &&
                r.UserId == userId &&
                r.ReactionId == reactionId &&
                r.CreatedById == userId
            )), Times.Once);
        }

        [Test]
        public void ReactArticle_WhenArticleDoesNotExist_ThrowsNotFoundException()
        {
            int articleId = 1;
            int userId = 123;
            int reactionId = 2;

            var emptyArticleList = new List<Article>().AsQueryable().BuildMockDbSet();

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(emptyArticleList.Object);

            Assert.ThrowsAsync<NotFoundException>(() => articleService.ReactToArticleAsync(articleId, userId, reactionId));
            mockArticleReactionRepository.Verify(repo => repo.AddAsync(It.IsAny<ArticleReaction>()), Times.Never);
        }


        [Test]
        public async Task ToggleVisibility_WhenArticleExistsAndIsHiddenChanged_UpdatesArticle()
        {
            int articleId = 1;
            int userId = 101;
            bool newIsHidden = true;

            var existingArticle = new Article
            {
                Id = articleId,
                IsHidden = false
            };

            var articleList = new List<Article> { existingArticle }.AsQueryable().BuildMockDbSet();

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.Is<Expression<Func<Article, bool>>>(expr => expr.Compile().Invoke(existingArticle))))
                .Returns(articleList.Object);

            mockArticleRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<Article>()))
                .ReturnsAsync(1);

            var result = await articleService.ToggleArticleVisibilityAsync(articleId, userId, newIsHidden);

            Assert.AreEqual(1, result);

            mockArticleRepository.Verify(repo => repo.UpdateAsync(It.Is<Article>(a =>
                a.Id == articleId &&
                a.IsHidden == newIsHidden &&
                a.ModifiedById == userId &&
                a.ModifiedDate != default
            )), Times.Once);
        }

        [Test]
        public void ToggleVisibility_WhenArticleDoesNotExist_ThrowsNotFoundException()
        {
            int articleId = 1;
            int userId = 101;
            bool newIsHidden = true;

            var emptyArticleList = new List<Article>().AsQueryable().BuildMockDbSet();

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(emptyArticleList.Object);

            Assert.ThrowsAsync<NotFoundException>(() => articleService.ToggleArticleVisibilityAsync(articleId, userId, newIsHidden));

            mockArticleRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Article>()), Times.Never);
        }

        [Test]
        public async Task ToggleVisibility_WhenIsHiddenIsSame_DoesNotUpdate_ReturnsZero()
        {
            int articleId = 1;
            int userId = 101;
            bool newIsHidden = true;

            var existingArticle = new Article
            {
                Id = articleId,
                IsHidden = true
            };

            var articleList = new List<Article> { existingArticle }.AsQueryable().BuildMockDbSet();

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(articleList.Object);

            var result = await articleService.ToggleArticleVisibilityAsync(articleId, userId, newIsHidden);

            Assert.AreEqual(0, result);
            mockArticleRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Article>()), Times.Never);
        }


        [Test]
        public async Task SaveArticle_WhenArticleExistsAndNotAlreadySaved_SavesArticle()
        {
            int articleId = 1;
            int userId = 123;

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(new List<Article> { new Article { Id = articleId } }.AsQueryable().BuildMockDbSet().Object);

            mockSavedArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<SavedArticle, bool>>>()))
                .Returns(new List<SavedArticle>().AsQueryable().BuildMockDbSet().Object);

            mockSavedArticleRepository
                .Setup(repo => repo.AddAsync(It.IsAny<SavedArticle>()))
                .ReturnsAsync(1);

            var result = await articleService.SaveUserArticleAsync(articleId, userId);

            Assert.AreEqual(1, result);

            mockSavedArticleRepository.Verify(repo => repo.AddAsync(It.Is<SavedArticle>(s =>
                s.ArticleId == articleId &&
                s.UserId == userId &&
                s.CreatedById == userId &&
                s.CreatedDate != default
            )), Times.Once);
        }

        [Test]
        public void SaveArticle_WhenArticleDoesNotExist_ThrowsNotFoundException()
        {
            int articleId = 1;
            int userId = 123;

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(new List<Article>().AsQueryable().BuildMockDbSet().Object);

            Assert.ThrowsAsync<NotFoundException>(() => articleService.SaveUserArticleAsync(articleId, userId));

            mockSavedArticleRepository.Verify(repo => repo.AddAsync(It.IsAny<SavedArticle>()), Times.Never);
        }

        [Test]
        public async Task SaveArticle_WhenAlreadySaved_DoesNotSaveAgain_ReturnsZero()
        {
            int articleId = 1;
            int userId = 123;

            var article = new Article { Id = articleId };
            var savedArticle = new SavedArticle { ArticleId = articleId, UserId = userId };

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(new List<Article> { article }.AsQueryable().BuildMockDbSet().Object);

            mockSavedArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<SavedArticle, bool>>>()))
                .Returns(new List<SavedArticle> { savedArticle }.AsQueryable().BuildMockDbSet().Object);

            var result = await articleService.SaveUserArticleAsync(articleId, userId);

            Assert.AreEqual(0, result);
            mockSavedArticleRepository.Verify(repo => repo.AddAsync(It.IsAny<SavedArticle>()), Times.Never);
        }


        [Test]
        public async Task IsNewsArticleExist_WhenArticleExists_ReturnsTrue()
        {
            int articleId = 1;
            var articles = new List<Article>
            {
                new Article { Id = articleId }
            }.AsQueryable();

            var mockArticleDbSet = articles.BuildMockDbSet();

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(mockArticleDbSet.Object);

            var result = await articleService.IsNewsArticleExist(articleId);

            Assert.IsTrue(result);
        }

        [Test]
        public async Task IsNewsArticleExist_WhenArticleDoesNotExist_ReturnsFalse()
        {
            int articleId = 1;
            var emptyArticles = new List<Article>().AsQueryable();

            var mockArticleDbSet = emptyArticles.BuildMockDbSet();

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(mockArticleDbSet.Object);

            var result = await articleService.IsNewsArticleExist(articleId);
            Assert.IsFalse(result);
        }

        [Test]
        public async Task IsNewsArticleExist_WhenArticleIsDeleted_ReturnsFalse()
        {
            int articleId = 1;
            var deletedArticles = new List<Article>
            {
                new Article { Id = articleId}
            }.AsQueryable();

            var mockArticleDbSet = deletedArticles.BuildMockDbSet();

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(mockArticleDbSet.Object);

            var result = await articleService.IsNewsArticleExist(articleId);
            Assert.IsTrue(result);
        }

        [Test]
        public async Task GetRecommendedArticles_ReturnsExpectedArticles()
        {
            int userId = 123;

            var articles = new List<Article>
            {
                new Article { Id = 1, NewsCategoryId = 10, Title = "Tech AI" },
                new Article { Id = 2, NewsCategoryId = 20, Title = "Health Tips" }
            };

            var likedCategory = new CategoryRecommendationDTO { CategoryId = 10, Count = 5 };
            var savedCategory = new CategoryRecommendationDTO { CategoryId = 20, Count = 3 };

            var notificationPreferences = new List<NotificationPreferenceDTO>
            {
                new NotificationPreferenceDTO
                {
                    UserId = userId,
                    NewsCategories = new List<NewsCategoryDTO>
                    {
                        new NewsCategoryDTO
                        {
                            CategoryId = 10,
                            IsEnabled = true,
                            Keywords = new List<NotificationPreferencesKeywordDTO>
                            {
                                new NotificationPreferencesKeywordDTO { Name = "AI", IsEnabled = true }
                            }
                        }
                    }
                }
            };

            mockArticleRepository.Setup(x => x.GetMostLikedCategory(userId)).ReturnsAsync(likedCategory);
            mockArticleRepository.Setup(x => x.GetMostSavedCategory(userId)).ReturnsAsync(savedCategory);
            mockArticleRepository.Setup(x => x.GetMostReadCategory(userId)).ReturnsAsync((CategoryRecommendationDTO)null!);

            mockNotificationPreferenceService
                .Setup(x => x.GetUserNotificationPreferencesAsync(It.Is<List<int>>(ids => ids.Contains(userId))))
                .ReturnsAsync(notificationPreferences);

            var result = await mockArticleService.Object.GetRecommendedArticles(userId, articles);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(a => a.Id == 1));
            Assert.IsTrue(result.Any(a => a.Id == 2));
        }

        [Test]
        public async Task GetRecommendedArticles_WhenNoMatchingCategoriesOrKeywords_ReturnsEmptyList()
        {
            int userId = 123;

            var articles = new List<Article>
            {
                new Article { Id = 1, NewsCategoryId = 30, Title = "Politics Today" },
                new Article { Id = 2, NewsCategoryId = 40, Title = "Global Warming" }
            };

            var likedCategory = new CategoryRecommendationDTO { CategoryId = 10, Count = 5 };
            var savedCategory = new CategoryRecommendationDTO { CategoryId = 20, Count = 3 };

            var notificationPreferences = new List<NotificationPreferenceDTO>
            {
                new NotificationPreferenceDTO
                {
                    UserId = userId,
                    NewsCategories = new List<NewsCategoryDTO>
                    {
                        new NewsCategoryDTO
                        {
                            CategoryId = 50,
                            IsEnabled = true,
                            Keywords = new List<NotificationPreferencesKeywordDTO>
                            {
                                new NotificationPreferencesKeywordDTO { Name = "Space", IsEnabled = true }
                            }
                        }
                    }
                }
            };

            mockArticleRepository.Setup(x => x.GetMostLikedCategory(userId)).ReturnsAsync(likedCategory);
            mockArticleRepository.Setup(x => x.GetMostSavedCategory(userId)).ReturnsAsync(savedCategory);
            mockArticleRepository.Setup(x => x.GetMostReadCategory(userId)).ReturnsAsync((CategoryRecommendationDTO)null!);

            mockNotificationPreferenceService
                .Setup(x => x.GetUserNotificationPreferencesAsync(It.Is<List<int>>(ids => ids.Contains(userId))))
                .ReturnsAsync(notificationPreferences);

            var result = await mockArticleService.Object.GetRecommendedArticles(userId, articles);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public async Task GetRecommendedArticles_WhenNotificationPreferencesDisabled_ReturnsEmptyList()
        {
            int userId = 123;
            var articles = new List<Article>
            {
                new Article { Id = 1, NewsCategoryId = 10, Title = "Tech AI" }
            };

            var likedCategory = new CategoryRecommendationDTO { CategoryId = 10, Count = 5 };

            var notificationPreferences = new List<NotificationPreferenceDTO>
            {
                new NotificationPreferenceDTO
                {
                    UserId = userId,
                    NewsCategories = new List<NewsCategoryDTO>
                    {
                        new NewsCategoryDTO
                        {
                            CategoryId = 10,
                            IsEnabled = false,
                            Keywords = new List<NotificationPreferencesKeywordDTO>
                            {
                                new NotificationPreferencesKeywordDTO { Name = "AI", IsEnabled = true }
                            }
                        }
                    }
                }
            };

            mockArticleRepository.Setup(x => x.GetMostLikedCategory(userId)).ReturnsAsync(likedCategory);
            mockArticleRepository.Setup(x => x.GetMostSavedCategory(userId)).ReturnsAsync((CategoryRecommendationDTO)null!);
            mockArticleRepository.Setup(x => x.GetMostReadCategory(userId)).ReturnsAsync((CategoryRecommendationDTO)null!);

            mockNotificationPreferenceService
                .Setup(x => x.GetUserNotificationPreferencesAsync(It.Is<List<int>>(ids => ids.Contains(userId))))
                .ReturnsAsync(notificationPreferences);

            var result = await mockArticleService.Object.GetRecommendedArticles(userId, articles);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public async Task GetAllArticles_WhenSearchTextIsProvided_FiltersBySearchText()
        {
            var userId = 123;
            var searchText = "ai";

            var request = new NewsArticleRequestDTO
            {
                SearchText = searchText
            };

            var articles = new List<Article>
            {
                new Article
                {
                    Id = 1,
                    Title = "AI and Future",
                    Content = "AI is transforming the world",
                    Description = "AI description",
                    IsHidden = false,
                    NewsCategory = new NewsCategory { IsHidden = false }
                }
            };

            var hiddenKeywords = new List<HiddenArticleKeyword>
            {
                new HiddenArticleKeyword
                {
                    Name = "block"
                }
            };

            var mockArticleDbSet = articles.AsQueryable().BuildMockDbSet();
            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(mockArticleDbSet.Object);

            var mockHiddenKeywordDbSet = hiddenKeywords.AsQueryable().BuildMockDbSet();
            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.GetAll())
                .Returns(mockHiddenKeywordDbSet.Object);

            mockMapper
                .Setup(m => m.Map<List<ArticleDTO>>(It.IsAny<List<Article>>()))
                .Returns(new List<ArticleDTO>
                {
                    new ArticleDTO { Id = 1, Title = "AI and Future" }
                });

            var result = await articleService.GetUserArticlesAsync(request, userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("AI and Future", result[0].Title);
        }

        [Test]
        public async Task GetAllArticles_WhenRequestedForToday_FiltersByToday()
        {
            var userId = 123;
            var today = DateTime.UtcNow.Date;

            var request = new NewsArticleRequestDTO
            {
                IsRequestedForToday = true
            };

            var articles = new List<Article>
            {
                new Article
                {
                    Id = 2,
                    PublishedAt = today,
                    IsHidden = false,
                    NewsCategory = new NewsCategory { Id = 10, IsHidden = false },
                    ArticleReactions = new List<ArticleReaction>(),
                    SavedArticles = new List<SavedArticle>(),
                    ReportedArticles = new List<ReportedArticle>()
                }
            };

            var hiddenKeywords = new List<HiddenArticleKeyword>
            {
                new HiddenArticleKeyword { Name = "block"}
            };

            var notificationPreferences = new List<NotificationPreferenceDTO>
            {
                new NotificationPreferenceDTO
                {
                    UserId = userId,
                    NewsCategories = new List<NewsCategoryDTO>
                    {
                        new NewsCategoryDTO
                        {
                            CategoryId = 10,
                            IsEnabled = true,
                            Keywords = new List<NotificationPreferencesKeywordDTO>()
                        }
                    }
                }
            };

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(articles.AsQueryable().BuildMockDbSet().Object);

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.GetAll())
                .Returns(hiddenKeywords.AsQueryable().BuildMockDbSet().Object);

            mockNotificationPreferenceService
                .Setup(x => x.GetUserNotificationPreferencesAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(notificationPreferences);

            mockArticleRepository
                .Setup(x => x.GetMostLikedCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockArticleRepository
                .Setup(x => x.GetMostSavedCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockArticleRepository
                .Setup(x => x.GetMostReadCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockMapper
                .Setup(m => m.Map<List<ArticleDTO>>(It.IsAny<List<Article>>()))
                .Returns(new List<ArticleDTO> { new ArticleDTO { Id = 2, Title = "AI Today" } });

            var result = await articleService.GetUserArticlesAsync(request, userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result[0].Id);
        }

        [Test]
        public async Task GetAllArticles_WhenFromDateAndToDateProvided_FiltersByDateRange()
        {
            var userId = 123;
            var fromDate = DateTime.UtcNow.Date.AddDays(-3);
            var toDate = DateTime.UtcNow.Date;

            var request = new NewsArticleRequestDTO
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            var articles = new List<Article>
            {
                new Article
                {
                    Id = 5,
                    PublishedAt = DateTime.UtcNow.Date,
                    IsHidden = false,
                    NewsCategory = new NewsCategory { Id = 20, IsHidden = false },
                    ArticleReactions = new List<ArticleReaction>(),
                    SavedArticles = new List<SavedArticle>(),
                    ReportedArticles = new List<ReportedArticle>()
                }
            };

            var hiddenKeywords = new List<HiddenArticleKeyword>
            {
                new HiddenArticleKeyword { Name = "block"}
            };

            var notificationPreferences = new List<NotificationPreferenceDTO>
            {
                new NotificationPreferenceDTO
                {
                    UserId = userId,
                    NewsCategories = new List<NewsCategoryDTO>
                    {
                        new NewsCategoryDTO
                        {
                            CategoryId = 20,
                            IsEnabled = true,
                            Keywords = new List<NotificationPreferencesKeywordDTO>()
                        }
                    }
                }
            };

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(articles.AsQueryable().BuildMockDbSet().Object);

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.GetAll())
                .Returns(hiddenKeywords.AsQueryable().BuildMockDbSet().Object);

            mockNotificationPreferenceService
                .Setup(x => x.GetUserNotificationPreferencesAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(notificationPreferences);

            mockArticleRepository
                .Setup(x => x.GetMostLikedCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockArticleRepository
                .Setup(x => x.GetMostSavedCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockArticleRepository
                .Setup(x => x.GetMostReadCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockMapper
                .Setup(m => m.Map<List<ArticleDTO>>(It.IsAny<List<Article>>()))
                .Returns(new List<ArticleDTO> { new ArticleDTO { Id = 5, Title = "Filtered News" } });

            var result = await articleService.GetUserArticlesAsync(request, userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(5, result[0].Id);
        }

        [Test]
        public async Task GetAllArticles_WhenSearchTextAndCategoryIdProvided_FiltersByBoth()
        {
            var userId = 123;
            var searchText = "climate";
            var categoryId = 30;

            var request = new NewsArticleRequestDTO
            {
                SearchText = searchText,
                CategoryId = categoryId
            };

            var articles = new List<Article>
    {
        new Article
        {
            Id = 10,
            Title = "Climate Change Updates",
            Description = "Latest climate report",
            Content = "Climate action needed",
            NewsCategoryId = categoryId,
            IsHidden = false,
            NewsCategory = new NewsCategory { Id = categoryId, IsHidden = false },
            ArticleReactions = new List<ArticleReaction>(),
            SavedArticles = new List<SavedArticle>(),
            ReportedArticles = new List<ReportedArticle>()
        }
    };

            var hiddenKeywords = new List<HiddenArticleKeyword>
            {
                new HiddenArticleKeyword { Name = "block"}
            };

            var notificationPreferences = new List<NotificationPreferenceDTO>
            {
                new NotificationPreferenceDTO
                {
                    UserId = userId,
                    NewsCategories = new List<NewsCategoryDTO>
                    {
                        new NewsCategoryDTO
                        {
                            CategoryId = categoryId,
                            IsEnabled = true,
                            Keywords = new List<NotificationPreferencesKeywordDTO>()
                        }
                    }
                }
            };

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(articles.AsQueryable().BuildMockDbSet().Object);

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.GetAll())
                .Returns(hiddenKeywords.AsQueryable().BuildMockDbSet().Object);

            mockNotificationPreferenceService
                .Setup(x => x.GetUserNotificationPreferencesAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(notificationPreferences);

            mockArticleRepository
                .Setup(x => x.GetMostLikedCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockArticleRepository
                .Setup(x => x.GetMostSavedCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockArticleRepository
                .Setup(x => x.GetMostReadCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockMapper
                .Setup(m => m.Map<List<ArticleDTO>>(It.IsAny<List<Article>>()))
                .Returns(new List<ArticleDTO> { new ArticleDTO { Id = 10, Title = "Climate Change Updates" } });

            var result = await articleService.GetUserArticlesAsync(request, userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(10, result[0].Id);
            Assert.AreEqual("Climate Change Updates", result[0].Title);
        }

        [Test]
        public async Task GetAllArticles_WhenNoArticlesMatchFilters_ReturnsEmptyList()
        {
            var userId = 123;

            var request = new NewsArticleRequestDTO
            {
                SearchText = "nonexistent keyword",
                CategoryId = 999
            };

            var emptyArticles = new List<Article>().AsQueryable();

            var hiddenKeywords = new List<HiddenArticleKeyword>
            {
                new HiddenArticleKeyword { Name = "block" }
            };

            var notificationPreferences = new List<NotificationPreferenceDTO>
            {
                new NotificationPreferenceDTO
                {
                    UserId = userId,
                    NewsCategories = new List<NewsCategoryDTO>()
                }
            };

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(emptyArticles.BuildMockDbSet().Object);

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.GetAll())
                .Returns(hiddenKeywords.AsQueryable().BuildMockDbSet().Object);

            mockNotificationPreferenceService
                .Setup(x => x.GetUserNotificationPreferencesAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(notificationPreferences);

            mockArticleRepository
                .Setup(x => x.GetMostLikedCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockArticleRepository
                .Setup(x => x.GetMostSavedCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockArticleRepository
                .Setup(x => x.GetMostReadCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockMapper
                .Setup(m => m.Map<List<ArticleDTO>>(It.IsAny<List<Article>>()))
                .Returns(new List<ArticleDTO>());

            var result = await articleService.GetUserArticlesAsync(request, userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public async Task GetAllArticles_WhenMatchingArticlesAreHidden_ReturnsEmptyList()
        {
            var userId = 123;
            var searchText = "climate";

            var request = new NewsArticleRequestDTO
            {
                SearchText = searchText
            };

            var articles = new List<Article>
            {
                new Article
                {
                    Id = 101,
                    Title = "Climate Change and Impact",
                    Content = "climate report",
                    IsHidden = true,
                    NewsCategory = new NewsCategory { Id = 10, IsHidden = false },
                    ArticleReactions = new List<ArticleReaction>(),
                    SavedArticles = new List<SavedArticle>(),
                    ReportedArticles = new List<ReportedArticle>()
                },
                new Article
                {
                    Id = 102,
                    Title = "Global Warming",
                    Content = "climate action",
                    IsHidden = false,
                    NewsCategory = new NewsCategory { Id = 11, IsHidden = true },
                    ArticleReactions = new List<ArticleReaction>(),
                    SavedArticles = new List<SavedArticle>(),
                    ReportedArticles = new List<ReportedArticle>()
                }
            };

            var hiddenKeywords = new List<HiddenArticleKeyword>();

            var notificationPreferences = new List<NotificationPreferenceDTO>
            {
                new NotificationPreferenceDTO
                {
                    UserId = userId,
                    NewsCategories = new List<NewsCategoryDTO>()
                }
            };

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(articles.AsQueryable().BuildMockDbSet().Object);

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.GetAll())
                .Returns(hiddenKeywords.AsQueryable().BuildMockDbSet().Object);

            mockNotificationPreferenceService
                .Setup(x => x.GetUserNotificationPreferencesAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(notificationPreferences);

            mockArticleRepository
                .Setup(x => x.GetMostLikedCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockArticleRepository
                .Setup(x => x.GetMostSavedCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockArticleRepository
                .Setup(x => x.GetMostReadCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockMapper
                .Setup(m => m.Map<List<ArticleDTO>>(It.IsAny<List<Article>>()))
                .Returns(new List<ArticleDTO>());

            var result = await articleService.GetUserArticlesAsync(request, userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public async Task GetAllArticles_WhenArticleContainsHiddenKeyword_ExcludesArticleFromResults()
        {
            var userId = 123;
            var request = new NewsArticleRequestDTO
            {
                SearchText = "politics"
            };

            var articles = new List<Article>
            {
                new Article
                {
                    Id = 1,
                    Title = "Politics in Modern Age",
                    Description = "Detailed view on politics",
                    Content = "Some political opinions",
                    IsHidden = false,
                    NewsCategory = new NewsCategory { Id = 1, IsHidden = false },
                    ArticleReactions = new List<ArticleReaction>(),
                    SavedArticles = new List<SavedArticle>(),
                    ReportedArticles = new List<ReportedArticle>()
                }
            };

            var hiddenKeywords = new List<HiddenArticleKeyword>
            {
                new HiddenArticleKeyword { Name = "politics" }
            };

            var notificationPreferences = new List<NotificationPreferenceDTO>
            {
                new NotificationPreferenceDTO
                {
                    UserId = userId,
                    NewsCategories = new List<NewsCategoryDTO>()
                }
            };

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(articles.AsQueryable().BuildMockDbSet().Object);

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.GetAll())
                .Returns(hiddenKeywords.AsQueryable().BuildMockDbSet().Object);

            mockNotificationPreferenceService
                .Setup(x => x.GetUserNotificationPreferencesAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(notificationPreferences);

            mockArticleRepository.Setup(x => x.GetMostLikedCategory(userId)).ReturnsAsync((CategoryRecommendationDTO)null!);
            mockArticleRepository.Setup(x => x.GetMostSavedCategory(userId)).ReturnsAsync((CategoryRecommendationDTO)null!);
            mockArticleRepository.Setup(x => x.GetMostReadCategory(userId)).ReturnsAsync((CategoryRecommendationDTO)null!);

            mockMapper
                .Setup(m => m.Map<List<ArticleDTO>>(It.IsAny<List<Article>>()))
                .Returns(new List<ArticleDTO>());


            var result = await articleService.GetUserArticlesAsync(request, userId);


            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public async Task ReactArticle_WhenExistingReactionExists_UpdatesReaction()
        {
            int articleId = 1;
            int userId = 123;
            int oldReactionId = 1;
            int newReactionId = 2;

            var articleList = new List<Article>
            {
                new Article { Id = articleId }
            }.AsQueryable().BuildMockDbSet();

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(articleList.Object);

            var existingReaction = new ArticleReaction
            {
                ArticleId = articleId,
                UserId = userId,
                ReactionId = oldReactionId
            };

            var reactionList = new List<ArticleReaction> { existingReaction }.AsQueryable().BuildMockDbSet();

            mockArticleReactionRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<ArticleReaction, bool>>>()))
                .Returns(reactionList.Object);

            mockArticleReactionRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<ArticleReaction>()))
                .ReturnsAsync(1);

            var result = await articleService.ReactToArticleAsync(articleId, userId, newReactionId);

            Assert.AreEqual(1, result);

            mockArticleReactionRepository.Verify(repo => repo.UpdateAsync(It.Is<ArticleReaction>(r =>
                r.ArticleId == articleId &&
                r.UserId == userId &&
                r.ReactionId == newReactionId
            )), Times.Once);

            mockArticleReactionRepository.Verify(repo => repo.AddAsync(It.IsAny<ArticleReaction>()), Times.Never);
        }

        [Test]
        public async Task ReactArticle_WhenExistingReactionHasSameReactionId_ReturnsZero()
        {
            int articleId = 1;
            int userId = 123;
            int reactionId = 1;

            var articleList = new List<Article>
            {
                new Article { Id = articleId }
            }.AsQueryable().BuildMockDbSet();

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(articleList.Object);

            var existingReaction = new ArticleReaction
            {
                ArticleId = articleId,
                UserId = userId,
                ReactionId = reactionId
            };

            var reactionList = new List<ArticleReaction> { existingReaction }.AsQueryable().BuildMockDbSet();

            mockArticleReactionRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<ArticleReaction, bool>>>()))
                .Returns(reactionList.Object);

            var result = await articleService.ReactToArticleAsync(articleId, userId, reactionId);

            Assert.AreEqual(0, result);

            mockArticleReactionRepository.Verify(repo => repo.UpdateAsync(It.IsAny<ArticleReaction>()), Times.Never);
            mockArticleReactionRepository.Verify(repo => repo.AddAsync(It.IsAny<ArticleReaction>()), Times.Never);
        }

        [Test]
        public async Task GetAllArticles_WhenCategoryIdProvided_FiltersAndRecommendsByCategory()
        {
            var userId = 123;
            var categoryId = 10;
            var request = new NewsArticleRequestDTO
            {
                CategoryId = categoryId
            };

            var articles = new List<Article>
            {
                new Article
                {
                    Id = 1,
                    Title = "Category Article",
                    NewsCategoryId = categoryId,
                    IsHidden = false,
                    NewsCategory = new NewsCategory { Id = categoryId, IsHidden = false },
                    ArticleReactions = new List<ArticleReaction>(),
                    SavedArticles = new List<SavedArticle>(),
                    ReportedArticles = new List<ReportedArticle>(),
                    ArticleReadHistory = new List<ArticleReadHistory>()
                },
                new Article
                {
                    Id = 2,
                    Title = "Other Category Article",
                    NewsCategoryId = 20,
                    IsHidden = false,
                    NewsCategory = new NewsCategory { Id = 20, IsHidden = false },
                    ArticleReactions = new List<ArticleReaction>(),
                    SavedArticles = new List<SavedArticle>(),
                    ReportedArticles = new List<ReportedArticle>(),
                    ArticleReadHistory = new List<ArticleReadHistory>()
                }
            };

            var hiddenKeywords = new List<HiddenArticleKeyword>();

            var notificationPreferences = new List<NotificationPreferenceDTO>
            {
                new NotificationPreferenceDTO
                {
                    UserId = userId,
                    NewsCategories = new List<NewsCategoryDTO>
                    {
                        new NewsCategoryDTO
                        {
                            CategoryId = categoryId,
                            IsEnabled = true,
                            Keywords = new List<NotificationPreferencesKeywordDTO>()
                        }
                    }
                }
            };

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(articles.AsQueryable().BuildMockDbSet().Object);

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.GetAll())
                .Returns(hiddenKeywords.AsQueryable().BuildMockDbSet().Object);

            mockNotificationPreferenceService
                .Setup(x => x.GetUserNotificationPreferencesAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(notificationPreferences);

            mockArticleRepository
                .Setup(x => x.GetMostLikedCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockArticleRepository
                .Setup(x => x.GetMostSavedCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockArticleRepository
                .Setup(x => x.GetMostReadCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockMapper
                .Setup(m => m.Map<List<ArticleDTO>>(It.IsAny<List<Article>>()))
                .Returns((List<Article> input) => input.Select(a => new ArticleDTO { Id = a.Id, Title = a.Title }).ToList());

            var result = await articleService.GetUserArticlesAsync(request, userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].Id);
        }

        [Test]
        public async Task GetAllArticles_WhenCategoryIdIsAll_ReturnsAllArticles()
        {
            var userId = 123;
            var request = new NewsArticleRequestDTO
            {
                CategoryId = (int)CategoryType.All
            };

            var articles = new List<Article>
            {
                new Article
                {
                    Id = 1,
                    Title = "Article 1",
                    IsHidden = false,
                    NewsCategory = new NewsCategory { Id = 3, Name = "Sport", IsHidden = false },
                    ArticleReactions = new List<ArticleReaction>(),
                    SavedArticles = new List<SavedArticle>(),
                    ReportedArticles = new List<ReportedArticle>()
                },
                new Article
                {
                    Id = 2,
                    Title = "Article 2",
                    IsHidden = false,
                    NewsCategory = new NewsCategory { Id = 3, Name = "Sport", IsHidden = false },
                    ArticleReactions = new List<ArticleReaction>(),
                    SavedArticles = new List<SavedArticle>(),
                    ReportedArticles = new List<ReportedArticle>()
                }
            };

            var hiddenKeywords = new List<HiddenArticleKeyword>();

            var notificationPreferences = new List<NotificationPreferenceDTO>
            {
                new NotificationPreferenceDTO
                {
                    UserId = userId,
                    NewsCategories = new List<NewsCategoryDTO> { new NewsCategoryDTO { CategoryId = 3, IsEnabled = true, Keywords = [] } }
                }
            };

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(articles.AsQueryable().BuildMockDbSet().Object);

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.GetAll())
                .Returns(hiddenKeywords.AsQueryable().BuildMockDbSet().Object);

            mockNotificationPreferenceService
                .Setup(x => x.GetUserNotificationPreferencesAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(notificationPreferences);

            mockArticleRepository
                .Setup(x => x.GetMostLikedCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockArticleRepository
                .Setup(x => x.GetMostSavedCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockArticleRepository
                .Setup(x => x.GetMostReadCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockMapper
                .Setup(m => m.Map<List<ArticleDTO>>(It.IsAny<List<Article>>()))
                .Returns((List<Article> input) => input.Select(a => new ArticleDTO { Id = a.Id, Title = a.Title }).ToList());

            var result = await articleService.GetUserArticlesAsync(request, userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public async Task GetAllArticles_WhenCategoryIdProvidedWithKeywords_PrioritizesByKeywordMatch()
        {
            var userId = 123;
            var categoryId = 10;
            var request = new NewsArticleRequestDTO
            {
                CategoryId = categoryId
            };

            var articles = new List<Article>
            {
                new Article
                {
                    Id = 1,
                    Title = "Article about AI",
                    Description = "AI technology",
                    Content = "Artificial Intelligence",
                    NewsCategoryId = categoryId,
                    IsHidden = false,
                    NewsCategory = new NewsCategory { Id = categoryId, IsHidden = false },
                    ArticleReactions = new List<ArticleReaction>(),
                    SavedArticles = new List<SavedArticle>(),
                    ReportedArticles = new List<ReportedArticle>(),
                    ArticleReadHistory = new List<ArticleReadHistory>()
                },
                new Article
                {
                    Id = 2,
                    Title = "Article about Technology",
                    Description = "Tech news",
                    Content = "Technology updates",
                    NewsCategoryId = categoryId,
                    IsHidden = false,
                    NewsCategory = new NewsCategory { Id = categoryId, IsHidden = false },
                    ArticleReactions = new List<ArticleReaction>(),
                    SavedArticles = new List<SavedArticle>(),
                    ReportedArticles = new List<ReportedArticle>(),
                    ArticleReadHistory = new List<ArticleReadHistory>()
                }
            };

            var hiddenKeywords = new List<HiddenArticleKeyword>();

            var notificationPreferences = new List<NotificationPreferenceDTO>
            {
                new NotificationPreferenceDTO
                {
                    UserId = userId,
                    NewsCategories = new List<NewsCategoryDTO>
                    {
                        new NewsCategoryDTO
                        {
                            CategoryId = categoryId,
                            IsEnabled = true,
                            Keywords = new List<NotificationPreferencesKeywordDTO>
                            {
                                new NotificationPreferencesKeywordDTO { Name = "AI", IsEnabled = true },
                                new NotificationPreferencesKeywordDTO { Name = "Technology", IsEnabled = true }
                            }
                        }
                    }
                }
            };

            mockArticleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<Article, bool>>>()))
                .Returns(articles.AsQueryable().BuildMockDbSet().Object);

            mockHiddenArticleKeywordRepository
                .Setup(repo => repo.GetAll())
                .Returns(hiddenKeywords.AsQueryable().BuildMockDbSet().Object);

            mockNotificationPreferenceService
                .Setup(x => x.GetUserNotificationPreferencesAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(notificationPreferences);

            mockArticleRepository
                .Setup(x => x.GetMostLikedCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockArticleRepository
                .Setup(x => x.GetMostSavedCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockArticleRepository
                .Setup(x => x.GetMostReadCategory(userId))
                .ReturnsAsync((CategoryRecommendationDTO)null!);

            mockMapper
                .Setup(m => m.Map<List<ArticleDTO>>(It.IsAny<List<Article>>()))
                .Returns((List<Article> input) => input.Select(a => new ArticleDTO { Id = a.Id, Title = a.Title }).ToList());

            var result = await articleService.GetUserArticlesAsync(request, userId);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public async Task AddIfNotNull_WhenCategoryRecommendationIsNotNull_AddsToRecommendations()
        {
            var userId = 123;
            var categoryRecommendation = new CategoryRecommendationDTO { CategoryId = 10, Count = 5 };
            var categoryRecommendations = new List<CategoryRecommendationDTO>();

            var task = Task.FromResult(categoryRecommendation);

            await articleService.AddIfNotNull(task, categoryRecommendations);

            Assert.AreEqual(1, categoryRecommendations.Count);
            Assert.AreEqual(10, categoryRecommendations[0].CategoryId);
            Assert.AreEqual(5, categoryRecommendations[0].Count);
        }

        [Test]
        public async Task AddIfNotNull_WhenCategoryRecommendationIsNull_DoesNotAddToRecommendations()
        {
            var userId = 123;
            var categoryRecommendations = new List<CategoryRecommendationDTO>();

            var task = Task.FromResult<CategoryRecommendationDTO>(null!);

            await articleService.AddIfNotNull(task, categoryRecommendations);

            Assert.AreEqual(0, categoryRecommendations.Count);
        }

        [Test]
        public async Task GetRecommendedArticles_WhenNoRecommendationsExist_ReturnsEmptyList()
        {
            var userId = 123;
            var articles = new List<Article>
            {
                new Article { Id = 1, Title = "Test Article" }
            };

            var notificationPreferences = new List<NotificationPreferenceDTO>
            {
                new NotificationPreferenceDTO
                {
                    UserId = userId,
                    NewsCategories = new List<NewsCategoryDTO>()
                }
            };

            mockArticleRepository.Setup(x => x.GetMostLikedCategory(userId)).ReturnsAsync((CategoryRecommendationDTO)null!);
            mockArticleRepository.Setup(x => x.GetMostSavedCategory(userId)).ReturnsAsync((CategoryRecommendationDTO)null!);
            mockArticleRepository.Setup(x => x.GetMostReadCategory(userId)).ReturnsAsync((CategoryRecommendationDTO)null!);

            mockNotificationPreferenceService
                .Setup(x => x.GetUserNotificationPreferencesAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(notificationPreferences);

            var result = await articleService.GetRecommendedArticles(userId, articles);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public async Task GetRecommendedArticles_WhenRecommendationsExist_OrdersByCountDescending()
        {
            var userId = 123;
            var articles = new List<Article>
            {
                new Article { Id = 1, NewsCategoryId = 10, Title = "Category 10 Article" },
                new Article { Id = 2, NewsCategoryId = 20, Title = "Category 20 Article" }
            };

            var likedCategory = new CategoryRecommendationDTO { CategoryId = 10, Count = 3 };
            var savedCategory = new CategoryRecommendationDTO { CategoryId = 20, Count = 5 };

            var notificationPreferences = new List<NotificationPreferenceDTO>
            {
                new NotificationPreferenceDTO
                {
                    UserId = userId,
                    NewsCategories = new List<NewsCategoryDTO>
                    {
                        new NewsCategoryDTO
                        {
                            CategoryId = 10,
                            IsEnabled = true,
                            Keywords = new List<NotificationPreferencesKeywordDTO>()
                        },
                        new NewsCategoryDTO
                        {
                            CategoryId = 20,
                            IsEnabled = true,
                            Keywords = new List<NotificationPreferencesKeywordDTO>()
                        }
                    }
                }
            };

            mockArticleRepository.Setup(x => x.GetMostLikedCategory(userId)).ReturnsAsync(likedCategory);
            mockArticleRepository.Setup(x => x.GetMostSavedCategory(userId)).ReturnsAsync(savedCategory);
            mockArticleRepository.Setup(x => x.GetMostReadCategory(userId)).ReturnsAsync((CategoryRecommendationDTO)null!);

            mockNotificationPreferenceService
                .Setup(x => x.GetUserNotificationPreferencesAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(notificationPreferences);

            var result = await articleService.GetRecommendedArticles(userId, articles);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(2, result[0].Id);
            Assert.AreEqual(1, result[1].Id);
        }

        [Test]
        public async Task GetRecommendedArticles_WhenDuplicateCategoriesExist_RemovesDuplicates()
        {
            var userId = 123;
            var articles = new List<Article>
            {
                new Article { Id = 1, NewsCategoryId = 10, Title = "Category 10 Article" }
            };

            var likedCategory = new CategoryRecommendationDTO { CategoryId = 10, Count = 5 };
            var savedCategory = new CategoryRecommendationDTO { CategoryId = 10, Count = 3 };
            var notificationPreferences = new List<NotificationPreferenceDTO>
            {
                new NotificationPreferenceDTO
                {
                    UserId = userId,
                    NewsCategories = new List<NewsCategoryDTO>
                    {
                        new NewsCategoryDTO
                        {
                            CategoryId = 10,
                            IsEnabled = true,
                            Keywords = new List<NotificationPreferencesKeywordDTO>()
                        }
                    }
                }
            };

            mockArticleRepository.Setup(x => x.GetMostLikedCategory(userId)).ReturnsAsync(likedCategory);
            mockArticleRepository.Setup(x => x.GetMostSavedCategory(userId)).ReturnsAsync(savedCategory);
            mockArticleRepository.Setup(x => x.GetMostReadCategory(userId)).ReturnsAsync((CategoryRecommendationDTO)null!);

            mockNotificationPreferenceService
                .Setup(x => x.GetUserNotificationPreferencesAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(notificationPreferences);

            var result = await articleService.GetRecommendedArticles(userId, articles);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public async Task GetRecommendedArticles_WhenNotificationPreferencesHaveKeywords_AddsKeywordsToRecommendations()
        {
            var userId = 123;
            var articles = new List<Article>
            {
                new Article { Id = 1, NewsCategoryId = 10, Title = "AI Article" }
            };

            var likedCategory = new CategoryRecommendationDTO { CategoryId = 10, Count = 5 };

            var notificationPreferences = new List<NotificationPreferenceDTO>
            {
                new NotificationPreferenceDTO
                {
                    UserId = userId,
                    NewsCategories = new List<NewsCategoryDTO>
                    {
                        new NewsCategoryDTO
                        {
                            CategoryId = 10,
                            IsEnabled = true,
                            Keywords = new List<NotificationPreferencesKeywordDTO>
                            {
                                new NotificationPreferencesKeywordDTO { Name = "AI", IsEnabled = true },
                                new NotificationPreferencesKeywordDTO { Name = "Technology", IsEnabled = true }
                            }
                        }
                    }
                }
            };

            mockArticleRepository.Setup(x => x.GetMostLikedCategory(userId)).ReturnsAsync(likedCategory);
            mockArticleRepository.Setup(x => x.GetMostSavedCategory(userId)).ReturnsAsync((CategoryRecommendationDTO)null!);
            mockArticleRepository.Setup(x => x.GetMostReadCategory(userId)).ReturnsAsync((CategoryRecommendationDTO)null!);

            mockNotificationPreferenceService
                .Setup(x => x.GetUserNotificationPreferencesAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(notificationPreferences);

            var result = await articleService.GetRecommendedArticles(userId, articles);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        #region Private Helper Methods
        private Article CreateVisibleArticle(int id)
        {
            return new Article
            {
                Id = id,
                IsHidden = false,
                Title = "Sample Article"
            };
        }

        private Article CreateTestArticle(int id)
        {
            return new Article
            {
                Id = id,
                Title = "Test Title",
                Description = "Test Description",
                Content = "Test Content",
                NewsCategory = new NewsCategory()
            };
        }

        private ArticleDTO CreateExpectedArticleDTO(int id)
        {
            return new ArticleDTO
            {
                Id = id,
                Title = "Test Title",
                Description = "Test Description",
                Content = "Test Content"
            };
        }

        private void AssertArticleDTOProperties(ArticleDTO actual, ArticleDTO expected)
        {
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Title, actual.Title);
            Assert.AreEqual(expected.Description, actual.Description);
            Assert.AreEqual(expected.Content, actual.Content);
        }

        private void VerifyReadHistoryWasAdded(int articleId, int userId)
        {
            mockArticleReadHistoryRepository.Verify(repo => repo.AddAsync(
                It.Is<ArticleReadHistory>(h =>
                    h.ArticleId == articleId &&
                    h.UserId == userId &&
                    h.CreatedById == userId)
            ), Times.Once);
        }
        #endregion
    }
}
