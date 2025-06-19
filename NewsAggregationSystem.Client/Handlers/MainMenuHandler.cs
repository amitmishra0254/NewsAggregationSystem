using NewsAggregationSystem.Client.MyClient;
using NewsAggregationSystem.Client.Services.Authentication;
using NewsAggregationSystem.Common.Constants;

namespace NewsAggregationSystem.Client.Handlers
{
    public class MainMenuHandler : IMainMenuHandler
    {
        private readonly IApiClient apiClient;
        private readonly IAuthService authService;
        private readonly HttpClient httpClient;

        public MainMenuHandler()
        {
            httpClient = new HttpClient();
            apiClient = new ApiClient(httpClient);
            authService = new AuthService(apiClient);
            httpClient.BaseAddress = new Uri("https://localhost:7122/api/");
        }

        public async Task ShowWelcomeMenuAsync()
        {
            Console.Clear();
            Console.WriteLine($"{ApplicationConstants.WelcomeMessageWithMenu}");

            Console.Write("\nEnter your choice (1-3): ");
            string input = Console.ReadLine();

            do
            {
                switch (input)
                {
                    case "1":
                        await HandleLoginAsync();
                        break;
                    case "2":
                        Console.WriteLine("Sign up.");
                        break;
                    case "3":
                        Console.WriteLine("Exit.");
                        return;
                    default:
                        Console.WriteLine("Invalid selection. Please try again.");
                        break;
                }
            } while (input != "3");

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private void HandleLoginAsync()
        {
            authService.LoginAsync();
        }
    }
}
