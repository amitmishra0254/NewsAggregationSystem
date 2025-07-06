using NewsAggregationSystem.Client.Services;
using NewsAggregationSystem.Client.Services.Interfaces;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsArticles;
using NewsAggregationSystem.Common.DTOs.NewsCategories;
using NewsAggregationSystem.Common.DTOs.NotificationPreferences;
using NewsAggregationSystem.Common.DTOs.Users;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Utilities;
using Spectre.Console;

namespace NewsAggregationSystem.Client.Factories
{
    public class UserMenuProvider : IMenuProvider
    {
        private readonly IArticleService articleService;
        private readonly INotificationPreferenceService notificationPreferenceService;
        private readonly INotificationServices notificationServices;
        private readonly IReportService reportService;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();

        public UserMenuProvider(HttpClient httpClient)
        {
            articleService = new ArticleService(httpClient);
            notificationPreferenceService = new NotificationPreferenceService(httpClient);
            notificationServices = new NotificationServices(httpClient);
            reportService = new ReportService(httpClient);
        }

        public async Task ShowMenu()
        {
            string input = "";
            do
            {
                Console.Clear();
                PrintWelcomeMessage();
                input = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Enter your choice: ")
                        .AddChoices(ApplicationConstants.UserWelcomeMenu));

                switch (input)
                {
                    case ApplicationConstants.Headlines:
                        await ShowHeadlines();
                        break;

                    case ApplicationConstants.SavedArticles:
                        await ShowSavedArticles();
                        break;

                    case ApplicationConstants.Search:
                        await SearchArticles();
                        break;

                    case ApplicationConstants.Notifications:
                        await NotificationHandler();
                        break;

                    case ApplicationConstants.Logout:
                        AnsiConsole.MarkupLine($"[bold red]Loggind out...[/]");
                        Thread.Sleep(2000);
                        UserState.Clear();
                        break;
                }
                if (input != ApplicationConstants.Notifications && input != ApplicationConstants.Headlines && input != ApplicationConstants.Logout)
                {
                    AnsiConsole.MarkupLine($"[bold red]{ApplicationConstants.PressAnyKeyToContinue}[/]");
                    Console.ReadKey();
                }
            }
            while (input != ApplicationConstants.Logout && UserState.IsLoggedIn);
        }

        private void PrintWelcomeMessage()
        {
            var welcomeMessage = string.Format(
                                        ApplicationConstants.WelcomeMessageOnUserLogin,
                                        UserState.UserName,
                                        dateTimeHelper.GetCurrentSystemDateTime.ToShortDateString(),
                                        dateTimeHelper.GetCurrentSystemDateTime.ToShortTimeString()
                                    );

            AnsiConsole.MarkupLine($"[bold green]{welcomeMessage}[/]");
        }

        private async Task NotificationHandler()
        {
            string input = "";

            do
            {
                Console.Clear();
                PrintWelcomeMessage();

                input = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Enter your choice: ")
                        .AddChoices(ApplicationConstants.NotificationMenu));

                switch (input)
                {
                    case ApplicationConstants.View:
                        await ShowNotifications();
                        break;

                    case ApplicationConstants.Configure:
                        await ShowNotificationConfiguration();
                        break;

                    case ApplicationConstants.Back:
                        break;
                    case ApplicationConstants.Logout:
                        AnsiConsole.MarkupLine($"[bold red]Logging out ...[/]");
                        Thread.Sleep(2000);
                        UserState.Clear();
                        break;
                }
                AnsiConsole.MarkupLine($"[bold red]{ApplicationConstants.PressAnyKeyToContinue}[/]");
                Console.ReadKey();

            } while (input != ApplicationConstants.Back && UserState.IsLoggedIn);
        }

        private async Task ShowNotifications()
        {
            Console.Clear();
            AnsiConsole.MarkupLine($"[bold blue]{ApplicationConstants.Notification}[/]");

            var notifications = await notificationServices.GetAllNotifications();

            if (notifications == null || !notifications.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No notifications found.[/]");
                return;
            }

            var table = new Table();
            table.Border = TableBorder.Rounded;
            table.AddColumn("[bold]ID[/]");
            table.AddColumn("[bold]Title[/]");
            table.AddColumn("[bold]Message[/]");

            foreach (var notification in notifications)
            {
                table.AddRow(
                    notification.Id.ToString(),
                    notification.Title ?? "[grey]N/A[/]",
                    notification.Message ?? "[grey]N/A[/]"
                );
            }

            AnsiConsole.Write(table);
        }

        private async Task SearchArticles()
        {
            var searchText = AnsiConsole.Ask<string>("[bold yellow]Please enter the search text:[/]");
            if (string.IsNullOrWhiteSpace(searchText))
            {
                AnsiConsole.MarkupLine("[red]Invalid input. Search text cannot be empty.[/]");
                return;
            }
            else
            {
                var request = new NewsArticleRequestDTO
                {
                    IsRequestedForToday = true,
                    CategoryId = null,
                    FromDate = null,
                    ToDate = null,
                    SearchText = searchText,
                    SortBy = null,
                };
                await GetHeadlines(request);
            }
        }

        public async Task ShowNotificationConfiguration()
        {
            Console.Clear();
            AnsiConsole.MarkupLine("[bold underline green]Notification Configuration[/]");
            var configurations = await notificationPreferenceService.GetUserNotificationPreferences();

            if (configurations == null || !configurations.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No notification preferences found.[/]");
                return;
            }

            var category = DisplayCategories(configurations.First().NewsCategories);

            if (category.CategoryId != 0)
            {
                await HandleCategorySelection(category);
            }
        }

        private NewsCategoryDTO DisplayCategories(List<NewsCategoryDTO> categories)
        {
            if (categories == null || !categories.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No categories available.[/]");
                return null;
            }

            var categoryDisplayMap = categories.Where(category => category.Name != CategoryType.All.ToString()).ToDictionary(
                category =>
                {
                    var keywords = (category.Keywords != null && category.Keywords.Any())
                        ? string.Join(", ", category.Keywords.Select(k => $"{k.Name} ({(k.IsEnabled ? "Enabled" : "Disabled")})"))
                        : "None";

                    var categoryStatus = category.IsEnabled ? "Enabled" : "Disabled";

                    return $"{category.Name} ({categoryStatus}) -> {keywords}";
                },
                category => category);

            var categoriesToShow = categoryDisplayMap.Keys.ToList();
            categoriesToShow.Add(ApplicationConstants.Back);

            var categorySelection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a category:")
                    .AddChoices(categoriesToShow));

            if (categorySelection != ApplicationConstants.Back)
            {
                return categoryDisplayMap[categorySelection];
            }
            return new();
        }

        private NotificationPreferencesKeywordDTO DisplayKeywords(NewsCategoryDTO category)
        {
            if (category == null)
            {
                AnsiConsole.MarkupLine("[yellow]No categories available.[/]");
                return null;
            }

            var keywordDisplayMap = category.Keywords.ToDictionary(
                keyword =>
                {
                    var keywordStatus = keyword.IsEnabled ? "Enabled" : "Disabled";
                    return $"{keyword.Name} ({keywordStatus})";
                },
                keyword => keyword);

            var keywordsToDisplay = keywordDisplayMap.Keys.ToList();
            keywordsToDisplay.Add(ApplicationConstants.Back);

            var keywordSelection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a keyword:")
                    .AddChoices(keywordsToDisplay));

            if (keywordSelection != ApplicationConstants.Back)
            {
                return keywordDisplayMap[keywordSelection];
            }
            return new();
        }

        private async Task HandleCategorySelection(NewsCategoryDTO newsCategory)
        {
            if (newsCategory == null)
            {
                return;
            }

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What do you want to do?")
                    .AddChoices(ApplicationConstants.PreferenceConfigurationMenu));

            switch (choice)
            {
                case ApplicationConstants.EnableCategoryStatus:
                    await ToggleCategoryStatus(newsCategory, true);
                    break;
                case ApplicationConstants.DisableCategoryStatus:
                    await ToggleCategoryStatus(newsCategory, false);
                    break;
                case ApplicationConstants.AddKeywordToCategory:
                    await AddKeywordToCategory(newsCategory);
                    break;
                case ApplicationConstants.ConfigureKeywords:
                    await KeywordConfigurationHandler(newsCategory);
                    break;
            }
        }

        public async Task KeywordConfigurationHandler(NewsCategoryDTO category)
        {
            var keyword = DisplayKeywords(category);

            if (keyword.Id != 0)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Please select a Keyword?")
                        .AddChoices(ApplicationConstants.PreferenceKeywordConfiguration));

                AnsiConsole.MarkupLine($"[bold underline green]Category: {category.Name}[/]");
                switch (choice)
                {
                    case ApplicationConstants.EnableKeywordStatus:
                        await ToggleKeywordStatus(keyword, true);
                        break;
                    case ApplicationConstants.DisableKeywordStatus:
                        await ToggleKeywordStatus(keyword, false);
                        break;
                }
            }
        }

        private async Task ToggleCategoryStatus(NewsCategoryDTO category, bool categoryStatus)
        {
            if (category.IsEnabled == categoryStatus)
            {
                AnsiConsole.MarkupLine($"[yellow]Category is already {(categoryStatus ? "[green]Enabled[/]" : "[red]Disabled[/]")}.[/]");
                return;
            }
            await notificationPreferenceService.ChangeCategoryStatus(category.CategoryId, categoryStatus);
        }

        private async Task ToggleKeywordStatus(NotificationPreferencesKeywordDTO keyword, bool keywordStatus)
        {
            if (keyword.IsEnabled == keywordStatus)
            {
                AnsiConsole.MarkupLine($"[yellow]Keyword is already {(keywordStatus ? "[green]Enabled[/]" : "[red]Disabled[/]")}.[/]");
                return;
            }
            if (keyword.Id != 0)
            {
                await notificationPreferenceService.ChangeKeywordStatus(keyword.Id, keywordStatus);
            }
        }

        private async Task AddKeywordToCategory(NewsCategoryDTO category)
        {
            var keywordName = InputHelper.ReadString("Enter the new keyword name: ");

            if (string.IsNullOrWhiteSpace(keywordName))
            {
                AnsiConsole.MarkupLine("[red]Invalid keyword name. No keyword added.[/]");
                return;
            }

            await notificationPreferenceService.AddKeyword(keywordName, category.CategoryId);
        }

        private async Task ShowHeadlines()
        {
            string input = "";
            do
            {
                Console.Clear();
                input = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("What do you want to do?")
                        .AddChoices(ApplicationConstants.HeadlinesSubMenuOptions));

                PrintWelcomeMessage();

                switch (input)
                {
                    case ApplicationConstants.HeadlineToday:
                        Console.Clear();
                        AnsiConsole.MarkupLine("[green]Fetching today's headlines...[/]");
                        await GetHeadlines(new NewsArticleRequestDTO
                        {
                            IsRequestedForToday = true,
                        });
                        break;

                    case ApplicationConstants.HeadlineDateRange:
                        await FetchHeadlinesByDateRangeHandler();
                        break;

                    case ApplicationConstants.Logout:
                        AnsiConsole.MarkupLine($"[bold red]Logging out...[/]");
                        Thread.Sleep(2000);
                        UserState.Clear();
                        break;

                    case ApplicationConstants.Back:
                        break;
                }
                AnsiConsole.MarkupLine($"[bold red]{ApplicationConstants.PressAnyKeyToContinue}[/]");
                Console.ReadKey();
            }
            while (input != ApplicationConstants.Logout && UserState.IsLoggedIn && input != ApplicationConstants.Back);
        }

        private async Task FetchHeadlinesByDateRangeHandler()
        {
            var validDates = dateTimeHelper.DateValidator();
            var preferences = await notificationPreferenceService.GetUserNotificationPreferences();

            if (validDates == null || preferences == null || !preferences.Any())
            {
                AnsiConsole.MarkupLine("[red]Invalid input or no category preferences found.[/]");
                return;
            }

            var selectedCategoryId = await PrintCategory(preferences.First());

            if (selectedCategoryId == -1)
            {
                return;
            }

            Console.Clear();
            AnsiConsole.MarkupLine($"[green]Fetching headlines from {validDates.FromDate:dd MMM yyyy} to {validDates.ToDate:dd MMM yyyy}...[/]");

            await GetHeadlines(new NewsArticleRequestDTO
            {
                IsRequestedForToday = false,
                CategoryId = selectedCategoryId,
                FromDate = validDates.FromDate,
                ToDate = validDates.ToDate
            });
        }


        private async Task GetHeadlines(NewsArticleRequestDTO request)
        {
            var response = await articleService.GetAllArticles(request);
            if (response.Any())
            {
                PrintArticles(response);
                await HeadlinesSubOperationHandler();
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]No articles found for the given criteria.[/]");
            }
        }

        private async Task HeadlinesSubOperationHandler()
        {
            string input = "";
            int articleId = 0;
            input = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What do you want to do?")
                    .AddChoices(ApplicationConstants.HeadlineSubMenuOptions));

            if (input == ApplicationConstants.HeadlineSaveArticle ||
                input == ApplicationConstants.HeadlineLikeArticle ||
                input == ApplicationConstants.HeadlineDislikeArticle ||
                input == ApplicationConstants.HeadlineReportArticle ||
                input == ApplicationConstants.HeadlineReadArticle)
            {
                articleId = InputHelper.ReadInt("Enter Article Id: ");
            }

            switch (input)
            {
                case ApplicationConstants.HeadlineSaveArticle:
                    await articleService.SaveArticle(articleId);
                    break;
                case ApplicationConstants.HeadlineLikeArticle:
                    await articleService.ReactArticle(articleId, (int)ReactionType.Like);
                    break;
                case ApplicationConstants.HeadlineDislikeArticle:
                    await articleService.ReactArticle(articleId, (int)ReactionType.Dislike);
                    break;
                case ApplicationConstants.HeadlineReportArticle:
                    await reportService.ReportNewsArticle(articleId);
                    break;
                case ApplicationConstants.HeadlineReadArticle:
                    await ReadArticleHandler(articleId);
                    break;
                case ApplicationConstants.Back:
                    break;
            }
        }

        private async Task<int?> PrintCategory(NotificationPreferenceDTO notificationPreference)
        {
            if (notificationPreference == null || notificationPreference.NewsCategories == null || !notificationPreference.NewsCategories.Any())
            {
                AnsiConsole.MarkupLine("[red]No categories available to select.[/]");
                return null;
            }
            var categoryMap = notificationPreference.NewsCategories
                    .ToDictionary(category => category.Name, category => category.CategoryId);

            var categories = categoryMap.Keys.ToList();
            categories.Add(ApplicationConstants.Back);

            var selectedCategory = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Please choose a news category:[/]")
                    .AddChoices(categories));

            if (selectedCategory == ApplicationConstants.Back)
            {
                return -1;
            }
            return categoryMap[selectedCategory];
        }

        private async Task ShowSavedArticles()
        {
            Console.Clear();
            var response = await articleService.GetSavedArticles();
            if (response != null)
            {
                PrintArticles(response);
                if (response != null && response.Any())
                {
                    var input = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("What do you want to do?")
                            .AddChoices(ApplicationConstants.Back,
                                ApplicationConstants.DeleteSavedArticle,
                                ApplicationConstants.SortArticlesByLikes));
                    if (input == ApplicationConstants.DeleteSavedArticle)
                    {
                        await DeleteSavedArticleHandler();
                    }
                    else if (input == ApplicationConstants.SortArticlesByLikes)
                    {
                        Console.Clear();
                        var sortedArticle = response.OrderByDescending(article => article.LikedCount)
                                                .ThenByDescending(article => article.SavedCount)
                                                .ToList();
                        PrintArticles(sortedArticle);
                    }
                }
            }
        }

        private async Task DeleteSavedArticleHandler()
        {
            AnsiConsole.MarkupLine("[bold yellow]\nEnter the article ID to delete from saved articles:[/]");
            if (!int.TryParse(Console.ReadLine(), out int articleId))
            {
                AnsiConsole.MarkupLine("[red]Invalid input. Please enter a valid numeric article ID.[/]");
                return;
            }
            await articleService.DeleteSavedArticle(articleId);
        }

        private void PrintArticles(List<ArticleDTO> articles)
        {
            if (articles == null || !articles.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No articles found.[/]");
                return;
            }

            var table = new Table();
            table.Border = TableBorder.Rounded;
            table.Expand();

            table.AddColumn("[bold]Article ID[/]");
            table.AddColumn("[bold]Title[/]");
            table.AddColumn("[bold]Source[/]");
            table.AddColumn("[bold]Category[/]");
            table.AddColumn("[bold]URL[/]");

            foreach (var article in articles)
            {
                table.AddRow(
                    article.Id.ToString(),
                    TrimToLength(article.Title, 70),
                    TrimToLength(article.SourceName, 20),
                    TrimToLength(article.NewsCategoryName, 20),
                    TrimToLength(article.Url, 65)
                );
            }
            AnsiConsole.Write(table);
        }

        public async Task ReadArticleHandler(int articleId)
        {
            var article = await articleService.GetArticleById(articleId);
            if (article != null)
            {
                Console.Clear();
                var table = new Table();
                table.Border = TableBorder.Rounded;
                table.Expand();

                table.AddColumn("[bold]Article ID[/]");
                table.AddColumn("[bold]Title[/]");
                table.AddColumn("[bold]Source[/]");
                table.AddColumn("[bold]Category[/]");
                table.AddColumn("[bold]URL[/]");
                table.AddColumn("[bold]Content[/]");

                table.AddRow(
                    article.Id.ToString(),
                    Markup.Escape(article.Title ?? ""),
                    Markup.Escape(article.SourceName ?? ""),
                    Markup.Escape(article.NewsCategoryName ?? ""),
                    Markup.Escape(article.Url ?? ""),
                    Markup.Escape(article.Content ?? "")
                );
                AnsiConsole.Write(table);
            }
        }

        private string TrimToLength(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input.Length <= maxLength ? input : input.Substring(0, maxLength - 3) + "...";
        }
    }
}
