using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Service.Interfaces;

namespace NewsAggregationSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsCategoriesController : BaseController
    {
        private readonly INewsCategoryService newsCategoryService;
        private readonly INotificationPreferenceService notificationPreferenceService;
        private readonly ILogger<NewsCategoriesController> logger;

        public NewsCategoriesController(INewsCategoryService newsCategoryService, INotificationPreferenceService notificationPreferenceService, ILogger<NewsCategoriesController> logger)
        {
            this.newsCategoryService = newsCategoryService;
            this.notificationPreferenceService = notificationPreferenceService;
            this.logger = logger;
        }

        [Authorize(Roles = ApplicationConstants.AdminOnly)]
        [HttpPost]
        public async Task<IActionResult> CreateNewsCategory([FromQuery] string category)
        {
            logger.LogInformation(ApplicationConstants.LogMessage.AddingNewsCategory, category, LoggedInUserId);
            try
            {
                var newsCategoryId = await newsCategoryService.CreateNewsCategoryAsync(category, LoggedInUserId);
                await notificationPreferenceService.AddNotificationPreferencesPerCategory(newsCategoryId);
                logger.LogInformation(ApplicationConstants.LogMessage.NewsCategoryAddedSuccessfully, category, newsCategoryId, LoggedInUserId);
                return CreatedAtAction(nameof(CreateNewsCategory), newsCategoryId);
            }
            catch (AlreadyExistException exception)
            {
                logger.LogError(exception, exception.Message);
                return Conflict(exception.Message);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [Authorize(Roles = ApplicationConstants.AdminOnly)]
        [HttpPost("toggle-visibility")]
        public async Task<IActionResult> ToggleCategoryVisibility([FromQuery] int categoryId, [FromQuery] bool IsHidden)
        {
            logger.LogInformation(ApplicationConstants.LogMessage.TogglingNewsCategoryVisibility, categoryId, IsHidden);
            try
            {
                var response = await newsCategoryService.ToggleCategoryVisibilityAsync(categoryId, IsHidden);
                if (response == 0)
                {
                    return BadRequest();
                }

                logger.LogInformation(ApplicationConstants.LogMessage.NewsCategoryVisibilityToggled, categoryId);
                return Ok();

            }
            catch (NotFoundException exception)
            {
                logger.LogError(exception, exception.Message);
                return NotFound(exception.Message);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [Authorize(Roles = ApplicationConstants.AdminOnly)]
        [HttpGet]
        public async Task<IActionResult> GetAllNewsCategories()
        {
            logger.LogInformation(ApplicationConstants.LogMessage.FetchingAllNewsCategories, LoggedInUserRole);
            try
            {
                var categories = await newsCategoryService.GetAllNewsCategoriesAsync(LoggedInUserRole);

                logger.LogInformation(ApplicationConstants.LogMessage.NewsCategoriesFetchedSuccessfully, LoggedInUserRole);
                return Ok(categories);

            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }
    }
}
