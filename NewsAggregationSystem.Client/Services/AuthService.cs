using NewsAggregationSystem.Client.Services.Interfaces;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.Authenticate;
using NewsAggregationSystem.Common.DTOs.Users;
using Spectre.Console;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;

        public AuthService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<AuthResponseDTO?> AuthenticateUserAsync(LoginRequestForClientDTO request)
        {
            try
            {
                var response = await SendAuthenticationRequestAsync(request);
                return await HandleAuthenticationResponseAsync(response);
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ApplicationConstants.LogMessage.LoginError, ex.Message);
                return null;
            }
        }

        public async Task RegisterUserAsync(UserRequestDTO request)
        {
            try
            {
                var response = await SendRegistrationRequestAsync(request);
                await HandleRegistrationResponseAsync(response);
            }
            catch (Exception exception)
            {
                DisplayErrorMessage(ApplicationConstants.LogMessage.SignupError, exception.Message);
            }
        }

        private async Task<HttpResponseMessage> SendAuthenticationRequestAsync(LoginRequestForClientDTO request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = CreateJsonContent(json);
            return await httpClient.PostAsync(ApplicationConstants.LoginPath, content);
        }

        private async Task<HttpResponseMessage> SendRegistrationRequestAsync(UserRequestDTO request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = CreateJsonContent(json);
            return await httpClient.PostAsync(ApplicationConstants.SignupPath, content);
        }

        private StringContent CreateJsonContent(string json)
        {
            return new StringContent(json, Encoding.UTF8, ApplicationConstants.JsonContentType);
        }

        private async Task<AuthResponseDTO?> HandleAuthenticationResponseAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return await DeserializeAuthResponseAsync(response);
            }

            HandleAuthenticationErrorResponse(response);
            return null;
        }

        private async Task HandleRegistrationResponseAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                DisplaySuccessMessage(ApplicationConstants.LogMessage.AccountCreatedSuccessfully);
                return;
            }

            HandleRegistrationErrorResponse(response);
        }

        private async Task<AuthResponseDTO?> DeserializeAuthResponseAsync(HttpResponseMessage response)
        {
            var responseStream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<AuthResponseDTO>(responseStream, jsonOptions);
        }

        private void HandleAuthenticationErrorResponse(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    DisplayWarningMessage(ApplicationConstants.LogMessage.InvalidEmailPasswordFormat);
                    break;
                case HttpStatusCode.Unauthorized:
                    DisplayErrorMessage(ApplicationConstants.LogMessage.IncorrectEmailPassword);
                    break;
                case HttpStatusCode.NotFound:
                    DisplayWarningMessage(ApplicationConstants.LogMessage.NoAccountFound);
                    break;
                default:
                    DisplayErrorMessage(ApplicationConstants.LogMessage.LoginFailed, 
                        (int)response.StatusCode, response.ReasonPhrase);
                    break;
            }
        }

        private void HandleRegistrationErrorResponse(HttpResponseMessage response)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    DisplayWarningMessage(ApplicationConstants.LogMessage.InvalidInputDetails);
                    break;
                case HttpStatusCode.Conflict:
                    DisplayWarningMessage(ApplicationConstants.LogMessage.AccountAlreadyExists);
                    break;
                default:
                    DisplayErrorMessage(ApplicationConstants.LogMessage.SignupFailed, 
                        (int)response.StatusCode, response.ReasonPhrase);
                    break;
            }
        }

        private void DisplaySuccessMessage(string message)
        {
            AnsiConsole.MarkupLine($"[bold green]{message}[/]");
        }

        private void DisplayWarningMessage(string message)
        {
            AnsiConsole.MarkupLine($"[yellow]{message}[/]");
        }

        private void DisplayErrorMessage(string message, params object[] args)
        {
            var formattedMessage = string.Format(message, args);
            AnsiConsole.MarkupLine($"[red]{formattedMessage}[/]");
        }
    }
}