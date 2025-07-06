using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.Authenticate;
using NewsAggregationSystem.Common.DTOs.Users;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Service.Interfaces;

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
        public async Task<IActionResult> RegisterUser([FromBody] UserRequestDTO userRequest)
        {
            logger.LogInformation(ApplicationConstants.LogMessage.SigningUpUser, userRequest.Email);
            try
            {
                var createdUser = await userService.CreateUserAsync(userRequest);
                logger.LogInformation(ApplicationConstants.LogMessage.UserSignedUpSuccessfully, userRequest.Email);
                return CreatedAtAction(nameof(RegisterUser), createdUser);
            }
            catch (AlreadyExistException exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status409Conflict, exception.Message);
            }
            catch (NotFoundException exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status404NotFound, exception.Message);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [HttpPost("login"), AllowAnonymous]
        public async Task<IActionResult> AuthenticateUser(LoginRequestDTO loginRequest)
        {
            logger.LogInformation(ApplicationConstants.LogMessage.UserLoginAttempt, loginRequest.Email);
            try
            {
                var result = await authService.AuthenticateUserAsync(loginRequest);
                logger.LogInformation(ApplicationConstants.LogMessage.UserLoggedInSuccessfully, loginRequest.Email);

                return Ok(result);
            }
            catch (NotFoundException exception)
            {
                logger.LogError(exception, exception.Message);
                return NotFound(exception.Message);
            }
            catch (InvalidCredentialsException exception)
            {
                logger.LogError(exception, exception.Message);
                return Unauthorized(exception.Message);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }

        [Authorize(Roles = ApplicationConstants.UserOnly)]
        [HttpPost("logout")]
        public async Task<IActionResult> SignOutUser()
        {
            logger.LogInformation(ApplicationConstants.LogMessage.UserLogoutAttempt);
            try
            {
                Response.Cookies.Append("accessToken", "", new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddDays(-1),
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });
                logger.LogInformation(ApplicationConstants.LogMessage.UserLoggedOutSuccessfully);
                return Ok(new { message = "User logged out successfully." });
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, exception.Message);
            }
        }
    }
}
