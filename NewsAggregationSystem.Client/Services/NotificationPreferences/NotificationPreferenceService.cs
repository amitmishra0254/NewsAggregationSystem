using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NotificationPreferences;
using NewsAggregationSystem.Common.DTOs.Users;
using Spectre.Console;
using System.Net.Http.Headers;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Services.NotificationPreferences
{
    public class NotificationPreferenceService : INotificationPreferenceService
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public NotificationPreferenceService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<NotificationPreferenceDTO>> GetUserNotificationPreferences()
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                       new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                var response = await httpClient.GetAsync("Notification/configurations");

                if (!response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine($"[red]Error fetching notification preferences: {(int)response.StatusCode} - {response.ReasonPhrase}[/]");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    AnsiConsole.MarkupLine($"[red]Details: {errorContent}[/]");
                    return new();
                }

                var jsonString = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<List<NotificationPreferenceDTO>>(jsonString, options);
            }
            catch (Exception exception)
            {
                AnsiConsole.MarkupLine($"[red]Exception occurred while fetching notification preferences: {exception.Message}[/]");
                return new();
            }
        }


        public async Task AddKeyword(string keyword, int categoryId)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                var response = await httpClient.PostAsync(
                    $"{ApplicationConstants.NotificationPreferences}/add-keyword?keyword={Uri.EscapeDataString(keyword)}&categoryId={categoryId}",
                    null);

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine($"[green]Keyword '{keyword}' added successfully.[/]");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    AnsiConsole.MarkupLine("[yellow]Keyword is already exist.[/]");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    AnsiConsole.MarkupLine($"[red]Failed to add keyword. Status Code: {(int)response.StatusCode}[/]");
                    AnsiConsole.MarkupLine($"[red]Message: {error}[/]");
                }
            }
            catch (Exception exception)
            {
                AnsiConsole.MarkupLine($"[red]Error while adding keyword: {exception.Message}[/]");
            }
        }

        public async Task ChangeKeywordStatus(int keywordId, bool isEnable)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                var response = await httpClient.PostAsync(
                    $"{ApplicationConstants.NotificationPreferences}/change-keyword-status?keywordId={keywordId}&isEnable={isEnable}",
                    null);

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine("[green]Keyword status updated successfully.[/]");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    AnsiConsole.MarkupLine($"[red]Failed to update keyword status. Status Code: {(int)response.StatusCode}[/]");
                    AnsiConsole.MarkupLine($"[red]Message: {error}[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error while changing keyword status: {ex.Message}[/]");
            }
        }

        public async Task ChangeCategoryStatus(int categoryId, bool isEnable)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                var response = await httpClient.PostAsync(
                    $"{ApplicationConstants.NotificationPreferences}/change-category-status?categoryId={categoryId}&isEnable={isEnable}",
                    null);

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine("[green]Category status updated successfully.[/]");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    AnsiConsole.MarkupLine($"[red]Failed to update category status. Status Code: {(int)response.StatusCode}[/]");
                    AnsiConsole.MarkupLine($"[red]Message: {error}[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error while changing category status: {ex.Message}[/]");
            }
        }
    }
}
