using NewsAggregationSystem.API.Adapters;
using NewsAggregationSystem.API.Adapters.Interfaces;
using NewsAggregationSystem.API.Scheduler;
using NewsAggregationSystem.Service.Interfaces;
using NewsAggregationSystem.Service.Services;

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
            serviceCollection.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();
            serviceCollection.AddScoped<INotificationService, NotificationService>();
            serviceCollection.AddScoped<IUserService, UserService>();
            serviceCollection.AddScoped<IHiddenArticleKeywordService, HiddenArticleKeywordService>();
            serviceCollection.AddScoped<IReportService, ReportService>();
            serviceCollection.AddScoped<NewsFetchScheduler>();
            serviceCollection.AddHttpClient();
        }
    }
}
