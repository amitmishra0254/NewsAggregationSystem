using Microsoft.AspNetCore.Mvc;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Service.Interfaces;

namespace NewsAggregationSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : BaseController
    {
        private readonly IReportService reportService;
        private readonly ILogger<ReportsController> logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
        {
            this.reportService = reportService;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ReportNewsArticle(ReportRequestDTO report)
        {
            logger.LogInformation("User {UserId} is attempting to report article ID: {ArticleId} with reason: {Reason}", LoggedInUserId, report.ArticleId, report.Reason);
            try
            {
                var response = await reportService.ReportNewsArticle(report, LoggedInUserId);

                if (response == 0)
                {
                    logger.LogWarning("Article ID: {ArticleId} has already been reported by user ID: {UserId}", report.ArticleId, LoggedInUserId);
                    return BadRequest(string.Format(ApplicationConstants.ArticleIsAlreadyReported, report.ArticleId));
                }

                logger.LogInformation("Article ID: {ArticleId} reported successfully by user ID: {UserId}", report.ArticleId, LoggedInUserId);
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
