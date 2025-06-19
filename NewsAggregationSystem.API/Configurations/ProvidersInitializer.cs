using NewsAggregationSystem.Common.Providers.SmtpProvider;
using NewsAggregationSystem.DAL.Repositories.Articles;
using NewsAggregationSystem.DAL.Repositories.Generic;
using NewsAggregationSystem.DAL.Repositories.NewsCategories;
using NewsAggregationSystem.DAL.Repositories.NewsSources;
using NewsAggregationSystem.DAL.Repositories.NotificationPreferences;
using NewsAggregationSystem.DAL.Repositories.Notifications;
using NewsAggregationSystem.DAL.Repositories.Users;

namespace NewsAggregationSystem.API.Configurations
{
    public static class ProvidersInitializer
    {
        public static void RegisterProviders(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IEmailProvider, EmailProvider>();
        }
    }
}
