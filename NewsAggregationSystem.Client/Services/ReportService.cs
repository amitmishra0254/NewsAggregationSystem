using NewsAggregationSystem.Client.Services.Interfaces;
using NewsAggregationSystem.Common.DTOs;
using NewsAggregationSystem.Common.DTOs.Users;
using Spectre.Console;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Services
{
    public class ReportService : IReportService
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;

        public ReportService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task CreateArticleReportAsync(int articleId, string reason)
        {
            try
            {
                SetAuthorizationHeader();
                var reportDto = CreateReportRequestAsync(articleId, reason);
                var response = await SendReportRequestAsync(reportDto);
                await HandleReportResponseAsync(response, articleId);
            }
            catch (Exception exception)
            {
                DisplayErrorMessage("Error occurred while reporting the article: {0}", exception.Message);
            }
        }

        private void SetAuthorizationHeader()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserState.AccessToken);
        }

        private ReportRequestDTO CreateReportRequestAsync(int articleId, string reason)
        {
            return new ReportRequestDTO
            {
                ArticleId = articleId,
                Reason = reason
            };
        }

        private async Task<HttpResponseMessage> SendReportRequestAsync(ReportRequestDTO reportDto)
        {
            return await httpClient.PostAsJsonAsync("Reports", reportDto);
        }

        private async Task HandleReportResponseAsync(HttpResponseMessage response, int articleId)
        {
            if (response.IsSuccessStatusCode)
            {
                DisplaySuccessMessage("Article with ID {0} reported successfully.", articleId);
                return;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.NotFound:
                    var message = await response.Content.ReadAsStringAsync();
                    DisplayWarningMessage(message);
                    break;
                default:
                    DisplayErrorMessage("Failed to report article. Status code: {0} - {1}", (int)response.StatusCode, response.ReasonPhrase);
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
