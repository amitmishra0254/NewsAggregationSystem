namespace NewsAggregationSystem.Common.Constants
{
    public static class ApplicationConstants
    {
        public static string DatabaseProviderNotConfigured = "Database provider is not configured. Use dependency injection or provide a connection string explicitly.";
        public const string DateTime2With2Precision = "datetime2(2)";
        public const string NewsAggregationSystemDbConnection = "NewsAggregationSystemDbConnection";
        public const string FailedToFetchNewsMessage = "Failed to fetch news from source: {0}, With Status Code: {1}, and Message: {3}";
        public const string TopicPredictionUrl = "http://127.0.0.1:5000/predict-topic"; //TODO
        public static int SystemUserId = -1;
        public const string NewArticleNotificationTitle = "New Article Alert for You!";
        public const string UserAlreadyExistWithThisEmail = "User already exist with {0} Email Or {1} UserName.";
        public const string UserNotFoundWithThisEmail = "User not found with {0} Email.";
        public const string UserNotFoundWithThisId = "User not found with {0} Id.";
        public const string InvalidPassword = "Provided Password is Invalid.";
        public static int AccessTokenExpireTime = 20;
        public const int MinStringLength = 2;
        public const int MaxStringLength = 50;
        public const string TokenType = "JWT";
        public const string EmailValidationRegex = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
        public const string PasswordFormatValidationRegex = @"(?=^.{8,}$)((?=.*\d)|(?=.*\W+))(?![.\n])(?=.*[A-Z])(?=.*[a-z]).*$";
        public const string UnexpectedError = "An Unexpected Error occured. Please try again after some time.";
        public const string AdminUser = "Admin, User";
        public const string AdminOnly = "Admin";
        public static string WelcomeMessageWithMenu =
@"Welcome to the News Aggregator application. Please choose the options below.
1. Login
2. Sign up
3. Exit";

        public static string WelcomeMessageOnUserLogin = "Welcome to the News Application, {0}! Date: {1} Time:{2}";

        public static string AdminMainMenu =
@"1. View the list of external servers and status
2. View the external server’s details
3. Update/Edit the external server’s details
4. Add new News Category
5. Logout";

        public static string UserWelcomeMenu =
WelcomeMessageOnUserLogin +
@"Please choose the options below
1. Headlines
2. Saved Articles
3. Search
4. Notifications
5. Logout";

        public static string HeadlinesSubMenu =
WelcomeMessageOnUserLogin +
@"Please choose the options below
1. Today
2. Date range
3. Logout";


        public static string HeadlinesCategoryMenu = WelcomeMessageOnUserLogin + @" Please choose the options below for Headlines";
    }
}
