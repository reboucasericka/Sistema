using Microsoft.AspNetCore.Identity;
using Sistema.Data.Entities;

namespace Sistema.Data
{
    public class SeedDb
    {
        private readonly SistemaDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedDb(SistemaDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedAsync()
            {
                await _context.Database.EnsureCreatedAsync();

            // Criar roles se não existirem
            await CheckRolesAsync();

            // Criar usuário admin se não existir
            await CheckUserAsync("admin@sistema.com", "Admin", "User", "Admin123!", "Admin");
            await CheckUserAsync("user@sistema.com", "User", "Test", "User123!", "User");

            // Criar dados iniciais
            await SeedInitialDataAsync();

            await _context.SaveChangesAsync();
        }

        private async Task CheckRolesAsync()
        {
            var roles = new[] { "Admin", "User", "Professional" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private async Task CheckUserAsync(string email, string firstName, string lastName, string password, string role)
        {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new User
                    {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                        UserName = email,
                        EmailConfirmed = true,
                    Active = true,
                    CreatedAt = DateTime.Now
                    };

                var result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
        }

        private async Task SeedInitialDataAsync()
        {
            // Criar categorias de produtos se não existirem
            if (!_context.ProductCategories.Any())
            {
                var categories = new[]
                {
                    new ProductCategory { Name = "Cabelo", Description = "Produtos para cabelo" },
                    new ProductCategory { Name = "Maquiagem", Description = "Produtos de maquiagem" },
                    new ProductCategory { Name = "Skincare", Description = "Produtos de cuidados com a pele" }
                };

                _context.ProductCategories.AddRange(categories);
            }

            // Criar categorias de serviços se não existirem
            if (!_context.Categories.Any())
            {
                var serviceCategories = new[]
                {
                    new Category { Name = "Cabelo", Description = "Serviços de cabelo" },
                    new Category { Name = "Maquiagem", Description = "Serviços de maquiagem" },
                    new Category { Name = "Estética", Description = "Serviços de estética" }
                };

                _context.Categories.AddRange(serviceCategories);
            }

            // Criar fornecedores se não existirem
            if (!_context.Suppliers.Any())
            {
                var suppliers = new[]
                {
                    new Supplier { Name = "Fornecedor A", Contact = "contato@fornecedora.com", Phone = "123456789" },
                    new Supplier { Name = "Fornecedor B", Contact = "contato@fornecedorb.com", Phone = "987654321" }
                };

                _context.Suppliers.AddRange(suppliers);
            }

            // Criar métodos de pagamento se não existirem
            if (!_context.PaymentMethods.Any())
            {
                var paymentMethods = new[]
                {
                    new PaymentMethod { Description = "Dinheiro", IsActive = true },
                    new PaymentMethod { Description = "Cartão de Débito", IsActive = true },
                    new PaymentMethod { Description = "Cartão de Crédito", IsActive = true },
                    new PaymentMethod { Description = "PIX", IsActive = true }
                };

                _context.PaymentMethods.AddRange(paymentMethods);
            }

            // Criar configurações iniciais se não existirem
            if (!_context.Settings.Any())
            {
                var settings = new[]
                {
                    new Setting { Key = "SalonName", Value = "Salão Sistema", Description = "Nome do salão" },
                    new Setting { Key = "SalonAddress", Value = "Rua Exemplo, 123", Description = "Endereço do salão" },
                    new Setting { Key = "SalonPhone", Value = "(11) 99999-9999", Description = "Telefone do salão" }
                };

                _context.Settings.AddRange(settings);
            }
        }
    }
}
