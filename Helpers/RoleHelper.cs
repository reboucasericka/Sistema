using Microsoft.AspNetCore.Identity;
using Sistema.Data.Entities;

namespace Sistema.Helpers
{
    public interface IRoleHelper
    {
        Task<IdentityResult> CreateRoleAsync(string roleName);
        Task<bool> RoleExistsAsync(string roleName);
        Task<IdentityResult> AddUserToRoleAsync(User user, string roleName);
        Task<bool> IsUserInRoleAsync(User user, string roleName);
        Task<IList<string>> GetUserRolesAsync(User user);
        Task<IdentityResult> RemoveUserFromRoleAsync(User user, string roleName);
    }

    public class RoleHelper : IRoleHelper
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        public RoleHelper(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IdentityResult> CreateRoleAsync(string roleName)
        {
            var roleExists = await RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
            return IdentityResult.Success;
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _roleManager.RoleExistsAsync(roleName);
        }

        public async Task<IdentityResult> AddUserToRoleAsync(User user, string roleName)
        {
            return await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<IList<string>> GetUserRolesAsync(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<IdentityResult> RemoveUserFromRoleAsync(User user, string roleName)
        {
            return await _userManager.RemoveFromRoleAsync(user, roleName);
        }
    }
}
