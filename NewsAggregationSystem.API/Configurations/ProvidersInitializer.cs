using NewsAggregationSystem.Common.Providers.SmtpProvider;

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
