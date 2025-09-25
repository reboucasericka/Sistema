using Microsoft.AspNetCore.Identity;
using Sistema.Data.Entities;

namespace Sistema.Helpers
{
    public interface IUsuarioHelper
    {
        Task<Usuario> GetUserByEmailAsync(string email);

        Task<IdentityResult> AddUserAsync(Usuario user, string Password);

        Task<SignInResult> LoginAsync(LoginViewModel model);

        Task LogoutAsync();

        //2 metodos para mudar a password


        Task<IdentityResult> UpdateUserAsync(Usuario user);



        Task<IdentityResult> ChangePasswordAsync(Usuario user, string oldPassword, string newPassword);
        Task CheckRoleAsync(string roleName);//veriifica se a role existe se nao existir cria a role
        Task AddUserToRoleAsync(Usuario user, string roleName);//adiciona o user a role
        Task<bool> IsUserInRoleAsync(Usuario user, string roleName);//verifica se o user esta na role
    }
}
