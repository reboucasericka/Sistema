using Microsoft.AspNetCore.Identity;
using Sistema.Data.Entities;
using Sistema.Models.Account;

namespace Sistema.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserHelper(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        public async Task<IdentityResult> AddUserAsync(User user, string Password)
        {
            return await _userManager.CreateAsync(user, Password);
        }

        public async Task AddUserToRoleAsync(User user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<IdentityResult> ChangePasswordAsync(
            User user, string oldPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }

        public async Task CheckRoleAsync(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = roleName
                });
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);//verifica se o user esta na role retorna um boolean
        }

        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            Console.WriteLine($"🔍 UserHelper.LoginAsync: Tentando login para '{model.Username}'");
            
            // Primeiro, tentar encontrar o usuário por email, depois por username
            var user = await _userManager.FindByEmailAsync(model.Username) 
                      ?? await _userManager.FindByNameAsync(model.Username);
            
            if (user == null)
            {
                Console.WriteLine($"❌ UserHelper.LoginAsync: Usuário não encontrado para '{model.Username}'");
                return SignInResult.Failed;
            }

            Console.WriteLine($"✅ UserHelper.LoginAsync: Usuário encontrado - Email: {user.Email}, UserName: {user.UserName}");

            // Usar o UserName para PasswordSignInAsync (não email)
            // Isso garante que o User.Identity.Name seja consistente
            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, false);
            
            Console.WriteLine($"🔍 UserHelper.LoginAsync: Resultado do login - Succeeded: {result.Succeeded}, IsLockedOut: {result.IsLockedOut}, IsNotAllowed: {result.IsNotAllowed}");
            
            return result;
        }

        public async Task LogoutAsync()
        {
            try
            {
                Console.WriteLine("🔓 UserHelper: Iniciando SignOut...");
                await _signInManager.SignOutAsync();
                Console.WriteLine("✅ UserHelper: SignOut realizado com sucesso");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ UserHelper: Erro no SignOut: {ex.Message}");
                throw;
            }
        }

        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userManager.FindByNameAsync(username);
        }

        public async Task<IdentityResult> DeleteUserAsync(User user)
        {
            return await _userManager.DeleteAsync(user);
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            // This method should be called from a controller with HttpContext access
            // For now, return null as it needs to be implemented in the controller
            return null;
        }

        public async Task<IList<User>> GetUsersInRoleAsync(string roleName)
        {
            return await _userManager.GetUsersInRoleAsync(roleName);
        }

        public async Task<IdentityResult> CreateEmployeeUserAsync(User user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer"); // Employees have Customer role
            }
            return result;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
        {
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<IList<string>> GetUserRolesAsync(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }

    }
}
