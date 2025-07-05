using NewsAggregationSystem.Common.Enums;

namespace NewsAggregationSystem.Client.Factories
{
    public class MenuFactory
    {
        public static IMenuProvider GetMenuProvider(string Role, HttpClient httpClient)
        {
            if (string.IsNullOrEmpty(Role))
            {
                throw new Exception("Invalid Role Exception.");
            }
            if (Role.ToLower() == UserRoles.Admin.ToString().ToLower())
            {
                return new AdminMenuProvider(httpClient);
            }
            else
            {
                return new UserMenuProvider(httpClient);
            }
        }
    }
}
