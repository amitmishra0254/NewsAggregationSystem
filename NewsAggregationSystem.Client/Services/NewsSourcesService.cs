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

        public async Task AddNewsSource(CreateNewsSourceDTO newsSource)
        {
            try
            {
                var json = JsonSerializer.Serialize(newsSource);
                var content = new StringContent(json, Encoding.UTF8, ApplicationConstants.JsonContentType);
                var request = new HttpRequestMessage(HttpMethod.Post, ApplicationConstants.AddNewsSource)
                {
                    Content = content
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine("[bold green] External Server Created Successfully![/]");
                    return;
                }

                AnsiConsole.MarkupLine($"[bold red] Add news source failed: {await response.Content.ReadAsStringAsync()}.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error adding news source: {ex.Message}[/]");
            }
        }

        public async Task<List<NewsSourceDTO>> GetAllNewsSource()
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                var response = await httpClient.GetAsync(ApplicationConstants.AddNewsSource);

                if (response.IsSuccessStatusCode)
                {
                    var newsSources = await response.Content.ReadFromJsonAsync<List<NewsSourceDTO>>();
                    return newsSources;
                }

                AnsiConsole.MarkupLine($"[red]Get all news sources failed: {await response.Content.ReadAsStringAsync()}.[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error while fetching news sources: {ex.Message}[/]");
            }
            return null;
        }

        public async Task UpdateNewsSource(int Id, UpdateNewsSourceDTO updateNewsSourceDTO)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                var response = await httpClient.PutAsJsonAsync($"NewsSources/{Id}", updateNewsSourceDTO);

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine("[green]External server updated successfully.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    AnsiConsole.MarkupLine($"[green]External server with Id {Id} not found.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Failed to update. Status Code: {(int)response.StatusCode}[/]");
                }
            }
            catch (Exception exception)
            {
                AnsiConsole.MarkupLine($"[bold red]Error updating news source: {exception.Message}[/]");
            }
        }
    }
}
