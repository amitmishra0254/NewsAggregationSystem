using NewsAggregationSystem.Client.Services.Interfaces;
using NewsAggregationSystem.Common.DTOs.Users;
using Spectre.Console;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Services
{
    public class AdminService : IAdminService
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;

        public AdminService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task AddKeywordToHideArticlesAsync(string keyword)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await SendAddHiddenKeywordRequestAsync(keyword);
                await HandleAddHiddenKeywordResponseAsync(response, keyword);
            }
            catch (Exception ex)
            {
                DisplayErrorMessage("Error occurred while adding keyword: {0}", ex.Message);
            }
        }

        private void SetAuthorizationHeader()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserState.AccessToken);
        }

        private async Task<HttpResponseMessage> SendAddHiddenKeywordRequestAsync(string keyword)
        {
            var url = $"Admin/hide-article-by-keyword?keyword={Uri.EscapeDataString(keyword)}";
            return await httpClient.PostAsync(url, null);
        }

        private async Task HandleAddHiddenKeywordResponseAsync(HttpResponseMessage response, string keyword)
        {
            if (response.IsSuccessStatusCode)
            {
                DisplaySuccessMessage("Keyword \"{0}\" added successfully to hide articles.", keyword);
                return;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    DisplayWarningMessage("Keyword \"{0}\" already exists or is invalid.", keyword);
                    break;
                default:
                    DisplayErrorMessage("Failed to add keyword. Status code: {0}", (int)response.StatusCode);
                    break;
            }
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
