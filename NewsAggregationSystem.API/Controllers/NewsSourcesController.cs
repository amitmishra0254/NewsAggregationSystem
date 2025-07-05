using Microsoft.AspNetCore.Mvc;
using NewsAggregationSystem.Common.DTOs.NewsSources;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Service.Interfaces;

namespace NewsAggregationSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsSourcesController : BaseController
    {
        private readonly INewsSourceService service;
        private readonly ILogger<NewsSourcesController> logger;

        public NewsSourcesController(INewsSourceService service, ILogger<NewsSourcesController> logger)
        {
            this.service = service;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var sources = await service.GetAll();
                logger.LogInformation("Fetched {Count} news sources.", sources.Count);
                return Ok(sources);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetById(int Id)
        {
            logger.LogInformation("Fetching news source with ID: {Id}", Id);
            try
            {
                var source = await service.GetById(Id);
                if (source == null)
                {
                    logger.LogWarning("News source with ID: {Id} not found.", Id);
                    return NotFound();
                }

                logger.LogInformation("News source with ID: {Id} fetched successfully.", Id);
                return Ok(source);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateNewsSourceDTO newsSource)
        {
            logger.LogInformation("Adding new news source with name: {Name}", newsSource.Name);
            try
            {
                await service.Add(newsSource, LoggedInUserId);
                logger.LogInformation("News source '{Name}' added successfully by user ID: {UserId}", newsSource.Name, LoggedInUserId);
                return Ok();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpPut("{Id}")]
        public async Task<IActionResult> Update(int Id, [FromBody] UpdateNewsSourceDTO newsSource)
        {
            logger.LogInformation("Updating news source ID: {Id}", Id);
            try
            {
                await service.Update(Id, newsSource, LoggedInUserId);
                logger.LogInformation("News source ID: {Id} updated successfully by user ID: {UserId}", Id, LoggedInUserId);
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

        [HttpDelete("{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            logger.LogInformation("Deleting news source with ID: {Id}", Id);
            try
            {
                await service.Delete(Id);
                logger.LogInformation("News source with ID: {Id} deleted successfully.", Id);
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
    }
}
