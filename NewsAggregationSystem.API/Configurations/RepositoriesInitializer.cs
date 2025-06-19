using NewsAggregationSystem.DAL.Repositories.Articles;
using NewsAggregationSystem.DAL.Repositories.Generic;
using NewsAggregationSystem.DAL.Repositories.NewsCategories;
using NewsAggregationSystem.DAL.Repositories.NewsSources;
using NewsAggregationSystem.DAL.Repositories.NotificationPreferences;
using NewsAggregationSystem.DAL.Repositories.Notifications;
using NewsAggregationSystem.DAL.Repositories.Users;

namespace NewsAggregationSystem.API.Configurations
{
    public static class RepositoriesInitializer
    {
        public static void RegisterRepositories(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
            serviceCollection.AddScoped<INewsSourceRepository, NewsSourceRepository>();
            serviceCollection.AddScoped<IArticleRepository, ArticleRepository>();
            serviceCollection.AddScoped<INewsSourceRepository, NewsSourceRepository>();
            serviceCollection.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();
            serviceCollection.AddScoped<INotificationRepository, NotificationRepository>();
            serviceCollection.AddScoped<IUserRepository, UserRepository>();
            serviceCollection.AddScoped<INewsCategoryRepository, NewsCategoryRepository>();
        }
    }
}
