using NewsAggregationSystem.Client.Factories;
using NewsAggregationSystem.Client.Services;
using NewsAggregationSystem.Client.Services.Interfaces;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.Users;
using NewsAggregationSystem.Common.Utilities;
using Spectre.Console;

namespace NewsAggregationSystem.Client.Handlers
{
    public class MainMenuHandler : IMainMenuHandler
    {
        private readonly IAuthService authService;
        private readonly HttpClient httpClient;

        public MainMenuHandler()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(ApplicationConstants.BaseUrl);
            authService = new AuthService(httpClient);
        }

        public async Task ShowWelcomeMenu()
        {
            string choice = "";
            do
            {
                Console.Clear();
                AnsiConsole.MarkupLine($"[bold green]{ApplicationConstants.WelcomeMessage}[/]");

                choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Enter your choice: ")
                        .AddChoices(ApplicationConstants.WelcomeMenu));

                switch (choice)
                {
                    case ApplicationConstants.Login:
                        await HandleLogin();
                        break;

                    case ApplicationConstants.SignUp:
                        await HandleSignup();
                        break;

                    case ApplicationConstants.Exit:
                        Environment.Exit(0);
                        return;
                }
                InputHelper.PressKeyToContinue();
            } while (choice != ApplicationConstants.Exit);
        }

        private async Task HandleLogin()
        {
            try
            {
                Console.Clear();
                AnsiConsole.MarkupLine("[bold green]Welcome to the News Application![/]\n");
                var email = InputHelper.ReadString("Enter your email: ");
                var password = InputHelper.ReadPassword("Enter your password: ");

                var loginRequest = new LoginRequestForClientDTO
                {
                    Email = email,
                    Password = password
                };

                var response = await authService.AuthenticateUserAsync(loginRequest);

                if (response != null)
                {
                    UserState.AccessToken = response.AccessToken;
                    UserState.Role = response.Roles;
                    UserState.UserName = response.UserName;
                    UserState.IsLoggedIn = true;
                    var menu = MenuFactory.GetMenuProvider(UserState.Role, httpClient);
                    await menu.ShowMenu();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private async Task HandleSignup()
        {
            Console.Clear();
            var firstName = InputHelper.ReadString("Enter your first name: ");
            var lastName = InputHelper.ReadString("Enter your last name: ");
            var userName = InputHelper.ReadString("Enter your username: ");
            var email = InputHelper.ReadString("Enter your email: ");
            var password = InputHelper.ReadPassword("Enter your password: ");

            var signupRequest = new UserRequestDTO
            {
                FirstName = firstName,
                LastName = lastName,
                UserName = userName,
                Email = email,
                Password = password
            };
            await authService.RegisterUserAsync(signupRequest);
        }
    }
}

