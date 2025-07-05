using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Service.Interfaces;

namespace NewsAggregationSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : BaseController
    {
        private readonly INotificationService notificationService;
        private readonly INotificationPreferenceService notificationPreferenceService;
        private readonly ILogger<NotificationController> logger;

        public NotificationController(INotificationService notificationService, INotificationPreferenceService notificationPreferenceService, ILogger<NotificationController> logger)
        {
            this.notificationService = notificationService;
            this.notificationPreferenceService = notificationPreferenceService;
            this.logger = logger;
        }

        [HttpGet, Authorize(Roles = ApplicationConstants.UserOnly)]
        public async Task<IActionResult> GetAll()
        {
            logger.LogInformation("Fetching notifications for user ID: {UserId}", LoggedInUserId);
            try
            {
                var notifications = await notificationService.GetAllNotifications(LoggedInUserId);
                logger.LogInformation("Fetched {Count} notifications for user ID: {UserId}", notifications.Count, LoggedInUserId);
                return Ok(notifications);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpGet("configurations"), Authorize(Roles = ApplicationConstants.UserOnly)]
        public async Task<IActionResult> GetNotificationConfigurations()
        {
            logger.LogInformation("Fetching notification preferences for user ID: {UserId}", LoggedInUserId);
            try
            {
                var preferences = await notificationPreferenceService.GetNotificationPreferences(new List<int> { LoggedInUserId });
                logger.LogInformation("Fetched notification preferences for user ID: {UserId}", LoggedInUserId);
                return Ok(preferences);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }
    }
}
