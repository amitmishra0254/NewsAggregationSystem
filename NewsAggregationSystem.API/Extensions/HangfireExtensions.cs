using Hangfire;
using NewsAggregationSystem.API.Scheduler;

namespace NewsAggregationSystem.API.Extensions
{
    public static class HangfireExtensions
    {
        public static void RegisterRecurringJobs(this IServiceProvider serviceProvider)
        {
            RecurringJob.AddOrUpdate<NewsFetchScheduler>(
                "fetch-news-job",
                scheduler => scheduler.ExecuteAsync(),
                Cron.DayInterval(1) // every 3 hours
            );
        }
    }

}
