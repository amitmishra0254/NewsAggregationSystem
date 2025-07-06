using NewsAggregationSystem.Common.DTOs.Authenticate;
using NewsAggregationSystem.Common.DTOs.Users;

namespace NewsAggregationSystem.Client.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO?> AuthenticateUserAsync(LoginRequestForClientDTO request);
        Task RegisterUserAsync(UserRequestDTO request);
    }
}
