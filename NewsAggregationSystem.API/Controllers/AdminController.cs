using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Service.Interfaces;

namespace NewsAggregationSystem.API.Controllers
{
    [Authorize(Roles = ApplicationConstants.AdminOnly)]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : BaseController
    {
        private readonly IHiddenArticleKeywordService hiddenArticleKeywordService;
        private readonly ILogger<AdminController> logger;

        public AdminController(IHiddenArticleKeywordService hiddenArticleKeywordService, ILogger<AdminController> logger)
        {
            this.hiddenArticleKeywordService = hiddenArticleKeywordService;
            this.logger = logger;
        }

        [HttpPost("hide-article-by-keyword")]
        public async Task<IActionResult> AddKeywordToHideArticles([FromQuery] string keyword)
        {
            logger.LogInformation(ApplicationConstants.LogMessage.HidingArticlesByKeywordStarted, keyword, LoggedInUserId);
            try
            {
                var result = await hiddenArticleKeywordService.Add(keyword, LoggedInUserId);

                logger.LogInformation(ApplicationConstants.LogMessage.HidingArticlesByKeywordSucceeded, keyword, LoggedInUserId);

                return Ok(result);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, ApplicationConstants.LogMessage.HidingArticlesByKeywordFailed, keyword, LoggedInUserId, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }
    }
}
