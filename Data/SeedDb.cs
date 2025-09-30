using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Helpers;

namespace Sistema.Data
{
    public class SeedDb
    {
        private readonly SistemaDbContext _context;
        private readonly IUserHelper _userHelper;
        private readonly RoleManager<IdentityRole> _roleManager;
        private Random _random;
        


        public SeedDb(SistemaDbContext context,IUserHelper userHelper, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userHelper = userHelper;
            _roleManager = roleManager;          
            _random = new Random();
        }

        public async Task SeedAsync()
        {
            // 1.Garante que BD existe
            await _context.Database.MigrateAsync();

            //await _context.Database.EnsureCreatedAsync();
            // Cria as roles se não existirem
            await _userHelper.CheckRoleAsync("Admin"); //verifica se a role admin existe se nao existir cria a role
            await _userHelper.CheckRoleAsync("Customer"); //verifica se a role customer existe se nao existir cria a role
            //verifica se esse usuario existe se nao existir cria o usuario
           

            // 2. Garante usuário admin (via Identity Helper)
            var user = await _userHelper.GetUserByEmailAsync("reboucasericka@gmail.com");
            if (user == null)
            {
                user = new User
                {
                    FirstName = "Ericka",
                    LastName = "Rebouças",
                    Email = "reboucasericka@gmail.com",
                    UserName = "reboucasericka@gmail.com",
                    PhoneNumber = "11999999999"
                };

                var result = await _userHelper.AddUserAsync(user, "123456");
                if (result != IdentityResult.Success)
                {
                    throw new InvalidOperationException("❌ Não foi possível criar o usuário seed");
                }

                await _userHelper.AddUserToRoleAsync(user, "Admin");
            }
            else
            {
                var isInRole = await _userHelper.IsUserInRoleAsync(user, "Admin");
                if (!isInRole)
                {
                    await _userHelper.AddUserToRoleAsync(user, "Admin");
                }
            }
        }

        private async Task CreateRoleIfNotExists(string roleName)
        {
            var roleExist = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}