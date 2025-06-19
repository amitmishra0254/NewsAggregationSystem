using NewsAggregationSystem.API.Services.NewsSources.NewsFetcher;
using NewsAggregationSystem.API.Services.NotificationPreferences;
using NewsAggregationSystem.API.Services.Notifications;
using NewsAggregationSystem.API.Services.Users;
using NewsAggregationSystem.Common.Providers.SmtpProvider;

namespace NewsAggregationSystem.API.Services.Scheduler
{
    public class NewsFetchScheduler
    {
        private readonly IEnumerable<INewsApiAdapter> adapters;
        private readonly ILogger<NewsFetchScheduler> logger;
        private readonly INotificationService notificationService;
        private readonly IUserService userService;
        private readonly IEmailProvider emailProvider;
        private readonly INotificationPreferenceService notificationPreferenceService;

        public NewsFetchScheduler(IEnumerable<INewsApiAdapter> adapters,
                                  INotificationService notificationService,
                                  INotificationPreferenceService notificationPreferenceService,
                                  IUserService userService,
                                  IEmailProvider emailProvider,
                                  ILogger<NewsFetchScheduler> logger)
        {
            this.adapters = adapters;
            this.notificationService = notificationService;
            this.logger = logger;
            this.emailProvider = emailProvider;
            this.userService = userService;
            this.notificationPreferenceService = notificationPreferenceService;
        }

        public async Task ExecuteAsync()
        {
            foreach (var adapter in adapters)
            {
                try
                {
                    var articles = await adapter.FetchNewsAsync();

                    if (articles.Any())
                    {
                        var userPreferences = await notificationPreferenceService.GetNotificationPreferences(new List<int>());
                        var notifications = notificationService.GenerateNotificationsFromUserPreferences(articles, userPreferences);
                        var emails = await userService.GenerateNotificationEmails(notifications);
                        await notificationService.AddNotifications(notifications);
                        await emailProvider.SendBulkEmail(emails);
                        logger.LogInformation($"Fetched {articles.Count} articles using {adapter.GetType().Name}");
                        logger.LogInformation($"Successfully sent {notifications.Count} in app notifications and Emails using with {articles.Count} articles.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Adapter {adapter.GetType().Name} failed. Marking as inactive.");
                }
            }
            logger.LogWarning($"We do not have any Active server to fetch the news Articles.");
        }
    }
}
