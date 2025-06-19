using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsAggregationSystem.API.Services.NewsCategories;
using NewsAggregationSystem.API.Services.NotificationPreferences;
using NewsAggregationSystem.Common.Constants;

namespace NewsAggregationSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsCategoryController : BaseController
    {
        private readonly INewsCategoryService newsCategoryService;
        private readonly INotificationPreferenceService notificationPreferenceService;

        public NewsCategoryController(INewsCategoryService newsCategoryService, INotificationPreferenceService notificationPreferenceService)
        {
            this.newsCategoryService = newsCategoryService;
            this.notificationPreferenceService = notificationPreferenceService;
        }

        [Authorize(Roles = ApplicationConstants.AdminOnly)]
        [HttpPost]
        public async Task<IActionResult> Add(string category)
        {
            try
            {
                var newsCategoryId = await newsCategoryService.AddNewsCategory(category, LoggedInUserId);
                await notificationPreferenceService.AddNotificationPreferencesPerCategory(newsCategoryId);
                return CreatedAtAction(nameof(Add), newsCategoryId);
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }
    }
}
