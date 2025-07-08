using NewsAggregationSystem.Client.Services.Interfaces;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NotificationPreferences;
using NewsAggregationSystem.Common.DTOs.Users;
using Spectre.Console;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Services
{
    public class NotificationPreferenceService : INotificationPreferenceService
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;

        public NotificationPreferenceService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<List<NotificationPreferenceDTO>> GetUserNotificationPreferencesAsync()
        {
            try
            {
                SetAuthorizationHeader();
                var response = await httpClient.GetAsync("Notification/configurations");
                return await HandleGetNotificationPreferencesResponseAsync(response);
            }
            catch (Exception exception)
            {
                DisplayErrorMessage("Exception occurred while fetching notification preferences: {0}", exception.Message);
                return new List<NotificationPreferenceDTO>();
            }
        }

        public async Task AddKeywordToCategoryAsync(string keyword, int categoryId)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await SendAddKeywordRequestAsync(keyword, categoryId);
                await HandleAddKeywordResponseAsync(response, keyword);
            }
            catch (Exception exception)
            {
                DisplayErrorMessage("Error while adding keyword: {0}", exception.Message);
            }
        }

        public async Task UpdateKeywordStatusAsync(int keywordId, bool isEnable)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await SendUpdateKeywordStatusRequestAsync(keywordId, isEnable);
                await HandleUpdateKeywordStatusResponseAsync(response);
            }
            catch (Exception ex)
            {
                DisplayErrorMessage("Error while changing keyword status: {0}", ex.Message);
            }
        }

        public async Task UpdateCategoryStatusAsync(int categoryId, bool isEnable)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await SendUpdateCategoryStatusRequestAsync(categoryId, isEnable);
                await HandleUpdateCategoryStatusResponseAsync(response);
            }
            catch (Exception ex)
            {
                DisplayErrorMessage("Error while changing category status: {0}", ex.Message);
            }
        }

        private void SetAuthorizationHeader()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserState.AccessToken);
        }

        private async Task<HttpResponseMessage> SendAddKeywordRequestAsync(string keyword, int categoryId)
        {
            var url = $"{ApplicationConstants.NotificationPreferences}/add-keyword?keyword={Uri.EscapeDataString(keyword)}&categoryId={categoryId}";
            return await httpClient.PostAsync(url, null);
        }

        private async Task<HttpResponseMessage> SendUpdateKeywordStatusRequestAsync(int keywordId, bool isEnable)
        {
            var url = $"{ApplicationConstants.NotificationPreferences}/change-keyword-status?keywordId={keywordId}&isEnable={isEnable}";
            return await httpClient.PostAsync(url, null);
        }

        private async Task<HttpResponseMessage> SendUpdateCategoryStatusRequestAsync(int categoryId, bool isEnable)
        {
            var url = $"{ApplicationConstants.NotificationPreferences}/change-category-status?categoryId={categoryId}&isEnable={isEnable}";
            return await httpClient.PostAsync(url, null);
        }

        private async Task<List<NotificationPreferenceDTO>> HandleGetNotificationPreferencesResponseAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<NotificationPreferenceDTO>>(jsonString, jsonOptions) ?? new List<NotificationPreferenceDTO>();
            }

            DisplayErrorMessage("Error fetching notification preferences: {0} - {1}", (int)response.StatusCode, response.ReasonPhrase);
            var errorContent = await response.Content.ReadAsStringAsync();
            DisplayErrorMessage("Details: {0}", errorContent);
            return new List<NotificationPreferenceDTO>();
        }

        private async Task HandleAddKeywordResponseAsync(HttpResponseMessage response, string keyword)
        {
            if (response.IsSuccessStatusCode)
            {
                DisplaySuccessMessage("Keyword '{0}' added successfully.", keyword);
                return;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.Conflict:
                    DisplayWarningMessage("Keyword is already exist.");
                    break;
                default:
                    var error = await response.Content.ReadAsStringAsync();
                    DisplayErrorMessage("Failed to add keyword. Status Code: {0}", (int)response.StatusCode);
                    DisplayErrorMessage("Message: {0}", error);
                    break;
            }
        }

        private async Task HandleUpdateKeywordStatusResponseAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                DisplaySuccessMessage("Keyword status updated successfully.");
                return;
            }

            var error = await response.Content.ReadAsStringAsync();
            DisplayErrorMessage("Failed to update keyword status. Status Code: {0}", (int)response.StatusCode);
            DisplayErrorMessage("Message: {0}", error);
        }

        private async Task HandleUpdateCategoryStatusResponseAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                DisplaySuccessMessage("Category status updated successfully.");
                return;
            }

            var error = await response.Content.ReadAsStringAsync();
            DisplayErrorMessage("Failed to update category status. Status Code: {0}", (int)response.StatusCode);
            DisplayErrorMessage("Message: {0}", error);
        }

        private void DisplaySuccessMessage(string message, params object[] args)
        {
            var formattedMessage = string.Format(message, args);
            AnsiConsole.MarkupLine($"[green]{formattedMessage}[/]");
        }

        private void DisplayWarningMessage(string message, params object[] args)
        {
            var formattedMessage = string.Format(message, args);
            AnsiConsole.MarkupLine($"[yellow]{formattedMessage}[/]");
        }

        private void DisplayErrorMessage(string message, params object[] args)
        {
            var formattedMessage = string.Format(message, args);
            AnsiConsole.MarkupLine($"[red]{formattedMessage}[/]");
        }
    }
}
