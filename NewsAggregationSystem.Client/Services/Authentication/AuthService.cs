using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.Authenticate;
using NewsAggregationSystem.Common.DTOs.Users;
using Spectre.Console;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Services.Authentication
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient httpClient;

        public AuthService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<AuthResponseDTO?> Login(LoginRequestForClientDTO request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, ApplicationConstants.JsonContentType);

                var response = await httpClient.PostAsync(ApplicationConstants.LoginPath, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content.ReadAsStreamAsync();
                    return await JsonSerializer.DeserializeAsync<AuthResponseDTO>(responseStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    AnsiConsole.MarkupLine("[yellow]Invalid email or password format.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    AnsiConsole.MarkupLine("[red]Incorrect email or password.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    AnsiConsole.MarkupLine("[yellow]No account found with the provided email.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Login failed. Status: {(int)response.StatusCode} - {response.ReasonPhrase}[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Login error: {ex.Message}[/]");
            }
            return null;
        }

        public async Task Signup(UserRequestDTO request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, ApplicationConstants.JsonContentType);

                var response = await httpClient.PostAsync(ApplicationConstants.SignupPath, content);

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine("[bold green]Account Created Successfully.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    AnsiConsole.MarkupLine("[yellow]Invalid input. Please check the details you entered.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    AnsiConsole.MarkupLine("[yellow]An account with this email already exists.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Signup failed. Status: {(int)response.StatusCode} - {response.ReasonPhrase}[/]");
                }
            }
            catch (Exception exception)
            {
                AnsiConsole.MarkupLine($"[red]Signup error: {exception.Message}[/]");
            }
        }
    }
}