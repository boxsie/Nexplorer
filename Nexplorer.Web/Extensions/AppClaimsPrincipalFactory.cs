using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Nexplorer.Data.Context;
using Nexplorer.Domain.Entity.User;

namespace Nexplorer.Web.Extensions
{
    public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        private readonly NexplorerDb _nexplorerDb;

        public AppClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> optionsAccessor, NexplorerDb nexplorerDb)
            : base(userManager, roleManager, optionsAccessor)
        {
            _nexplorerDb = nexplorerDb;
        }
    }
}