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
        private readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public NotificationServices(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<NotificationDTO>> GetAllNotifications()
        {
            List<NotificationDTO> notifications = new List<NotificationDTO>();
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                var response = await httpClient.GetAsync(ApplicationConstants.Notification);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    notifications = JsonSerializer.Deserialize<List<NotificationDTO>>(json, jsonSerializerOptions);
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Failed to fetch notifications. Status Code: {(int)response.StatusCode}[/]");
                    AnsiConsole.MarkupLine($"[red]Reason: {response.ReasonPhrase}[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error occurred while fetching notifications: {ex.Message}[/]");
            }

            return notifications;
        }
    }
}
