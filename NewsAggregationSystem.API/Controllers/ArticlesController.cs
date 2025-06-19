using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsAggregationSystem.API.Services.Articles;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsArticles;

namespace NewsAggregationSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : BaseController
    {
        private readonly IArticleService articleService;

        public ArticlesController(IArticleService articleService)
        {
            this.articleService = articleService;
        }

        [HttpGet, Authorize(Roles = ApplicationConstants.AdminUser)]
        public async Task<IActionResult> GetAll([FromBody] NewsArticleRequestDTO newsArticleRequestDTO)
        {
            try
            {
                return Ok(await articleService.GetAllArticles(newsArticleRequestDTO));
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpDelete("saved-articles/{Id:int}"), Authorize(Roles = ApplicationConstants.AdminUser)]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                return Ok(await articleService.DeleteSavedArticles(Id, LoggedInUserId));
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpGet("saved-articles"), Authorize(Roles = ApplicationConstants.AdminUser)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                return Ok(await articleService.GetAllSavedArticles(LoggedInUserId));
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpGet("save-article{Id:int}"), Authorize(Roles = ApplicationConstants.AdminUser)]
        public async Task<IActionResult> SaveArticle(int Id)
        {
            try
            {
                return Ok(await articleService.SaveArticle(Id, LoggedInUserId));
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpGet("{Id}"), Authorize(Roles = ApplicationConstants.AdminUser)]
        public async Task<IActionResult> GetById(int Id)
        {
            try
            {
                var article = await articleService.GetArticleById(Id);
                if (article == null)
                    return NotFound();

                return Ok(article);
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }
    }
}
