using NewsAggregationSystem.Client.Services.Interfaces;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsCategories;
using NewsAggregationSystem.Common.DTOs.Users;
using Spectre.Console;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Services
{
    public class NewsCategoryService : INewsCategoryService
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;

        public NewsCategoryService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task CreateNewsCategoryAsync(string category)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await SendCreateNewsCategoryRequestAsync(category);
                await HandleCreateNewsCategoryResponseAsync(response);
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ApplicationConstants.LogMessage.ErrorAddingNewsCategory, ex.Message);
            }
        }

        public async Task ToggleNewsCategoryVisibilityAsync(int categoryId, bool isHidden)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await SendToggleCategoryVisibilityRequestAsync(categoryId, isHidden);
                await HandleToggleCategoryVisibilityResponseAsync(response, isHidden);
            }
            catch (Exception exception)
            {
                DisplayErrorMessage(ApplicationConstants.LogMessage.ErrorTogglingCategoryVisibility, exception.Message);
            }
        }

        public async Task<List<NotificationPreferencesKeywordDTO>> GetAllNewsCategoriesAsync()
        {
            try
            {
                SetAuthorizationHeader();
                var response = await httpClient.GetAsync("NewsCategories");
                return await HandleGetAllNewsCategoriesResponseAsync(response);
            }
            catch (Exception exception)
            {
                DisplayErrorMessage(ApplicationConstants.LogMessage.ErrorFetchingCategories, exception.Message);
                return new List<NotificationPreferencesKeywordDTO>();
            }
        }

        private void SetAuthorizationHeader()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserState.AccessToken);
        }

        private async Task<HttpResponseMessage> SendCreateNewsCategoryRequestAsync(string category)
        {
            var url = $"{ApplicationConstants.AddNewsCategoryPath}?category={Uri.EscapeDataString(category)}";
            return await httpClient.PostAsync(url, null);
        }

        private async Task<HttpResponseMessage> SendToggleCategoryVisibilityRequestAsync(int categoryId, bool isHidden)
        {
            var url = $"NewsCategories/toggle-visibility?categoryId={categoryId}&IsHidden={isHidden}";
            return await httpClient.PostAsync(url, null);
        }

        private async Task HandleCreateNewsCategoryResponseAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                DisplaySuccessMessage(ApplicationConstants.LogMessage.ClientNewsCategoryAddedSuccessfully);
                return;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.Conflict:
                    DisplayWarningMessage(ApplicationConstants.LogMessage.NewsCategoryAlreadyExists);
                    break;
                default:
                    var error = await response.Content.ReadAsStringAsync();
                    DisplayErrorMessage(ApplicationConstants.LogMessage.FailedToAddCategory, (int)response.StatusCode);
                    DisplayErrorMessage($"Message: {error}");
                    break;
            }
        }

        private async Task HandleToggleCategoryVisibilityResponseAsync(HttpResponseMessage response, bool isHidden)
        {
            if (response.IsSuccessStatusCode)
            {
                var message = isHidden
                    ? ApplicationConstants.LogMessage.CategoryHiddenSuccessfully
                    : ApplicationConstants.LogMessage.CategoryUnhiddenSuccessfully;
                DisplaySuccessMessage(message);
                return;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    DisplayWarningMessage(ApplicationConstants.LogMessage.CategoryVisibilityAlreadyUpdated);
                    break;
                case HttpStatusCode.NotFound:
                    DisplayErrorMessage(ApplicationConstants.LogMessage.CategoryNotFound);
                    break;
                default:
                    var message = await response.Content.ReadAsStringAsync();
                    DisplayErrorMessage(ApplicationConstants.LogMessage.FailedToToggleCategoryVisibility, (int)response.StatusCode);
                    DisplayErrorMessage($"Message: {message}");
                    break;
            }
        }

        private async Task<List<NotificationPreferencesKeywordDTO>> HandleGetAllNewsCategoriesResponseAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<NotificationPreferencesKeywordDTO>>(responseString, jsonOptions)
                    ?? new List<NotificationPreferencesKeywordDTO>();
            }

            DisplayErrorMessage(ApplicationConstants.LogMessage.FailedToRetrieveNewsCategories, (int)response.StatusCode);
            return new List<NotificationPreferencesKeywordDTO>();
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
