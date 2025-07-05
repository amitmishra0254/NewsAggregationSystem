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
        public async Task<IActionResult> AddKeyword([FromQuery] string keyword, [FromQuery] int categoryId)
        {
            logger.LogInformation("User {UserId} is attempting to add keyword '{Keyword}' to category ID {CategoryId}.", LoggedInUserId, keyword, categoryId);
            try
            {
                var result = await notificationPreferenceService.AddKeyword(keyword, categoryId, LoggedInUserId);
                logger.LogInformation("Keyword '{Keyword}' added successfully for category ID {CategoryId} by user {UserId}.", keyword, categoryId, LoggedInUserId);
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
        public async Task<IActionResult> ChangeKeywordStatus([FromQuery] int keywordId, [FromQuery] bool isEnable)
        {
            logger.LogInformation("User {UserId} is attempting to change keyword status. Keyword ID: {KeywordId}, Enable: {IsEnable}.", LoggedInUserId, keywordId, isEnable);
            try
            {
                var result = await notificationPreferenceService.ChangeKeywordStatus(keywordId, isEnable);
                logger.LogInformation("Keyword status updated successfully. Keyword ID: {KeywordId}, Enable: {IsEnable}.", keywordId, isEnable);
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
        public async Task<IActionResult> ChangeCategoryStatus([FromQuery] int categoryId, [FromQuery] bool isEnable)
        {
            logger.LogInformation("User {UserId} is attempting to change category status. Category ID: {CategoryId}, Enable: {IsEnable}.", LoggedInUserId, categoryId, isEnable);
            try
            {
                var result = await notificationPreferenceService.ChangeCategoryStatus(categoryId, isEnable, LoggedInUserId);
                logger.LogInformation("Category status updated successfully. Category ID: {CategoryId}, Enable: {IsEnable}, User ID: {UserId}.", categoryId, isEnable, LoggedInUserId);
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
