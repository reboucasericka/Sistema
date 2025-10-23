using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly Random _random;

        public SeedDb(
            SistemaDbContext context,
            IUserHelper userHelper,
            ILogger<SeedDb> logger,
            IConfiguration configuration,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IServiceProvider serviceProvider)
        {
            _context = context;
            _userHelper = userHelper;
            _logger = logger;
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _serviceProvider = serviceProvider;
            _random = new Random();
        }

        public async Task SeedAsync()
        {
            _logger.LogInformation("🌱 Starting SeedDb initialization...");

            // Use migrations
            try
            {
                await _context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("⚠️ Migration warning (continuing): {Message}", ex.Message);
            }

            // Garante as roles básicas
            await _userHelper.CheckRoleAsync("Admin");
            await _userHelper.CheckRoleAsync("Customer");
            await _userHelper.CheckRoleAsync("Professional");


            var adminEmail = _configuration["AdminUser:Email"];
            var adminPassword = _configuration["AdminUser:Password"];
            var adminFirstName = _configuration["AdminUser:FirstName"] ?? "Administrator";
            var adminLastName  = _configuration["AdminUser:LastName"] ?? "System";

            // Se não estiver configurado, usa valores padrão
            if (string.IsNullOrWhiteSpace(adminEmail))
            {
                adminEmail = "admin@admin.com";
                adminPassword = "admin";
                adminFirstName = "Admin";
                adminLastName = "System";
                _logger.LogInformation("ℹ️ Using default admin credentials: admin@admin.com");
            }

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

                var result = await _userHelper.AddUserAsync(adminUser, adminPassword!);
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

                // Garante que o admin tem a role correta
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
            await SeedTestCustomerIdentityAsync();


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
                    Email = "contato@distribuidorabeleza.com",
                    Phone = "(+351) 777-777-777",
                    Address = "Rua das Flores",
                    TaxId = "000.000.002",
                    RegistrationDate = DateTime.UtcNow
                },
                new Supplier 
                { 
                    Name = "Cosméticos Premium S.A.", 
                    Email = "vendas@cosmeticospremium.com",
                    Phone = "(+351) 777-777-777",
                    Address = "Av. Paulista",
                    TaxId = "000.000.001",
                    RegistrationDate = DateTime.UtcNow
                },
                new Supplier 
                { 
                    Name = "Fornecedor de Ferramentas", 
                    Email = "contato@ferramentasbeleza.com",
                    Phone = "(+351) 777-777-777",
                    Address = "Rua das Ferramentas",
                    TaxId = "000.000.000",
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

            // Profissional 1
            var user1 = await _userHelper.GetUserByEmailAsync("carla.ribeiro@empresa.com");
            if (user1 == null)
            {
                user1 = new User
                {
                    FirstName = "Carla",
                    LastName = "Ribeiro",
                    Email = "carla.ribeiro@empresa.com",
                    UserName = "carlaribeiro",
                    PhoneNumber = "930000001",
                    Active = true,
                    EmailConfirmed = true
                };

                var result1 = await _userHelper.AddUserAsync(user1, "123456789");
                if (result1.Succeeded)
                    await _userHelper.AddUserToRoleAsync(user1, "Professional");
            }

            // Profissional 2
            var user2 = await _userHelper.GetUserByEmailAsync("joao.pereira@empresa.com");
            if (user2 == null)
            {
                user2 = new User
                {
                    FirstName = "João",
                    LastName = "Pereira",
                    Email = "joao.pereira@empresa.com",
                    UserName = "joaopereira",
                    PhoneNumber = "930000002",
                    Active = true,
                    EmailConfirmed = true
                };

                var result2 = await _userHelper.AddUserAsync(user2, "123456789");
                if (result2.Succeeded)
                    await _userHelper.AddUserToRoleAsync(user2, "Professional");
            }

            // Agora associa os usuários criados aos profissionais
            var professionals = new List<Professional>
    {
        new Professional
        {
            Name = "Carla Ribeiro",
            Specialty = "Makeup",
            DefaultCommission = 20,
            CommissionPercentage = 20,
            IsActive = true,
            UserId = user1.Id
        },
        new Professional
        {
            Name = "João Pereira",
            Specialty = "Hair",
            DefaultCommission = 15,
            CommissionPercentage = 15,
            IsActive = true,
            UserId = user2.Id
        }
    };

            _context.Professionals.AddRange(professionals);
            await _context.SaveChangesAsync();
            _logger.LogInformation("👩‍🎨 Seeded {Count} professionals (with user accounts)", professionals.Count);
        }

        private async Task SeedCustomersAsync()
        {
            if (await _context.Customers.AnyAsync())
            {
                _logger.LogInformation("✅ Clientes já existentes. Seed ignorado.");
                return;
            }

            // Garante que a role "Customer" exista
            if (!await _roleManager.RoleExistsAsync("Customer"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Customer"));
                _logger.LogInformation("🧩 Role 'Customer' criada com sucesso.");
            }

            // Lista de clientes base
            var customerSeeds = new List<(string Name, string Email, string Phone)>
    {
        ("Julia Rebouças", "julia@email.com", "910000000"),
        ("Maria Silva", "maria@email.com", "910000001"),
        ("Pedro Costa", "pedro@email.com", "920000000")
    };

            foreach (var (name, email, phone) in customerSeeds)
            {
                // Cria usuário Identity
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = email,
                        Email = email,
                        FirstName = name.Split(' ')[0],
                        LastName = name.Split(' ').Length > 1 ? name.Split(' ')[1] : "",
                        PhoneNumber = phone,
                        EmailConfirmed = true,
                        Active = true
                    };

                    var result = await _userManager.CreateAsync(user, "123456"); // senha padrão
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Customer");
                        _logger.LogInformation($"👤 Usuário '{email}' criado com role Customer.");
                    }
                    else
                    {
                        _logger.LogWarning($"⚠️ Falha ao criar usuário {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        continue;
                    }
                }

                // Cria registro Customer vinculado ao User
                if (!await _context.Customers.AnyAsync(c => c.Email == email))
                {
                    var customer = new Customer
                    {
                        Name = name,
                        Email = email,
                        Phone = phone,
                        IsActive = true,
                        UserId = user.Id
                    };

                    _context.Customers.Add(customer);
                    _logger.LogInformation($"🧾 Cliente '{name}' vinculado ao usuário '{email}'.");
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Clientes e usuários Customer criados com sucesso.");
        }


        private async Task SeedTestCustomerIdentityAsync()
        {
            var email = "testecliente@ewellinbeauty.com";
            var userHelper = _serviceProvider.GetRequiredService<IUserHelper>();

            var existingUser = await userHelper.GetUserByEmailAsync(email);
            if (existingUser == null)
            {
                var user = new User
                {
                    FirstName = "Cliente",
                    LastName = "Teste",
                    Email = email,
                    UserName = "cliente.teste",
                    PhoneNumber = "910000001",
                    EmailConfirmed = true,
                    Active = true
                };

                var result = await userHelper.AddUserAsync(user, "Teste@123");
                if (result.Succeeded)
                {
                    await userHelper.AddUserToRoleAsync(user, "Customer");
                    Console.WriteLine("✅ Usuário de teste criado via Identity: testecliente@ewellinbeauty.com / senha: Teste@123");
                }
                else
                {
                    Console.WriteLine($"⚠️ Falha ao criar usuário: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

    }
}
