using NewsAggregationSystem.API.Services.Articles;
using NewsAggregationSystem.API.Services.Authentication;
using NewsAggregationSystem.API.Services.NewsArticleClassifier;
using NewsAggregationSystem.API.Services.NewsCategories;
using NewsAggregationSystem.API.Services.NewsSources;
using NewsAggregationSystem.API.Services.NewsSources.NewsFetcher;
using NewsAggregationSystem.API.Services.NotificationPreferences;
using NewsAggregationSystem.API.Services.Notifications;
using NewsAggregationSystem.API.Services.Scheduler;
using NewsAggregationSystem.API.Services.Users;

namespace NewsAggregationSystem.API.Configurations
{
    public static class ServicesInitializer
    {
        public static void RegisterServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<INewsSourceService, NewsSourceService>();
            serviceCollection.AddScoped<IArticleService, ArticleService>();
            serviceCollection.AddScoped<IAuthService, AuthService>();
            serviceCollection.AddScoped<ITopicPredictionAdapter, TopicPredictionAdapter>();
            serviceCollection.AddScoped<INewsCategoryService, NewsCategoryService>();
            serviceCollection.AddScoped<INewsApiAdapter, NewsApiAdapter>();
            serviceCollection.AddScoped<INewsApiAdapter, TheNewsApiAdapter>();
            serviceCollection.AddScoped<INewsSourceService, NewsSourceService>();
            serviceCollection.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();
            serviceCollection.AddScoped<INotificationService, NotificationService>();
            serviceCollection.AddScoped<IUserService, UserService>();
            serviceCollection.AddScoped<NewsFetchScheduler>();
            serviceCollection.AddHttpClient();
        }
    }
}
