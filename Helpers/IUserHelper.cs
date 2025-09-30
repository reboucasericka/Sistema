using Microsoft.AspNetCore.Identity;
using Sistema.Data.Entities;
using Sistema.Models;

namespace Sistema.Helpers
{
    public interface IUserHelper //IUserHelper
    {
        Task<User> GetUserByEmailAsync(string email);

        Task<IdentityResult> AddUserAsync(User user, string Password);

        Task<SignInResult> LoginAsync(LoginViewModel model);

        Task LogoutAsync();

        //2 metodos para mudar a password

        
        Task<IdentityResult> UpdateUserAsync(User user); //so muda os dados do utlizador

        Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword); //muda a password do user
        Task CheckRoleAsync(string roleName);//veriifica se a role existe se nao existir cria a role
        Task AddUserToRoleAsync(User user, string roleName);//adiciona o user a role
        Task<bool> IsUserInRoleAsync(User user, string roleName);//verifica se o user esta na role
    }
}
