using Hangfire;
using NewsAggregationSystem.API.Services.Scheduler;

namespace NewsAggregationSystem.API.Extensions
{
    public static class HangfireExtensions
    {
        public static void RegisterRecurringJobs(this IServiceProvider serviceProvider)
        {
            RecurringJob.AddOrUpdate<NewsFetchScheduler>(
                "fetch-news-job",
                scheduler => scheduler.ExecuteAsync(),
                Cron.MinuteInterval(59) // every 3 hours
            );
        }
    }

}
