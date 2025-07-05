namespace NewsAggregationSystem.Common.DTOs.Users
{
    public class UserState
    {
        public static string AccessToken { get; set; }
        public static string Role { get; set; }
        public static string UserName { get; set; }
        public static bool IsLoggedIn { get; set; }

        public static void Clear()
        {
            UserName = null;
            Role = null;
            AccessToken = null;
            IsLoggedIn = false;
        }
    }
}
