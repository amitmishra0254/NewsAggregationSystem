using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Service.Interfaces;

namespace NewsAggregationSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationPreferencesController : BaseController
    {
        private readonly INotificationPreferenceService notificationPreferenceService;
        private readonly ILogger<NotificationPreferencesController> logger;

        public NotificationPreferencesController(INotificationPreferenceService notificationPreferenceService, ILogger<NotificationPreferencesController> logger)
        {
            this.notificationPreferenceService = notificationPreferenceService;
            this.logger = logger;
        }

        [HttpPost("add-keyword"), Authorize(Roles = ApplicationConstants.UserOnly)]
        public async Task<IActionResult> AddKeywordToCategory([FromQuery] string keyword, [FromQuery] int categoryId)
        {
            logger.LogInformation(ApplicationConstants.LogMessage.AddingKeywordToCategory, LoggedInUserId, keyword, categoryId);
            try
            {
                var result = await notificationPreferenceService.AddKeywordToCategoryAsync(keyword, categoryId, LoggedInUserId);
                logger.LogInformation(ApplicationConstants.LogMessage.KeywordAddedToCategory, keyword, categoryId, LoggedInUserId);
                return Ok(result);
            }
            catch (AlreadyExistException exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status409Conflict, exception.Message);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpPost("change-keyword-status"), Authorize(Roles = ApplicationConstants.UserOnly)]
        public async Task<IActionResult> UpdateKeywordStatus([FromQuery] int keywordId, [FromQuery] bool isEnable)
        {
            logger.LogInformation(ApplicationConstants.LogMessage.ChangingKeywordStatus, LoggedInUserId, keywordId, isEnable);
            try
            {
                var result = await notificationPreferenceService.UpdateKeywordStatusAsync(keywordId, isEnable);
                logger.LogInformation(ApplicationConstants.LogMessage.KeywordStatusUpdated, keywordId, isEnable);
                return Ok(result);
            }
            catch (NotFoundException exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpPost("change-category-status"), Authorize(Roles = ApplicationConstants.UserOnly)]
        public async Task<IActionResult> UpdateCategoryStatus([FromQuery] int categoryId, [FromQuery] bool isEnable)
        {
            logger.LogInformation(ApplicationConstants.LogMessage.ChangingCategoryStatus, LoggedInUserId, categoryId, isEnable);
            try
            {
                var result = await notificationPreferenceService.UpdateCategoryStatusAsync(categoryId, isEnable, LoggedInUserId);
                logger.LogInformation(ApplicationConstants.LogMessage.CategoryStatusUpdated, categoryId, isEnable, LoggedInUserId);
                return Ok(result);
            }
            catch (NotFoundException exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }
    }
}
