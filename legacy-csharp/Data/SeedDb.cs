using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sistema.Data.Entities;
using Sistema.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sistema.Data
{
    public class SeedDb
    {
        private readonly SistemaDbContext _context;
        private readonly IUserHelper _userHelper;
        private readonly ILogger<SeedDb> _logger;
        private readonly IConfiguration _configuration;
        private readonly Random _random;

        public SeedDb(
            SistemaDbContext context,
            IUserHelper userHelper,
            ILogger<SeedDb> logger,
            IConfiguration configuration)
        {
            _context = context;
            _userHelper = userHelper;
            _logger = logger;
            _configuration = configuration;
            _random = new Random();
        }

        public async Task SeedAsync()
        {
            _logger.LogInformation("🌱 Starting SeedDb initialization...");

            // Use migrations
            await _context.Database.MigrateAsync();

            await _userHelper.CheckRoleAsync("Admin");
            await _userHelper.CheckRoleAsync("Professional");
            await _userHelper.CheckRoleAsync("Customer");

            var adminEmail = _configuration["AdminUser:Email"];
            var adminPassword = _configuration["AdminUser:Password"];
            var adminFirstName = _configuration["AdminUser:FirstName"] ?? "Administrator";
            var adminLastName  = _configuration["AdminUser:LastName"] ?? "System";

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                _logger.LogError("❌ AdminUser configuration missing in appsettings.json or secrets.json!");
                throw new InvalidOperationException("AdminUser credentials are not configured.");
            }

            var adminUser = await _userHelper.GetUserByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    FirstName = adminFirstName,
                    LastName  = adminLastName,
                    Email     = adminEmail,
                    UserName  = adminEmail,
                    PhoneNumber   = "000000000",
                    Active        = true,
                    EmailConfirmed= true,
                    CreatedAt     = DateTime.UtcNow
                };

                var result = await _userHelper.AddUserAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                {
                    var errorList = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("❌ Failed to create admin user: {Errors}", errorList);
                    throw new InvalidOperationException($"Failed to create admin user: {errorList}");
                }

                await _userHelper.AddUserToRoleAsync(adminUser, "Admin");
                _logger.LogInformation("✅ Admin user created successfully: {Email}", adminEmail);
            }
            else
            {
                var changed = false;
                if (!adminUser.EmailConfirmed) { adminUser.EmailConfirmed = true; changed = true; }
                if (!adminUser.Active)         { adminUser.Active        = true; changed = true; }
                if (changed) await _context.SaveChangesAsync();

                if (!await _userHelper.IsUserInRoleAsync(adminUser, "Admin"))
                {
                    await _userHelper.AddUserToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation("✅ Admin user found and added to Admin role: {Email}", adminEmail);
                }
                else
                {
                    _logger.LogInformation("ℹ️ Admin user already exists and is properly configured: {Email}", adminEmail);
                }
            }

            await SeedCategoriesAsync();
            await SeedProductCategoriesAsync();
            await SeedSuppliersAsync();
            await SeedServicesAsync();
            await SeedProfessionalsAsync();
            await SeedCustomersAsync();

            _logger.LogInformation("✅ SeedDb process completed successfully!");
        }

        // ---------------------------- Seeds Auxiliares ----------------------------

        private async Task SeedCategoriesAsync()
        {
            if (await _context.Categories.AnyAsync()) return;

            var categories = new List<Category>
            {
                new Category { Name = "Hair", Description = "Haircut, coloring and styling services" },
                new Category { Name = "Nails", Description = "Manicure, pedicure and nail art" },
                new Category { Name = "Makeup", Description = "Professional makeup and events" },
                new Category { Name = "Massage", Description = "Relaxing and therapeutic massages" },
                new Category { Name = "Facial Treatments", Description = "Cleansing, peeling and skincare" },
                new Category { Name = "Waxing", Description = "Complete waxing services" }
            };

            _context.Categories.AddRange(categories);
            await _context.SaveChangesAsync();
            _logger.LogInformation("📦 Seeded {Count} service categories", categories.Count);
        }

        private async Task SeedProductCategoriesAsync()
        {
            if (await _context.ProductCategories.AnyAsync()) return;

            var productCategories = new List<ProductCategory>
            {
                new ProductCategory { Name = "Shampoos e Condicionadores" },
                new ProductCategory { Name = "Tinturas e Colorantes" },
                new ProductCategory { Name = "Produtos para Unhas" },
                new ProductCategory { Name = "Maquiagem" },
                new ProductCategory { Name = "Cremes e Hidratantes" },
                new ProductCategory { Name = "Ferramentas e Acessórios" }
            };

            _context.ProductCategories.AddRange(productCategories);
            await _context.SaveChangesAsync();
            _logger.LogInformation("🛍️ Seeded {Count} product categories", productCategories.Count);
        }

        private async Task SeedSuppliersAsync()
        {
            if (await _context.Suppliers.AnyAsync()) return;

            var suppliers = new List<Supplier>
            {
                new Supplier 
                { 
                    Name = "Distribuidora de Beleza Ltda", 
                    Email = "contato@distribuidorabeleza.com.br",
                    Phone = "(11) 99999-9999",
                    Address = "Rua das Flores, 123 - São Paulo/SP",
                    TaxId = "12.345.678/0001-90",
                    RegistrationDate = DateTime.UtcNow
                },
                new Supplier 
                { 
                    Name = "Cosméticos Premium S.A.", 
                    Email = "vendas@cosmeticospremium.com.br",
                    Phone = "(11) 88888-8888",
                    Address = "Av. Paulista, 456 - São Paulo/SP",
                    TaxId = "98.765.432/0001-10",
                    RegistrationDate = DateTime.UtcNow
                },
                new Supplier 
                { 
                    Name = "Fornecedor de Ferramentas", 
                    Email = "contato@ferramentasbeleza.com.br",
                    Phone = "(11) 77777-7777",
                    Address = "Rua das Ferramentas, 789 - São Paulo/SP",
                    TaxId = "11.222.333/0001-44",
                    RegistrationDate = DateTime.UtcNow
                }
            };

            _context.Suppliers.AddRange(suppliers);
            await _context.SaveChangesAsync();
            _logger.LogInformation("🏪 Seeded {Count} suppliers", suppliers.Count);
        }

        private async Task SeedServicesAsync()
        {
            if (await _context.Services.AnyAsync()) return;

            var hair = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Hair");
            var makeup = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Makeup");

            var services = new List<Service>
            {
                new Service { Name = "Women's Haircut", CategoryId = hair.CategoryId, Price = 25.00m, Duration = "45min", IsActive = true },
                new Service { Name = "Hair Coloring", CategoryId = hair.CategoryId, Price = 60.00m, Duration = "1h30", IsActive = true },
                new Service { Name = "Social Makeup", CategoryId = makeup.CategoryId, Price = 40.00m, Duration = "1h", IsActive = true }
            };

            _context.Services.AddRange(services);
            await _context.SaveChangesAsync();
            _logger.LogInformation("💇 Seeded {Count} services", services.Count);
        }

        private async Task SeedProfessionalsAsync()
        {
            if (await _context.Professionals.AnyAsync()) return;

            var professionals = new List<Professional>
            {
                new Professional { Name = "Carla Ribeiro", Specialty = "Makeup", DefaultCommission = 20, IsActive = true },
                new Professional { Name = "João Pereira", Specialty = "Hair", DefaultCommission = 15, IsActive = true }
            };

            _context.Professionals.AddRange(professionals);
            await _context.SaveChangesAsync();
            _logger.LogInformation("👩‍🎨 Seeded {Count} professionals", professionals.Count);
        }

        private async Task SeedCustomersAsync()
        {
            if (await _context.Customers.AnyAsync()) return;

            var customers = new List<Customer>
            {
                new Customer { Name = "Maria Silva", Email = "maria@email.com", Phone = "910000000", IsActive = true },
                new Customer { Name = "Pedro Costa", Email = "pedro@email.com", Phone = "920000000", IsActive = true }
            };

            _context.Customers.AddRange(customers);
            await _context.SaveChangesAsync();
            _logger.LogInformation("🧍 Seeded {Count} customers", customers.Count);
        }
    }
}
