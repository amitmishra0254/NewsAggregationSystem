using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.Common.DTOs.Users;
using NewsAggregationSystem.Common.Enums;
using Spectre.Console;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Services.Articles
{
    public class ArticleService : IArticleService
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public ArticleService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<ArticleDTO>> GetSavedArticles()
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", UserState.AccessToken);
                var response = await httpClient.GetAsync(ApplicationConstants.SavedArticlesPath);
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var articles = JsonSerializer.Deserialize<List<ArticleDTO>>(responseString, options);
                    return articles;
                }
                AnsiConsole.MarkupLine($"[red]Failed to retrieve saved articles. Status code: {(int)response.StatusCode}[/]");
            }
            catch (Exception exception)
            {
                AnsiConsole.MarkupLine($"[red]Error occurred while fetching saved articles: {exception.Message}[/]");
            }
            return new();
        }

        public async Task<List<ArticleDTO>> GetAllArticles(NewsArticleRequestDTO request)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                string queryString = BuildQueryString(request);

                string url = $"Articles{queryString}";

                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<ArticleDTO>>(content, options);
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Failed to fetch articles. Status Code: {(int)response.StatusCode}[/]");
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    AnsiConsole.WriteLine($"Message: {errorMessage}");
                }
            }
            catch (Exception exception)
            {
                AnsiConsole.MarkupLine($"[red]Error occurred while fetching articles: {exception.Message}[/]");
            }
            return new();
        }

        public async Task SaveArticle(int articleId)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                var response = await httpClient.PostAsync($"Articles/save-article/{articleId}", null);

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine($"[green]Article saved successfully.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    AnsiConsole.MarkupLine($"[yellow]Article with ID {articleId} not found.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    AnsiConsole.MarkupLine("[yellow]Article is already saved.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Failed to save article. Status code: {(int)response.StatusCode}[/]");
                }
            }
            catch (Exception exception)
            {
                AnsiConsole.MarkupLine($"[red]Error occurred while saving article: {exception.Message}[/]");
            }
        }

        public async Task ReactArticle(int articleId, int reactionId)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                var url = $"Articles/{articleId}/react-article{reactionId}";
                var response = await httpClient.PostAsync(url, null);

                string reactionName = Enum.IsDefined(typeof(ReactionType), reactionId)
                    ? Enum.GetName(typeof(ReactionType), reactionId)
                    : "Reacted";

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine($"[green]Article {reactionName} successfully.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    AnsiConsole.MarkupLine($"[yellow]Article with ID {articleId} not found.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    AnsiConsole.MarkupLine("[yellow]You have already reacted to this article.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Failed to react to article. Status code: {(int)response.StatusCode}[/]");
                }
            }
            catch (Exception exception)
            {
                AnsiConsole.MarkupLine($"[red]Error occurred while reacting to article: {exception.Message}[/]");
            }
        }

        public async Task DeleteSavedArticle(int articleId)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                var response = await httpClient.DeleteAsync($"Articles/delete-saved-article/{articleId}");

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine("[green]Article removed from saved list successfully.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    AnsiConsole.MarkupLine($"[yellow]Article with ID {articleId} not found.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    AnsiConsole.MarkupLine("[yellow]Article is not saved. Save it before trying to delete it.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Failed to delete saved article. Status code: {(int)response.StatusCode}[/]");
                }
            }
            catch (Exception exception)
            {
                AnsiConsole.MarkupLine($"[red]Error occurred while deleting saved article: {exception.Message}[/]");
            }
        }

        public async Task ToggleArticleVisibility(int articleId, bool isHidden)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                string url = $"Articles/change-status?articleId={articleId}&IsHidden={isHidden.ToString().ToLower()}";

                var response = await httpClient.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    AnsiConsole.MarkupLine($"[green]Article ID {articleId} is now {(isHidden ? "hidden" : "visible")}.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    AnsiConsole.MarkupLine($"[yellow]Article is already {(isHidden ? "hidden" : "visible")}.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    AnsiConsole.MarkupLine("[yellow]Article not found.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Failed to update article visibility. Status code: {(int)response.StatusCode}[/]");
                }
            }
            catch (Exception exception)
            {
                AnsiConsole.MarkupLine($"[red]Error occurred while toggling article visibility: {exception.Message}[/]");
            }
        }

        public async Task<ArticleDTO?> GetArticleById(int articleId)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", UserState.AccessToken);

                var response = await httpClient.GetAsync($"Articles/{articleId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ArticleDTO>(json, options);
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    AnsiConsole.MarkupLine($"[yellow]Article with ID {articleId} not found.[/]");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    AnsiConsole.MarkupLine("[red]Unauthorized: Please login as a valid user.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]Error: {response.StatusCode} - {response.ReasonPhrase}[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error occurred while fetching the article: {ex.Message}[/]");
            }
            return null;
        }


        private string BuildQueryString(NewsArticleRequestDTO request)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.SearchText))
                queryParams.Add($"SearchText={Uri.EscapeDataString(request.SearchText)}");

            if (request.IsRequestedForToday)
                queryParams.Add($"IsRequestedForToday={request.IsRequestedForToday}");

            if (request.FromDate.HasValue)
                queryParams.Add($"FromDate={request.FromDate.Value:yyyy-MM-dd}");

            if (request.ToDate.HasValue)
                queryParams.Add($"ToDate={request.ToDate.Value:yyyy-MM-dd}");

            if (request.CategoryId.HasValue)
                queryParams.Add($"CategoryId={request.CategoryId.Value}");

            if (!string.IsNullOrWhiteSpace(request.SortBy))
                queryParams.Add($"SortBy={Uri.EscapeDataString(request.SortBy)}");

            return queryParams.Any() ? "?" + string.Join("&", queryParams) : string.Empty;
        }

    }
}
