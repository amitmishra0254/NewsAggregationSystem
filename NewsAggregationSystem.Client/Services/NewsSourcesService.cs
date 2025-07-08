using NewsAggregationSystem.Client.Services.Interfaces;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsSources;
using NewsAggregationSystem.Common.DTOs.Users;
using Spectre.Console;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Services
{
    public class NewsSourcesService : INewsSourcesService
    {
        private readonly HttpClient httpClient;

        public NewsSourcesService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task CreateNewsSourceAsync(CreateNewsSourceDTO newsSource)
        {
            try
            {
                var response = await SendCreateNewsSourceRequestAsync(newsSource);
                await HandleCreateNewsSourceResponseAsync(response);
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ApplicationConstants.LogMessage.ErrorAddingNewsSource, ex.Message);
            }
        }

        public async Task<List<NewsSourceDTO>> GetAllNewsSourcesAsync()
        {
            try
            {
                SetAuthorizationHeader();
                var response = await httpClient.GetAsync(ApplicationConstants.AddNewsSource);
                return await HandleGetAllNewsSourcesResponseAsync(response);
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ApplicationConstants.LogMessage.ErrorFetchingNewsSources, ex.Message);
                return new List<NewsSourceDTO>();
            }
        }

        public async Task UpdateNewsSourceAsync(int Id, UpdateNewsSourceDTO updateNewsSourceDTO)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await httpClient.PutAsJsonAsync($"NewsSources/{Id}", updateNewsSourceDTO);
                await HandleUpdateNewsSourceResponseAsync(response, Id);
            }
            catch (Exception exception)
            {
                DisplayErrorMessage(ApplicationConstants.LogMessage.ErrorUpdatingNewsSource, exception.Message);
            }
        }

        private async Task<HttpResponseMessage> SendCreateNewsSourceRequestAsync(CreateNewsSourceDTO newsSource)
        {
            var json = JsonSerializer.Serialize(newsSource);
            var content = CreateJsonContent(json);
            var request = new HttpRequestMessage(HttpMethod.Post, ApplicationConstants.AddNewsSource)
            {
                Content = content
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", UserState.AccessToken);
            return await httpClient.SendAsync(request);
        }

        private StringContent CreateJsonContent(string json)
        {
            return new StringContent(json, Encoding.UTF8, ApplicationConstants.JsonContentType);
        }

        private void SetAuthorizationHeader()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserState.AccessToken);
        }

        private async Task HandleCreateNewsSourceResponseAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                DisplaySuccessMessage(ApplicationConstants.LogMessage.ExternalServerCreatedSuccessfully);
                return;
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            DisplayErrorMessage(ApplicationConstants.LogMessage.AddNewsSourceFailed, errorMessage);
        }

        private async Task<List<NewsSourceDTO>> HandleGetAllNewsSourcesResponseAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var newsSources = await response.Content.ReadFromJsonAsync<List<NewsSourceDTO>>();
                return newsSources ?? new List<NewsSourceDTO>();
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            DisplayErrorMessage(ApplicationConstants.LogMessage.GetAllNewsSourcesFailed, errorMessage);
            return new List<NewsSourceDTO>();
        }

        private async Task HandleUpdateNewsSourceResponseAsync(HttpResponseMessage response, int Id)
        {
            if (response.IsSuccessStatusCode)
            {
                DisplaySuccessMessage(ApplicationConstants.LogMessage.ExternalServerUpdatedSuccessfully);
                return;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    DisplayWarningMessage(ApplicationConstants.LogMessage.ExternalServerNotFound, Id);
                    break;
                default:
                    DisplayErrorMessage(ApplicationConstants.LogMessage.FailedToUpdateExternalServer, (int)response.StatusCode);
                    break;
            }
        }

        private void DisplaySuccessMessage(string message, params object[] args)
        {
            var formattedMessage = string.Format(message, args);
            AnsiConsole.MarkupLine($"[bold green]{formattedMessage}[/]");
        }

        private void DisplayWarningMessage(string message, params object[] args)
        {
            var formattedMessage = string.Format(message, args);
            AnsiConsole.MarkupLine($"[green]{formattedMessage}[/]");
        }

        private void DisplayErrorMessage(string message, params object[] args)
        {
            var formattedMessage = string.Format(message, args);
            AnsiConsole.MarkupLine($"[red]{formattedMessage}[/]");
        }
    }
}
