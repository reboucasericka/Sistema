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

            // Garante que a BD está criada
            await _context.Database.EnsureCreatedAsync();

            // Cria roles principais
            await _userHelper.CheckRoleAsync("Admin");
            await _userHelper.CheckRoleAsync("Professional");
            await _userHelper.CheckRoleAsync("Customer");

            // Lê credenciais do appsettings.json ou user-secrets
            var adminEmail = _configuration["AdminUser:Email"];
            var adminPassword = _configuration["AdminUser:Password"];
            var adminFirstName = _configuration["AdminUser:FirstName"] ?? "Administrator";
            var adminLastName = _configuration["AdminUser:LastName"] ?? "System";

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                _logger.LogError("❌ AdminUser configuration missing in appsettings.json or secrets.json!");
                throw new InvalidOperationException("AdminUser credentials are not configured.");
            }

            // Cria ou atualiza o administrador
            var adminUser = await _userHelper.GetUserByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    FirstName = adminFirstName,
                    LastName = adminLastName,
                    Email = adminEmail,
                    UserName = adminEmail,
                    PhoneNumber = "000000000",
                    Active = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userHelper.AddUserAsync(adminUser, adminPassword);
                if (result != IdentityResult.Success)
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
                // Atualiza role se necessário
                var isInRole = await _userHelper.IsUserInRoleAsync(adminUser, "Admin");
                if (!isInRole)
                {
                    await _userHelper.AddUserToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation("✅ Admin user found and added to Admin role: {Email}", adminEmail);
                }
                else
                {
                    _logger.LogInformation("ℹ️ Admin user already exists and is properly configured: {Email}", adminEmail);
                }
            }

            // Executa seeds adicionais apenas se as tabelas estiverem vazias
            await SeedCategoriesAsync();
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

        private async Task SeedServicesAsync()
        {
            if (await _context.Service.AnyAsync()) return;

            var hair = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Hair");
            var makeup = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Makeup");

            var services = new List<Service>
            {
                new Service { Name = "Women's Haircut", CategoryId = hair.Id, Price = 25.00m, Duration = "45min", IsActive = true },
                new Service { Name = "Hair Coloring", CategoryId = hair.Id, Price = 60.00m, Duration = "1h30", IsActive = true },
                new Service { Name = "Social Makeup", CategoryId = makeup.Id, Price = 40.00m, Duration = "1h", IsActive = true }
            };

            _context.Service.AddRange(services);
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
