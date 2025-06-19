using Microsoft.AspNetCore.Mvc;
using NewsAggregationSystem.API.Services.Scheduler;
using NewsAggregationSystem.API.Services.Users;

namespace NewsAggregationSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseController
    {
        private readonly IUserService userService;
        private readonly NewsFetchScheduler newsFetchScheduler;
        public UsersController(IUserService userService, NewsFetchScheduler newsFetchScheduler)
        {
            this.userService = userService;
            this.newsFetchScheduler = newsFetchScheduler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await userService.GetAllUsers());
        }
    }
}
