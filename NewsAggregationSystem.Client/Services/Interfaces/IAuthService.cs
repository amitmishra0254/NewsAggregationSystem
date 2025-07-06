using NewsAggregationSystem.Common.DTOs.Authenticate;
using NewsAggregationSystem.Common.DTOs.Users;

namespace NewsAggregationSystem.Client.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO?> Login(LoginRequestForClientDTO request);
        Task Signup(UserRequestDTO request);
    }

}
