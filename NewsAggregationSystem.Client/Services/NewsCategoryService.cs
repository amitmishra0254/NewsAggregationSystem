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
        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        public NewsCategoryService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task AddNewsCategory(string category)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                var response = await httpClient.PostAsync(
                    $"{ApplicationConstants.AddNewsCategoryPath}?category={Uri.EscapeDataString(category)}", null);


                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine("[green]News category added successfully.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    AnsiConsole.MarkupLine("[yellow]News category already exists with this name.[/]");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    AnsiConsole.MarkupLine($"[red]Failed to add category. Status Code: {(int)response.StatusCode}[/]");
                    AnsiConsole.WriteLine($"Message: {error}");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error while adding news category: {ex.Message}[/]");
            }
        }


        public async Task ToggleNewsCategoryVisibility(int categoryId, bool isHidden)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                string url = $"NewsCategories/toggle-visibility?categoryId={categoryId}&IsHidden={isHidden}";

                var response = await httpClient.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine($"[green]Category {(isHidden ? "hidden" : "unhidden")} successfully.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    AnsiConsole.MarkupLine("[yellow]Category visibility is already updated.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    AnsiConsole.MarkupLine("[red]Category not found.[/]");
                }
                else
                {
                    var message = await response.Content.ReadAsStringAsync();
                    AnsiConsole.MarkupLine($"[red]Failed to toggle category visibility. Status Code: {(int)response.StatusCode}[/]");
                    AnsiConsole.WriteLine($"Message: {message}");
                }
            }
            catch (Exception exception)
            {
                AnsiConsole.MarkupLine($"[red]Error occurred: {exception.Message}[/]");
            }
        }

        public async Task<List<NotificationPreferencesKeywordDTO>> GetAllNewsCategories()
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                var response = await httpClient.GetAsync("NewsCategories");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var categories = JsonSerializer.Deserialize<List<NotificationPreferencesKeywordDTO>>(responseString, options);

                    return categories;
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Failed to retrieve news categories. Status code: {(int)response.StatusCode}[/]");
                }
            }
            catch (Exception exception)
            {
                AnsiConsole.MarkupLine($"[red]Error occurred while fetching categories: {exception.Message}[/]");
            }

            return null;
        }
    }
}
