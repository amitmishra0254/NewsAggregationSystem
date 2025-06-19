using Microsoft.AspNetCore.Mvc;
using NewsAggregationSystem.API.Services.NewsSources;
using NewsAggregationSystem.Common.DTOs.NewsSources;

namespace NewsAggregationSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsSourcesController : BaseController
    {
        private readonly INewsSourceService service;
        private readonly ILogger<NewsSourcesController> logger;
        private readonly int LoggerInUserId = 10;

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
                return Ok(await service.GetAll());
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
            try
            {
                var source = await service.GetById(Id);
                return source == null ? NotFound() : Ok(source);
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
            try
            {
                await service.Add(newsSource, LoggerInUserId);
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
            try
            {
                await service.Update(Id, newsSource, LoggerInUserId);
                return Ok();
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
            try
            {
                await service.Delete(Id);
                return Ok();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }
    }
}
