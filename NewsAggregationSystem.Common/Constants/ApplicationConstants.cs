namespace NewsAggregationSystem.Common.Constants
{
    public static class ApplicationConstants
    {
        public static class LogMessage
        {
            public const string AddingNewsCategory = "Attempting to add news category: {Category} by user ID: {UserId}";
            public const string NewsCategoryAddedSuccessfully = "News category '{Category}' added successfully with ID: {CategoryId} by user ID: {UserId}";

            public const string TogglingNewsCategoryVisibility = "Toggling visibility of category ID: {CategoryId} to hidden: {IsHidden}";
            public const string NewsCategoryVisibilityToggled = "Visibility toggled for category ID: {CategoryId}";

            public const string FetchingAllNewsCategories = "Fetching all news categories for role: {UserRole}";
            public const string NewsCategoriesFetchedSuccessfully = "Fetched news categories for role: {UserRole}";

            public const string SigningUpUser = "Signing up user with email: {Email}";
            public const string UserSignedUpSuccessfully = "User signed up successfully with email: {Email}";

            public const string UserLoginAttempt = "Login attempt for email: {Email}";
            public const string UserLoggedInSuccessfully = "User logged in successfully: {Email}";

            public const string UserLogoutAttempt = "User logout initiated.";
            public const string UserLoggedOutSuccessfully = "User logged out successfully.";

            public const string ArticleNotFound = "Article ID: {ArticleId} not found for user ID: {UserId}";
            public const string ArticleFetchedSuccessfully = "Article ID: {ArticleId} successfully fetched for user ID: {UserId}";
            public const string FetchingArticles = "Fetching articles for user ID: {UserId}";
            public const string FetchingSavedArticles = "Fetching saved articles for user ID: {UserId}";
            public const string FetchingArticleById = "Fetching article by ID: {ArticleId} for user ID: {UserId}";
            public const string SavingArticle = "Saving article ID: {ArticleId} for user ID: {UserId}";
            public const string ArticleSavedSuccessfully = "Article ID: {ArticleId} saved successfully for user ID: {UserId}";
            public const string DeletingSavedArticle = "Deleting saved article ID: {ArticleId} for user ID: {UserId}";
            public const string ArticleDeletedSuccessfully = "Article ID: {ArticleId} deleted successfully for user ID: {UserId}";
            public const string ReactingToArticle = "User ID: {UserId} reacting to article ID: {ArticleId} with reaction ID: {ReactionId}";
            public const string ArticleReactedSuccessfully = "Reaction added for article ID: {ArticleId} by user ID: {UserId}";
            public const string TogglingArticleVisibility = "User ID: {UserId} toggling visibility for article ID: {ArticleId} to hidden: {IsHidden}";
            public const string ArticleVisibilityToggled = "Visibility toggled for article ID: {ArticleId} by user ID: {UserId}";
            public const string HidingArticlesByKeywordSucceeded = "Successfully hid articles for keyword: {Keyword} by user ID: {UserId}";
            public const string HidingArticlesByKeywordStarted = "Request received to hide articles by keyword: {Keyword} from user ID: {UserId}";
            public const string HidingArticlesByKeywordFailed = "Error occurred while hiding articles for keyword: {Keyword} from user ID: {UserId}. Exception: {Exception}";
            public const string PredictingCategory = "Predicting category for article: {Title}";
            public const string ResolvedCategory = "Resolved category ID {CategoryId} for article: {Title}";
            public const string ProcessingArticle = "Processing article: {Title}";
            public const string FetchingNewsStarted = "Started fetching news from source ID {NewsSourceId} for country: {Country}, category: {Category}";
            public const string NewsSourceNotFound = "News source with ID {NewsSourceId} not found.";
            public const string SendingRequestToApi = "Sending request to API: {Url}";
            public const string ApiResponseStatus = "Received response with status code: {StatusCode} from source ID {NewsSourceId}";
            public const string DeserializationCompleted = "Deserialization of articles completed from source ID {NewsSourceId}";
            public const string ProcessingArticleTitle = "Processing article: {Title}";
            public const string SavingArticlesToDb = "Saving {Count} articles to database from source ID {NewsSourceId}";
            public const string SourceStatusUpdated = "News source status updated to {Status} for source ID {NewsSourceId}";
            public const string ExceptionWhileFetching = "Exception occurred while fetching news from source ID {NewsSourceId}: {Message}";
            public const string PredictingCategoryForArticle = "Predicting category for article with title: {Title}";
            public const string ResolvedCategoryId = "Resolved category ID: {CategoryId} for title: {Title}";
            public const string ArticleProcessed = "Article processed: {Title}";
            
            // User related messages
            public const string FetchingAllUsers = "Fetching all users.";
            public const string UsersFetchedSuccessfully = "Fetched {Count} users successfully.";
            
            // News Source related messages
            public const string FetchingAllNewsSources = "Fetching all news sources.";
            public const string NewsSourcesFetchedSuccessfully = "Fetched {Count} news sources.";
            public const string FetchingNewsSourceById = "Fetching news source with ID: {NewsSourceId}";
            public const string NewsSourceFetchedSuccessfully = "News source with ID: {NewsSourceId} fetched successfully.";
            public const string NewsSourceNotFoundById = "News source with ID: {NewsSourceId} not found.";
            public const string CreatingNewsSource = "Adding new news source with name: {NewsSourceName}";
            public const string NewsSourceCreatedSuccessfully = "News source '{NewsSourceName}' added successfully by user ID: {UserId}";
            public const string UpdatingNewsSource = "Updating news source ID: {NewsSourceId}";
            public const string NewsSourceUpdatedSuccessfully = "News source ID: {NewsSourceId} updated successfully by user ID: {UserId}";
            public const string DeletingNewsSource = "Deleting news source with ID: {NewsSourceId}";
            public const string NewsSourceDeletedSuccessfully = "News source with ID: {NewsSourceId} deleted successfully.";

            // Notification Preferences related messages
            public const string AddingKeywordToCategory = "User {UserId} is attempting to add keyword '{Keyword}' to category ID {CategoryId}.";
            public const string KeywordAddedToCategory = "Keyword '{Keyword}' added successfully for category ID {CategoryId} by user {UserId}.";
            public const string ChangingKeywordStatus = "User {UserId} is attempting to change keyword status. Keyword ID: {KeywordId}, Enable: {IsEnable}.";
            public const string KeywordStatusUpdated = "Keyword status updated successfully. Keyword ID: {KeywordId}, Enable: {IsEnable}.";
            public const string ChangingCategoryStatus = "User {UserId} is attempting to change category status. Category ID: {CategoryId}, Enable: {IsEnable}.";
            public const string CategoryStatusUpdated = "Category status updated successfully. Category ID: {CategoryId}, Enable: {IsEnable}, User ID: {UserId}.";
            
            // Reports related messages
            public const string CreatingArticleReport = "User {UserId} is attempting to report article ID: {ArticleId} with reason: {Reason}";
            public const string ArticleReportCreated = "Article ID: {ArticleId} reported successfully by user ID: {UserId}";
            public const string ArticleAlreadyReported = "Article ID: {ArticleId} has already been reported by user ID: {UserId}";
            
            // Client UI Messages
            public const string InvalidEmailPasswordFormat = "Invalid email or password format.";
            public const string IncorrectEmailPassword = "Incorrect email or password.";
            public const string NoAccountFound = "No account found with the provided email.";
            public const string LoginFailed = "Login failed. Status: {StatusCode} - {ReasonPhrase}";
            public const string LoginError = "Login error: {ErrorMessage}";
            public const string AccountCreatedSuccessfully = "Account Created Successfully.";
            public const string InvalidInputDetails = "Invalid input. Please check the details you entered.";
            public const string AccountAlreadyExists = "An account with this email already exists.";
            public const string SignupFailed = "Signup failed. Status: {StatusCode} - {ReasonPhrase}";
            public const string SignupError = "Signup error: {ErrorMessage}";
            
            // Article Service Messages
            public const string FailedToRetrieveSavedArticles = "Failed to retrieve saved articles. Status code: {StatusCode}";
            public const string ErrorFetchingSavedArticles = "Error occurred while fetching saved articles: {ErrorMessage}";
            public const string FailedToFetchArticles = "Failed to fetch articles. Status Code: {StatusCode}";
            public const string ErrorFetchingArticles = "Error occurred while fetching articles: {ErrorMessage}";
            public const string ClientArticleSavedSuccessfully = "Article saved successfully.";
            public const string ClientArticleNotFound = "Article with ID {ArticleId} not found.";
            public const string ArticleAlreadySaved = "Article is already saved.";
            public const string FailedToSaveArticle = "Failed to save article. Status code: {StatusCode}";
            public const string ErrorSavingArticle = "Error occurred while saving article: {ErrorMessage}";
            public const string ClientArticleReactedSuccessfully = "Article {ReactionName} successfully.";
            public const string AlreadyReactedToArticle = "You have already reacted to this article.";
            public const string FailedToReactToArticle = "Failed to react to article. Status code: {StatusCode}";
            public const string ErrorReactingToArticle = "Error occurred while reacting to article: {ErrorMessage}";
            public const string ArticleRemovedFromSaved = "Article removed from saved list successfully.";
            public const string ArticleNotSaved = "Article is not saved. Save it before trying to delete it.";
            public const string FailedToDeleteSavedArticle = "Failed to delete saved article. Status code: {StatusCode}";
            public const string ErrorDeletingSavedArticle = "Error occurred while deleting saved article: {ErrorMessage}";
            public const string ArticleVisibilityUpdated = "Article ID {ArticleId} is now {Visibility}.";
            public const string ArticleAlreadyInState = "Article is already {State}.";
            public const string FailedToUpdateArticleVisibility = "Failed to update article visibility. Status code: {StatusCode}";
            public const string ErrorTogglingArticleVisibility = "Error occurred while toggling article visibility: {ErrorMessage}";
            public const string UnauthorizedAccess = "Unauthorized: Please login as a valid user.";
            public const string ErrorFetchingArticle = "Error occurred while fetching the article: {ErrorMessage}";
            
            // News Sources Service Messages
            public const string ExternalServerCreatedSuccessfully = "External Server Created Successfully!";
            public const string AddNewsSourceFailed = "Add news source failed: {ErrorMessage}.";
            public const string ErrorAddingNewsSource = "Error adding news source: {ErrorMessage}";
            public const string GetAllNewsSourcesFailed = "Get all news sources failed: {ErrorMessage}.";
            public const string ErrorFetchingNewsSources = "Error while fetching news sources: {ErrorMessage}";
            public const string ExternalServerUpdatedSuccessfully = "External server updated successfully.";
            public const string ExternalServerNotFound = "External server with Id {ServerId} not found.";
            public const string FailedToUpdateExternalServer = "Failed to update. Status Code: {StatusCode}";
            public const string ErrorUpdatingNewsSource = "Error updating news source: {ErrorMessage}";
            
            // News Category Service Messages
            public const string ClientNewsCategoryAddedSuccessfully = "News category added successfully.";
            public const string NewsCategoryAlreadyExists = "News category already exists with this name.";
            public const string FailedToAddCategory = "Failed to add category. Status Code: {StatusCode}";
            public const string ErrorAddingNewsCategory = "Error while adding news category: {ErrorMessage}";
            public const string CategoryHiddenSuccessfully = "Category hidden successfully.";
            public const string CategoryUnhiddenSuccessfully = "Category unhidden successfully.";
            public const string CategoryVisibilityAlreadyUpdated = "Category visibility is already updated.";
            public const string CategoryNotFound = "Category not found.";
            public const string FailedToToggleCategoryVisibility = "Failed to toggle category visibility. Status Code: {StatusCode}";
            public const string ErrorTogglingCategoryVisibility = "Error occurred: {ErrorMessage}";
            public const string FailedToRetrieveNewsCategories = "Failed to retrieve news categories. Status code: {StatusCode}";
            public const string ErrorFetchingCategories = "Error occurred while fetching categories: {ErrorMessage}";
        }


        public static string DatabaseProviderNotConfigured = "Database provider is not configured. Use dependency injection or provide a connection string explicitly.";
        public const string DateTime2With2Precision = "datetime2(2)";
        public const string NewsAggregationSystemDbConnection = "NewsAggregationSystemDbConnection";
        public const string FailedToFetchNewsMessage = "Failed to fetch news from source: {0}, With Status Code: {1}, and Message: {3}";
        public const string TopicPredictionUrl = "http://127.0.0.1:5000/predict-topic"; //TODO
        public static int SystemUserId = -1;

        // API URL Parameters
        public const string CountryParameter = "country";
        public const string CategoryParameter = "category";
        public const string ApiKeyParameter = "apiKey";
        public const string LocaleParameter = "locale";
        public const string LimitParameter = "limit";
        public const string ApiTokenParameter = "api_token";
        public const string DefaultCountry = "us";
        public const string DefaultCategory = "";
        public const string DefaultLimit = "3";
        public const string NewArticleNotificationTitle = "New Article Alert for You!";
        public const string UserAlreadyExistWithThisEmail = "User already exist with {0} Email Or {1} UserName.";
        public const string UserNotFoundWithThisEmail = "User not found with {0} Email.";
        public const string KeywordNotFoundWithThisId = "Keyword not found with {0} Id.";
        public const string KeywordAlreadyExist = "Keyword {0} is already exist.";
        public const string UserNotFoundWithThisId = "User not found with {0} Id.";
        public const string ArticleNotFoundWithThisId = "Article not found with {0} Id.";
        public const string CategoryNotFoundWithThisId = "Category not found with {0} Id.";
        public const string InvalidPassword = "Provided Password is Invalid.";
        public static int AccessTokenExpireTime = 20;
        public const int MinStringLength = 2;
        public const int MaxStringLength = 50;
        public const string TokenType = "JWT";
        public const string EmailValidationRegex = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
        public const string PasswordFormatValidationRegex = @"(?=^.{8,}$)((?=.*\d)|(?=.*\W+))(?![.\n])(?=.*[A-Z])(?=.*[a-z]).*$";
        public const string UnexpectedError = "An Unexpected Error occured. Please try again after some time.";
        public const string UserOnly = "User";
        public const string AdminOnly = "Admin";
        public const string JsonContentType = "application/json";
        public const string ArticleReactionAlreadyPresentMessage = "Article reaction with Id {0} is already present.";
        public const string ArticleIsAlreadyHiddenMessage = "Article reaction with Id {0} is already Hidden.";
        public const string ArticleIsAlreadySaved = "Article with Id {0} is already saved.";
        public const string ArticleIsAlreadyReported = "Article with Id {0} is already reported.";
        public const string ReportTitle = "Article Report Received.";
        public const string CategoryAlreadyExist = "Category with Name {0} is already exist.";
        public const string CategoryReactionIsAlreadyExist = "News Category reaction is already exist.";
        public const string ArticleIsInUnsavedState = "Article with Id {0} is not present in saved articles.";
        public static string WelcomeMessage = "Welcome to the News Aggregator application. Please choose the options below.";
        public static string PressAnyKeyToContinue = "Press any key to continue...";

        public const string Login = "Login";
        public const string SignUp = "Sign up";
        public const string Exit = "Exit";
        public const string DeleteSavedArticle = "Delete Saved Article.";
        public const string SortArticlesByLikes = "Sort Articles By Likes.";

        public static readonly List<string> WelcomeMenu = new()
        {
            Login,
            SignUp,
            Exit
        };

        public static string WelcomeMessageOnUserLogin = "Welcome to the News Application, {0}! Date: {1} Time:{2}";

        public const string CreateExternalServer = "Create External Server";
        public const string ViewExternalServers = "View the list of external servers and status";
        public const string ViewExternalServerDetails = "View the external server's details";
        public const string EditExternalServer = "Update/Edit the external server's details";
        public const string AddNewsCategory = "Add new News Category";
        public const string HideArticlesByCategory = "Hide the Articles by News Category";
        public const string UnhideArticlesByCategory = "UnHide the Articles by News Category";
        public const string HideArticlesById = "Hide the Articles by Id";
        public const string UnhideArticlesById = "UnHide the Articles by Id";
        public const string HideArticlesByKeyword = "Hide the Articles by News Keywords";
        public const string Logout = "Logout";

        public static readonly List<string> AdminMainMenu = new()
        {
            CreateExternalServer,
            ViewExternalServers,
            ViewExternalServerDetails,
            EditExternalServer,
            AddNewsCategory,
            HideArticlesByCategory,
            UnhideArticlesByCategory,
            HideArticlesById,
            UnhideArticlesById,
            HideArticlesByKeyword,
            Logout
        };

        public const string Headlines = "Headlines";
        public const string SavedArticles = "Saved Articles";
        public const string Search = "Search";
        public const string Notifications = "Notifications";

        public static readonly List<string> UserWelcomeMenu = new()
        {
            Headlines,
            SavedArticles,
            Search,
            Notifications,
            Logout
        };

        public const string EnableCategoryStatus = "Enable this category";
        public const string DisableCategoryStatus = "Disable this category";
        public const string AddKeywordToCategory = "Add a new keyword to this category";
        public const string ConfigureKeywords = "Configure Keywords";
        public const string EnableKeywordStatus = "Enable this keyword";
        public const string DisableKeywordStatus = "Disable this keyword";

        public static readonly List<string> PreferenceConfigurationMenu = new()
        {
            EnableCategoryStatus,
            DisableCategoryStatus,
            AddKeywordToCategory,
            ConfigureKeywords,
            Back
        };

        public static readonly List<string> PreferenceKeywordConfiguration = new()
        {
            EnableKeywordStatus,
            DisableKeywordStatus,
            Back
        };

        public const string HeadlineToday = "Today";
        public const string HeadlineDateRange = "Date range";

        public static readonly List<string> HeadlinesSubMenuOptions = new()
        {
            HeadlineToday,
            HeadlineDateRange,
            Back,
            Logout
        };

        public static string HeadlinesSubMenu =
WelcomeMessageOnUserLogin +
@" Please choose the options below

1. Today
2. Date range
3. Logout
";

        public const string View = "View Notifications";
        public const string Configure = "Configure Notifications";
        public const string Back = "Back";

        public static readonly List<string> NotificationMenu = new()
        {
            View,
            Configure,
            Back,
            Logout
        };

        public const string HeadlineSaveArticle = "Save Article.";
        public const string HeadlineLikeArticle = "Like Article.";
        public const string HeadlineDislikeArticle = "Dislike Article.";
        public const string HeadlineReportArticle = "Report Article.";
        public const string HeadlineReadArticle = "Read Article.";

        public static readonly List<string> HeadlineSubMenuOptions = new()
        {
            HeadlineSaveArticle,
            HeadlineLikeArticle,
            HeadlineDislikeArticle,
            HeadlineReportArticle,
            HeadlineReadArticle,
            Back
        };

        public static string HeadlinesCategoryMenu = WelcomeMessageOnUserLogin + @" Please choose the options below for Headlines";

        public static string LoginPath = "Auth/login";
        public static string SignupPath = "Auth/sign-up";
        public static string AddNewsSource = "NewsSources";
        public static string AddNewsCategoryPath = "NewsCategories";
        public static string SavedArticlesPath = "Articles/saved-articles";
        public static string GetAllArticles = "Articles";
        public static string Notification = "Notification";
        public static string NotificationPreferences = "NotificationPreferences";
        public static string NewsSourceNotFoundMessage = "News source not found";
        public static string BaseUrl = "https://localhost:7122/api/";
    }
}
