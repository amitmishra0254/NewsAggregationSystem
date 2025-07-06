using Microsoft.Extensions.DependencyInjection;
using NewsAggregationSystem.API.Adapters;
using NewsAggregationSystem.API.Adapters.Interfaces;
using NewsAggregationSystem.API.Adapters.Strategies;

namespace NewsAggregationSystem.API.Configurations
{
    public static class AdaptersInitializer
    {
        public static IServiceCollection RegisterAdapters(this IServiceCollection services)
        {
            services.AddScoped<NewsApiResponseStrategy>();
            services.AddScoped<TheNewsApiResponseStrategy>();

            services.AddScoped<NewsApiAdapter>();
            services.AddScoped<TheNewsApiAdapter>();
            
            services.AddScoped<INewsApiAdapterFactory, NewsApiAdapterFactory>();

            services.AddScoped<INewsAggregationFacade, NewsAggregationFacade>();

            return services;
        }
    }
} 