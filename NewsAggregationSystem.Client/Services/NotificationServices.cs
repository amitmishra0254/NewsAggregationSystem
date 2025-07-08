using NewsAggregationSystem.Client.Services.Interfaces;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.Notifications;
using NewsAggregationSystem.Common.DTOs.Users;
using Spectre.Console;
using System.Net.Http.Headers;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Services
{
    public class NotificationServices : INotificationServices
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;

        public NotificationServices(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<List<NotificationDTO>> GetUserNotificationsAsync()
        {
            try
            {
                SetAuthorizationHeader();
                var response = await httpClient.GetAsync(ApplicationConstants.Notification);
                return await HandleGetNotificationsResponseAsync(response);
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ApplicationConstants.LogMessage.ErrorFetchingSavedArticles, ex.Message);
                return new List<NotificationDTO>();
            }
        }

        private void SetAuthorizationHeader()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserState.AccessToken);
        }

        private async Task<List<NotificationDTO>> HandleGetNotificationsResponseAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<NotificationDTO>>(json, jsonOptions) ?? new List<NotificationDTO>();
            }

            DisplayErrorMessage("Failed to fetch notifications. Status Code: {0}", (int)response.StatusCode);
            DisplayErrorMessage("Reason: {0}", response.ReasonPhrase);
            return new List<NotificationDTO>();
        }

        private void DisplayErrorMessage(string message, params object[] args)
        {
            var formattedMessage = string.Format(message, args);
            AnsiConsole.MarkupLine($"[red]{formattedMessage}[/]");
        }
    }
}
