using NewsAggregationSystem.Client.MyClient;
using NewsAggregationSystem.Common.DTOs.Authenticate;
using NewsAggregationSystem.Common.DTOs.Users;
using NewsAggregationSystem.Common.Exceptions;

namespace NewsAggregationSystem.Client.Services.Authentication
{
    public class AuthService : IAuthService
    {
        private readonly IApiClient _apiClient;
        private const string LoginEndpoint = "Auth/login";

        public AuthService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<AuthResponseDTO?> LoginAsync(LoginRequestForClientDTO request)
        {
            try
            {
                return await _apiClient.PostAsync<LoginRequestForClientDTO, AuthResponseDTO>(LoginEndpoint, request);
            }
            catch (NotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (InvalidCredentialsException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
    }

}
