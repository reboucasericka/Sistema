using Microsoft.AspNetCore.Identity;
using Sistema.Data.Entities;
using Sistema.Models.Account;

namespace Sistema.Helpers
{
    public interface IUserHelper
    {
        // User retrieval methods
        Task<User?> GetUserByEmailAsync(string email);

        Task<IdentityResult> AddUserAsync(User user, string password);//ok

        Task<SignInResult> LoginAsync(LoginViewModel model); //ok

        Task LogoutAsync(); //ok


        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByIdAsync(string userId);
        Task<User?> GetCurrentUserAsync();
        string GetUserId(System.Security.Claims.ClaimsPrincipal user);
        
        // User management methods
       
        Task<IdentityResult> UpdateUserAsync(User user);
        Task<IdentityResult> DeleteUserAsync(User user);
        Task<IdentityResult> CreateEmployeeUserAsync(User user, string password);
        
        // Authentication methods
        
        
        
        // Password methods
        Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword);
        Task<string> GeneratePasswordResetTokenAsync(User user);
        Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword);
        
        // Role management methods
        Task CheckRoleAsync(string roleName); //ok
        Task AddUserToRoleAsync(User user, string roleName); //ok
        Task<bool> IsUserInRoleAsync(User user, string roleName); //ok

        Task<IList<string>> GetUserRolesAsync(User user);
        Task<IList<User>> GetUsersInRoleAsync(string roleName);
        
        // Email activation methods
        Task<string> GenerateEmailConfirmationTokenAsync(User user);
        Task<IdentityResult> ConfirmEmailAsync(User user, string token);
    }
}
