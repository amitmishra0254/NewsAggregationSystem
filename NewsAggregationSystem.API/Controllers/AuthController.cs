using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsAggregationSystem.API.Services.Authentication;
using NewsAggregationSystem.API.Services.Users;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.Authenticate;
using NewsAggregationSystem.Common.DTOs.Users;
using NewsAggregationSystem.Common.Exceptions;

namespace NewsAggregationSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly ILogger<AuthController> logger;
        private readonly IConfiguration configuration;
        private readonly IUserService userService;

        public AuthController(IAuthService authService, ILogger<AuthController> logger, IConfiguration configuration, IUserService userService)
        {
            this.authService = authService;
            this.logger = logger;
            this.configuration = configuration;
            this.userService = userService;
        }

        [HttpPost("sign-up"), AllowAnonymous]
        public async Task<IActionResult> Add([FromBody] UserRequestDTO userRequest)
        {
            try
            {
                return CreatedAtAction(nameof(Add), await userService.AddUser(userRequest));
            }
            catch (AlreadyExistException exception)
            {
                return StatusCode(StatusCodes.Status409Conflict, exception.Message);
            }
            catch (NotFoundException exception)
            {
                return StatusCode(StatusCodes.Status404NotFound, exception.Message);
            }
            catch (Exception exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpPost("login"), AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequestDTO loginRequest)
        {
            try
            {
                return Ok(await authService.Login(loginRequest));
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidCredentialsException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ApplicationConstants.UnexpectedError });
            }
        }
        [Authorize(Roles = ApplicationConstants.AdminUser)]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                Response.Cookies.Append("accessToken", "", new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddDays(-1),
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });

                return Ok(new { message = "User logged out successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ApplicationConstants.UnexpectedError });
            }
        }
    }
}
