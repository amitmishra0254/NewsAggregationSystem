using NewsAggregationSystem.Client.Services.Interfaces;
using NewsAggregationSystem.Common.DTOs;
using NewsAggregationSystem.Common.DTOs.Users;
using NewsAggregationSystem.Common.Utilities;
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
        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public ReportService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task ReportNewsArticle(int articleId)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                var reason = InputHelper.ReadString("Enter the reason for reporting this article: ");

                var reportDto = new ReportRequestDTO
                {
                    ArticleId = articleId,
                    Reason = reason
                };

                var response = await httpClient.PostAsJsonAsync("Reports", reportDto);

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine($"[green]Article with ID {articleId} reported successfully.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest ||
                         response.StatusCode == HttpStatusCode.NotFound)
                {
                    var message = await response.Content.ReadAsStringAsync();
                    AnsiConsole.MarkupLine($"[yellow]{message}[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Failed to report article. Status code: {(int)response.StatusCode} - {response.ReasonPhrase}[/]");
                }
            }
            catch (Exception exception)
            {
                AnsiConsole.MarkupLine($"[red]Error occurred while reporting the article: {exception.Message}[/]");
            }
        }

    }
}
