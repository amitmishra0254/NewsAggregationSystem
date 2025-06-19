using NewsAggregationSystem.Client.Handlers;
using NewsAggregationSystem.Client.MyClient;
using NewsAggregationSystem.Client.Services.Authentication;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.Users;

namespace NewsAggregationSystem.Client
{
    public class Startup
    {
        private readonly IMainMenuHandler mainMenuHandler;
        public Startup()
        {
            mainMenuHandler = new MainMenuHandler();
        }
        public static void Main(string[] args)
        {
            ShowMainMenu();
        }

        private static void ShowMainMenu()
        {
            Console.Clear();
            Console.WriteLine($"{ApplicationConstants.WelcomeMessageWithMenu}");

            Console.Write("\nEnter your choice (1-3): ");
            string input = Console.ReadLine();

            var httpClient = new System.Net.Http.HttpClient
            {
                BaseAddress = new Uri("https://localhost:7122/api/")
            };


            IApiClient apiClient = new ApiClient(httpClient);
            IAuthService authService = new AuthService(apiClient);

            switch (input)
            {
                case "1":
                    Console.WriteLine("Welcome to the News Application!");
                    Console.Write("Enter your email: ");
                    string email = Console.ReadLine();

                    Console.Write("Enter your password: ");
                    string password = Console.ReadLine();

                    var loginRequest = new LoginRequestForClientDTO
                    {
                        Email = email,
                        Password = password
                    };

                    authService.LoginAsync(loginRequest);
                    break;
                case "2":
                    Console.WriteLine("Sign up.");
                    break;
                case "3":
                    Console.WriteLine("Exit.");
                    break;
                default:
                    Console.WriteLine("Invalid selection. Please try again.");
                    break;
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
