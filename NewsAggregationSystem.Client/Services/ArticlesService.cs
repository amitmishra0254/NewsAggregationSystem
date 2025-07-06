using NewsAggregationSystem.Client.Services.Interfaces;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.Common.DTOs.Users;
using NewsAggregationSystem.Common.Enums;
using Spectre.Console;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Services
{
    public class ArticleService : IArticleService
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;

        public ArticleService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<List<ArticleDTO>> GetUserSavedArticlesAsync()
        {
            try
            {
                SetAuthorizationHeader();
                var response = await httpClient.GetAsync(ApplicationConstants.SavedArticlesPath);
                return await HandleGetArticlesResponseAsync(response);
            }
            catch (Exception exception)
            {
                DisplayErrorMessage(ApplicationConstants.LogMessage.ErrorFetchingSavedArticles, exception.Message);
                return new List<ArticleDTO>();
            }
        }

        public async Task<List<ArticleDTO>> GetUserArticlesAsync(NewsArticleRequestDTO request)
        {
            try
            {
                SetAuthorizationHeader();
                var queryString = BuildQueryString(request);
                var url = $"Articles{queryString}";
                var response = await httpClient.GetAsync(url);
                return await HandleGetArticlesResponseAsync(response);
            }
            catch (Exception exception)
            {
                DisplayErrorMessage(ApplicationConstants.LogMessage.ErrorFetchingArticles, exception.Message);
                return new List<ArticleDTO>();
            }
        }

        public async Task SaveArticleAsync(int articleId)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await httpClient.PostAsync($"Articles/save-article/{articleId}", null);
                await HandleSaveArticleResponseAsync(response, articleId);
            }
            catch (Exception exception)
            {
                DisplayErrorMessage(ApplicationConstants.LogMessage.ErrorSavingArticle, exception.Message);
            }
        }

        public async Task ReactToArticleAsync(int articleId, int reactionId)
        {
            try
            {
                SetAuthorizationHeader();
                var url = $"Articles/{articleId}/react-article{reactionId}";
                var response = await httpClient.PostAsync(url, null);
                await HandleReactToArticleResponseAsync(response, articleId, reactionId);
            }
            catch (Exception exception)
            {
                DisplayErrorMessage(ApplicationConstants.LogMessage.ErrorReactingToArticle, exception.Message);
            }
        }

        public async Task RemoveSavedArticleAsync(int articleId)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await httpClient.DeleteAsync($"Articles/delete-saved-article/{articleId}");
                await HandleRemoveSavedArticleResponseAsync(response, articleId);
            }
            catch (Exception exception)
            {
                DisplayErrorMessage(ApplicationConstants.LogMessage.ErrorDeletingSavedArticle, exception.Message);
            }
        }

        public async Task ToggleArticleVisibilityAsync(int articleId, bool isHidden)
        {
            try
            {
                SetAuthorizationHeader();
                var url = $"Articles/change-status?articleId={articleId}&IsHidden={isHidden.ToString().ToLower()}";
                var response = await httpClient.PostAsync(url, null);
                await HandleToggleArticleVisibilityResponseAsync(response, articleId, isHidden);
            }
            catch (Exception exception)
            {
                DisplayErrorMessage(ApplicationConstants.LogMessage.ErrorTogglingArticleVisibility, exception.Message);
            }
        }

        public async Task<ArticleDTO?> GetArticleByIdAsync(int articleId)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await httpClient.GetAsync($"Articles/{articleId}");
                return await HandleGetArticleByIdResponseAsync(response, articleId);
            }
            catch (Exception ex)
            {
                DisplayErrorMessage(ApplicationConstants.LogMessage.ErrorFetchingArticle, ex.Message);
                return null;
            }
        }

        private void SetAuthorizationHeader()
        {
            httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", UserState.AccessToken);
        }

        private async Task<List<ArticleDTO>> HandleGetArticlesResponseAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ArticleDTO>>(content, jsonOptions) ?? new List<ArticleDTO>();
            }

            DisplayErrorMessage(ApplicationConstants.LogMessage.FailedToFetchArticles, (int)response.StatusCode);
            var errorMessage = await response.Content.ReadAsStringAsync();
            DisplayErrorMessage($"Message: {errorMessage}");
            return new List<ArticleDTO>();
        }

        private async Task HandleSaveArticleResponseAsync(HttpResponseMessage response, int articleId)
        {
            if (response.IsSuccessStatusCode)
            {
                DisplaySuccessMessage(ApplicationConstants.LogMessage.ClientArticleSavedSuccessfully);
                return;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    DisplayWarningMessage(ApplicationConstants.LogMessage.ClientArticleNotFound, articleId);
                    break;
                case HttpStatusCode.BadRequest:
                    DisplayWarningMessage(ApplicationConstants.LogMessage.ArticleAlreadySaved);
                    break;
                default:
                    DisplayErrorMessage(ApplicationConstants.LogMessage.FailedToSaveArticle, (int)response.StatusCode);
                    break;
            }
        }

        private async Task HandleReactToArticleResponseAsync(HttpResponseMessage response, int articleId, int reactionId)
        {
            var reactionName = GetReactionName(reactionId);

            if (response.IsSuccessStatusCode)
            {
                DisplaySuccessMessage(ApplicationConstants.LogMessage.ClientArticleReactedSuccessfully, reactionName);
                return;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    DisplayWarningMessage(ApplicationConstants.LogMessage.ClientArticleNotFound, articleId);
                    break;
                case HttpStatusCode.BadRequest:
                    DisplayWarningMessage(ApplicationConstants.LogMessage.AlreadyReactedToArticle);
                    break;
                default:
                    DisplayErrorMessage(ApplicationConstants.LogMessage.FailedToReactToArticle, (int)response.StatusCode);
                    break;
            }
        }

        private async Task HandleRemoveSavedArticleResponseAsync(HttpResponseMessage response, int articleId)
        {
            if (response.IsSuccessStatusCode)
            {
                DisplaySuccessMessage(ApplicationConstants.LogMessage.ArticleRemovedFromSaved);
                return;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    DisplayWarningMessage(ApplicationConstants.LogMessage.ClientArticleNotFound, articleId);
                    break;
                case HttpStatusCode.BadRequest:
                    DisplayWarningMessage(ApplicationConstants.LogMessage.ArticleNotSaved);
                    break;
                default:
                    DisplayErrorMessage(ApplicationConstants.LogMessage.FailedToDeleteSavedArticle, (int)response.StatusCode);
                    break;
            }
        }

        private async Task HandleToggleArticleVisibilityResponseAsync(HttpResponseMessage response, int articleId, bool isHidden)
        {
            var visibility = isHidden ? "hidden" : "visible";

            if (response.IsSuccessStatusCode)
            {
                DisplaySuccessMessage(ApplicationConstants.LogMessage.ArticleVisibilityUpdated, articleId, visibility);
                return;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    DisplayWarningMessage(ApplicationConstants.LogMessage.ArticleAlreadyInState, visibility);
                    break;
                case HttpStatusCode.NotFound:
                    DisplayWarningMessage(ApplicationConstants.LogMessage.ClientArticleNotFound, articleId);
                    break;
                default:
                    DisplayErrorMessage(ApplicationConstants.LogMessage.FailedToUpdateArticleVisibility, (int)response.StatusCode);
                    break;
            }
        }

        private async Task<ArticleDTO?> HandleGetArticleByIdResponseAsync(HttpResponseMessage response, int articleId)
        {
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ArticleDTO>(json, jsonOptions);
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    DisplayWarningMessage(ApplicationConstants.LogMessage.ClientArticleNotFound, articleId);
                    break;
                case HttpStatusCode.Unauthorized:
                    DisplayErrorMessage(ApplicationConstants.LogMessage.UnauthorizedAccess);
                    break;
                default:
                    DisplayErrorMessage($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    break;
            }

            return null;
        }

        private string GetReactionName(int reactionId)
        {
            return Enum.IsDefined(typeof(ReactionType), reactionId)
                ? Enum.GetName(typeof(ReactionType), reactionId) ?? "Reacted"
                : "Reacted";
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
