using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsAggregationSystem.API.Scheduler;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Service.Interfaces;

namespace NewsAggregationSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : BaseController
    {
        private readonly IArticleService articleService;
        private readonly NewsFetchScheduler newsFetchScheduler;
        private readonly ILogger<ArticlesController> logger;

        public ArticlesController(IArticleService articleService, NewsFetchScheduler newsFetchScheduler, ILogger<ArticlesController> logger)
        {
            this.articleService = articleService;
            this.newsFetchScheduler = newsFetchScheduler;
            this.logger = logger;
        }

        [HttpGet, Authorize(Roles = ApplicationConstants.UserOnly)]
        public async Task<IActionResult> GetUserArticles([FromQuery] NewsArticleRequestDTO newsArticleRequestDTO)
        {
            logger.LogInformation(ApplicationConstants.LogMessage.FetchingArticles, LoggedInUserId);
            try
            {
                var result = await articleService.GetUserArticlesAsync(newsArticleRequestDTO, LoggedInUserId);
                logger.LogInformation("Fetched {Count} articles for user ID: {UserId}", result.Count, LoggedInUserId);
                return Ok(result);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpDelete("delete-saved-article/{Id:int}"), Authorize(Roles = ApplicationConstants.UserOnly)]
        public async Task<IActionResult> DeleteUserSavedArticle(int Id)
        {
            logger.LogInformation(ApplicationConstants.LogMessage.DeletingSavedArticle, Id, LoggedInUserId);
            try
            {
                var result = await articleService.DeleteUserSavedArticleAsync(Id, LoggedInUserId);
                if (result == 0)
                    return BadRequest(string.Format(ApplicationConstants.ArticleIsInUnsavedState, Id));

                logger.LogInformation(ApplicationConstants.LogMessage.ArticleDeletedSuccessfully, Id, LoggedInUserId);
                return Ok();
            }
            catch (NotFoundException exception)
            {
                logger.LogError(exception, exception.Message);
                return NotFound();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpGet("saved-articles"), Authorize(Roles = ApplicationConstants.UserOnly)]
        public async Task<IActionResult> GetUserSavedArticles()
        {
            logger.LogInformation(ApplicationConstants.LogMessage.FetchingSavedArticles, LoggedInUserId);
            try
            {
                var result = await articleService.GetUserSavedArticlesAsync(LoggedInUserId);
                logger.LogInformation("Fetched {Count} saved articles for user ID: {UserId}", result.Count, LoggedInUserId);
                return Ok(result);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpPost("save-article/{Id:int:min(1)}"), Authorize(Roles = ApplicationConstants.UserOnly)]
        public async Task<IActionResult> SaveUserArticle(int Id)
        {
            logger.LogInformation(ApplicationConstants.LogMessage.SavingArticle, Id, LoggedInUserId);
            try
            {
                var result = await articleService.SaveUserArticleAsync(Id, LoggedInUserId);
                if (result == 0)
                    return BadRequest(string.Format(ApplicationConstants.ArticleIsAlreadySaved, Id));

                logger.LogInformation(ApplicationConstants.LogMessage.ArticleSavedSuccessfully, Id, LoggedInUserId);
                return Ok();
            }
            catch (NotFoundException exception)
            {
                logger.LogError(exception, exception.Message);
                return NotFound();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpGet("{Id}"), Authorize(Roles = ApplicationConstants.UserOnly)]
        public async Task<IActionResult> GetUserArticleById(int Id)
        {
            logger.LogInformation(ApplicationConstants.LogMessage.FetchingArticleById, Id, LoggedInUserId);
            try
            {
                var article = await articleService.GetUserArticleByIdAsync(Id, LoggedInUserId);
                if (article == null)
                {
                    logger.LogWarning(ApplicationConstants.LogMessage.ArticleNotFound, Id, LoggedInUserId);
                    return NotFound();
                }

                logger.LogInformation(ApplicationConstants.LogMessage.ArticleFetchedSuccessfully, Id, LoggedInUserId);
                return Ok(article);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpPost("{Id:int}/react-article{reactionId:int}"), Authorize(Roles = ApplicationConstants.UserOnly)]
        public async Task<IActionResult> ReactToArticle(int Id, int reactionId)
        {
            logger.LogInformation(ApplicationConstants.LogMessage.ReactingToArticle, LoggedInUserId, Id, reactionId);
            try
            {
                var result = await articleService.ReactToArticleAsync(Id, LoggedInUserId, reactionId);
                logger.LogInformation(ApplicationConstants.LogMessage.ArticleReactedSuccessfully, Id, LoggedInUserId);
                return result == 0 ? BadRequest(string.Format(ApplicationConstants.ArticleReactionAlreadyPresentMessage, Id)) : Ok();
            }
            catch (NotFoundException exception)
            {
                logger.LogError(exception, exception.Message);
                return NotFound();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpPost("change-status"), Authorize(Roles = ApplicationConstants.AdminOnly)]
        public async Task<IActionResult> ToggleArticleVisibility([FromQuery] int articleId, [FromQuery] bool IsHidden)
        {
            logger.LogInformation(ApplicationConstants.LogMessage.TogglingArticleVisibility, LoggedInUserId, articleId, IsHidden);
            try
            {
                var result = await articleService.ToggleArticleVisibilityAsync(articleId, LoggedInUserId, IsHidden);
                if (result == 0)
                    return BadRequest(string.Format(ApplicationConstants.ArticleIsAlreadyHiddenMessage, articleId));

                logger.LogInformation(ApplicationConstants.LogMessage.ArticleVisibilityToggled, articleId, LoggedInUserId);
                return Ok();
            }
            catch (NotFoundException exception)
            {
                logger.LogError(exception, exception.Message);
                return NotFound();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            await newsFetchScheduler.ExecuteAsync();
            return Ok();
        }
    }
}
