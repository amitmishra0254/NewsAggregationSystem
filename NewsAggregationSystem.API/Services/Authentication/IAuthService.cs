using NewsAggregationSystem.Common.DTOs.Authenticate;

namespace NewsAggregationSystem.API.Services.Authentication
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> Login(LoginRequestDTO loginRequest);
    }
}
