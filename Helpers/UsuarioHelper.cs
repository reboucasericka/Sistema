using Microsoft.AspNetCore.Identity;
using Sistema.Data.Entities;

namespace Sistema.Helpers
{
    public class UsuarioHelper : IUsuarioHelper
    {
        private readonly UserManager<Usuario> _userManager;

        public UsuarioHelper(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IdentityResult> AddUserAsync(Usuario user, string Password)
        {
            return await _userManager.CreateAsync(user, Password);
        }

        public Task AddUserToRoleAsync(Usuario user, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> ChangePasswordAsync(Usuario user, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task CheckRoleAsync(string roleName)
        {
            throw new NotImplementedException();
        }

        public async Task<Usuario> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public Task<bool> IsUserInRoleAsync(Usuario user, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            throw new NotImplementedException();
        }

        public Task LogoutAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateUserAsync(Usuario user)
        {
            throw new NotImplementedException();
        }
    }
}
