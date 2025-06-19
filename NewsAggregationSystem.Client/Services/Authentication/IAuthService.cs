using NewsAggregationSystem.Common.DTOs.Authenticate;
using NewsAggregationSystem.Common.DTOs.Users;

namespace NewsAggregationSystem.Client.Services.Authentication
{
    public interface IAuthService
    {
        Task<AuthResponseDTO?> LoginAsync(LoginRequestForClientDTO request);
    }

}
