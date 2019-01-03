using Hangfire.Dashboard;
using Nexplorer.Web.Enums;
using Nexplorer.Web.Extensions;

namespace Nexplorer.Web.Auth
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Allow all authenticated users to see the Dashboard (potentially dangerous).
            return httpContext.User.Identity.IsAuthenticated && httpContext.User.UserHasAccessLevel(UserRoles.Admin);
        }
    }
}