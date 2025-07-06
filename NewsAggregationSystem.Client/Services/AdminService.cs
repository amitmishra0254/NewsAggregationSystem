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
        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public AdminService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task AddKeywordToHideArticles(string keyword)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                var url = $"Admin/hide-article-by-keyword?keyword={Uri.EscapeDataString(keyword)}";

                var response = await httpClient.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine($"[green]Keyword \"{keyword}\" added successfully to hide articles.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    AnsiConsole.MarkupLine($"[yellow]Keyword \"{keyword}\" already exists or is invalid.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Failed to add keyword. Status code: {(int)response.StatusCode}[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error occurred while adding keyword: {ex.Message}[/]");
            }
        }
    }
}
