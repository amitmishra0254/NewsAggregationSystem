using NewsAggregationSystem.Common.DTOs.Authenticate;

namespace NewsAggregationSystem.Service.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> Login(LoginRequestDTO loginRequest);
    }
}
