using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Nexplorer.Config;
using Nexplorer.Domain.Entity.User;
using Nexplorer.Web.Enums;

namespace Nexplorer.Web.Services.User
{
    public class UserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserService> _logger;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<UserService> logger)
        {
            _roleManager = roleManager;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task CreateRoles()
        {
            var roles = Enum.GetNames(typeof(UserRoles));
            var userRoles = Settings.UserConfig.UserRoles;

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));
            }

            foreach (var userRole in userRoles.Where(x => roles.Any(y => y == x.Role)))
            {
                var user = await _userManager.FindByEmailAsync(userRole.Email);

                if (user != null)
                    await _userManager.AddToRoleAsync(user, userRole.Role);
                else
                    _logger.LogWarning($"{userRole.Role} {userRole.Email} cannot be created as the user doesn't exist");
            }
        }
    }
}
