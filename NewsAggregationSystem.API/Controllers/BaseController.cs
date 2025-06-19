using Microsoft.AspNetCore.Mvc;
using NewsAggregationSystem.Common.Enums;
using System.Security.Claims;

namespace NewsAggregationSystem.API.Controllers
{
    public class BaseController : ControllerBase
    {
        #region Public Properties
        public int LoggedInUserId
        {
            get
            {
                return Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }
        }

        public string LoggedInUserName
        {
            get
            {
                return HttpContext.User.FindFirstValue(ClaimTypes.Name)!;
            }
        }

        public bool IsLoggedInUserAdmin
        {
            get
            {
                return (LoggedInUserRole == UserRoles.Admin.ToString());
            }
        }

        public string LoggedInUserRole
        {
            get
            {
                if (HttpContext.User.Claims.Any(o => o.Type == ClaimTypes.Role))
                    return HttpContext.User.Claims.FirstOrDefault(o => o.Type == ClaimTypes.Role)!.Value;
                return null;
            }

        }
        #endregion
    }
}
