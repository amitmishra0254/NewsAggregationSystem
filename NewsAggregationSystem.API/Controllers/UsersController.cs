using Microsoft.AspNetCore.Mvc;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Service.Interfaces;

namespace NewsAggregationSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseController
    {
        private readonly IUserService userService;
        private readonly ILogger<UsersController> logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            this.userService = userService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            logger.LogInformation(ApplicationConstants.LogMessage.FetchingAllUsers);

            try
            {
                var users = await userService.GetAllUsers();
                LogUsersRetrievedSuccessfully(users.Count);
                return Ok(users);
            }
            catch (Exception exception)
            {
                LogUserRetrievalError(exception);
                return CreateInternalServerErrorResponse(exception);
            }
        }

        private void LogUsersRetrievedSuccessfully(int userCount)
        {
            logger.LogInformation(ApplicationConstants.LogMessage.UsersFetchedSuccessfully, userCount);
        }

        private void LogUserRetrievalError(Exception exception)
        {
            logger.LogError(exception, exception.Message);
        }

        private IActionResult CreateInternalServerErrorResponse(Exception exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
        }
    }
}
