using NewsAggregationSystem.Client.Services.Admin;
using NewsAggregationSystem.Client.Services.Articles;
using NewsAggregationSystem.Client.Services.NewsCategory;
using NewsAggregationSystem.Client.Services.NewsSources;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.NewsSources;
using NewsAggregationSystem.Common.DTOs.Users;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Utilities;
using Spectre.Console;

namespace NewsAggregationSystem.Client.Factories
{
    public partial class AdminMenuProvider : IMenuProvider
    {
        private readonly INewsSourcesService newsSourcesService;
        private readonly INewsCategoryService newsCategoryService;
        private readonly IArticleService articleService;
        private readonly IAdminService adminService;

        public AdminMenuProvider(HttpClient httpClient)
        {
            newsSourcesService = new NewsSourcesService(httpClient);
            newsCategoryService = new NewsCategoryService(httpClient);
            articleService = new ArticleService(httpClient);
            adminService = new AdminService(httpClient);
        }

        public async Task ShowMenu()
        {
            string input = "";
            do
            {
                Console.Clear();

                input = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Enter your choice: ")
                        .AddChoices(ApplicationConstants.AdminMainMenu));

                switch (input)
                {
                    case ApplicationConstants.CreateExternalServer:
                        await CreateExternalServer();
                        break;

                    case ApplicationConstants.ViewExternalServers:
                        await ViewAllExternalServers();
                        break;

                    case ApplicationConstants.ViewExternalServerDetails:
                        await ViewExternalServerDetails();
                        break;

                    case ApplicationConstants.EditExternalServer:
                        await EditExternalServer();
                        break;

                    case ApplicationConstants.AddNewsCategory:
                        await AddNewsCategory();
                        break;

                    case ApplicationConstants.HideArticlesByCategory:
                        await ToggleNewsCategoryHandler(true);
                        break;

                    case ApplicationConstants.UnhideArticlesByCategory:
                        await ToggleNewsCategoryHandler(false);
                        break;

                    case ApplicationConstants.HideArticlesById:
                        await ToggleArticleVisibilityHandler(true);
                        break;

                    case ApplicationConstants.UnhideArticlesById:
                        await ToggleArticleVisibilityHandler(false);
                        break;

                    case ApplicationConstants.HideArticlesByKeyword:
                        await HideArticlesByNewsKeywordHandler();
                        break;

                    case ApplicationConstants.Logout:
                        UserState.Clear();
                        Console.WriteLine("Logging out...");
                        break;
                }
                AnsiConsole.MarkupLine($"[bold red]{ApplicationConstants.PressAnyKeyToContinue}[/]");
                Console.ReadKey();
            } while (input != ApplicationConstants.Logout && UserState.IsLoggedIn);
        }

        private async Task ToggleNewsCategoryHandler(bool IsHidden)
        {
            var categories = await newsCategoryService.GetAllNewsCategories();
            var categoryMap = categories
                    .Where(category => category.Name != CategoryType.All.ToString())
                    .ToDictionary(category => $"{category.Name} {(category.IsEnabled ? "(Hidden)" : "")}", category => category);

            var categoriesToShow = categoryMap.Keys.ToList();
            categoriesToShow.Add(ApplicationConstants.Back);

            var categoryName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select any Category: ")
                    .AddChoices(categoriesToShow));

            if (categoryName.Equals(ApplicationConstants.Back, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var matchedCategory = categoryMap[categoryName];
            await newsCategoryService.ToggleNewsCategoryVisibility(matchedCategory.Id, IsHidden);
        }

        private async Task ToggleArticleVisibilityHandler(bool IsHidden)
        {
            int articleId = InputHelper.ReadInt("Enter article Id: ");
            await articleService.ToggleArticleVisibility(articleId, IsHidden);
        }

        private async Task HideArticlesByNewsKeywordHandler()
        {
            string keyword = InputHelper.ReadString("Enter the keyword: ");
            await adminService.AddKeywordToHideArticles(keyword);
        }

        private async Task AddNewsCategory()
        {
            string newsCategory = InputHelper.ReadString("Enter new News Category: ");
            await newsCategoryService.AddNewsCategory(newsCategory);
        }

        private async Task EditExternalServer()
        {
            int id = InputHelper.ReadInt("Enter the ID of the external server to update: ");
            string apiKey = InputHelper.ReadString("Enter new API key: ");

            bool isActive = AnsiConsole.Prompt(
                                new SelectionPrompt<string>()
                                    .Title("Is Active?")
                                    .AddChoices("Yes", "No")
                            ) == "Yes";

            var updateNewsSourceDTO = new UpdateNewsSourceDTO
            {
                ApiKey = apiKey,
                IsActive = isActive
            };
            await newsSourcesService.UpdateNewsSource(id, updateNewsSourceDTO);
        }

        private async Task ViewExternalServerDetails()
        {
            var servers = await newsSourcesService.GetAllNewsSource();

            if (servers == null || !servers.Any())
            {
                AnsiConsole.MarkupLine("[red]No external servers found.[/]");
            }
            PrintExternalServers(servers);
        }

        private void PrintExternalServers(List<NewsSourceDTO> servers)
        {
            var table = new Table().Centered();
            table.Border = TableBorder.Rounded;
            table.AddColumn("ID");
            table.AddColumn("Name");
            table.AddColumn("API Key");
            table.AddColumn("Active");
            table.AddColumn("Last Access");

            foreach (var server in servers)
            {
                table.AddRow(
                    server.Id.ToString(),
                    server.Name,
                    server.ApiKey,
                    server.IsActive ? "[green]Yes[/]" : "[red]No[/]",
                    server.LastAccess.ToShortTimeString()
                );
            }

            AnsiConsole.Write(table);
        }

        private async Task ViewAllExternalServers()
        {
            var servers = await newsSourcesService.GetAllNewsSource();

            if (servers == null || !servers.Any())
            {
                AnsiConsole.MarkupLine("[red]No external servers found.[/]");
            }

            var table = new Table()
                .Border(TableBorder.Rounded)
                .Centered();

            table.AddColumn("[bold yellow]ID[/]");
            table.AddColumn("[bold yellow]Name[/]");
            table.AddColumn("[bold yellow]Active[/]");
            table.AddColumn("[bold yellow]Last Access");

            foreach (var server in servers)
            {
                table.AddRow(
                    server.Id.ToString(),
                    server.Name,
                    server.IsActive ? "[green]Yes[/]" : "[red]No[/]",
                    server.LastAccess.ToShortTimeString()
                );
            }

            AnsiConsole.Write(new Markup("[bold green]\n List of External News Servers:[/]\n"));
            AnsiConsole.Write(table);
        }

        private async Task CreateExternalServer()
        {
            AnsiConsole.MarkupLine("[bold underline yellow]Create External News Server[/]");

            string name = InputHelper.ReadString("Enter News Source Name: ");
            string baseUrl = InputHelper.ReadString("Enter Base URL: ");
            string apiKey = InputHelper.ReadString("Enter API Key: ");

            var newsSource = new CreateNewsSourceDTO
            {
                Name = name,
                BaseUrl = baseUrl,
                ApiKey = apiKey
            };

            await newsSourcesService.AddNewsSource(newsSource);
        }

    }
}
