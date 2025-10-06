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
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private Random _random;
        


        public SeedDb(SistemaDbContext context, IUserHelper userHelper, UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userHelper = userHelper;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;          
            _random = new Random();
        }

        public async Task SeedAsync(IConfiguration configuration)
        {
            Console.WriteLine("🌱 Starting database seed...");
            
            // 1. Ensure database exists
            //await _context.Database.MigrateAsync();
            //await _context.Database.EnsureCreatedAsync();

            // Create roles if they don't exist
            Console.WriteLine("🔧 Checking/creating roles...");
            await _userHelper.CheckRoleAsync("Admin"); // Check if admin role exists, create if not
            await _userHelper.CheckRoleAsync("Customer"); // Check if customer role exists, create if not
            await _userHelper.CheckRoleAsync("Professional"); // Check if professional role exists, create if not

            // 2. Load initial admin config from configuration (appsettings, user-secrets, or environment variables)
            var adminSection = configuration.GetSection("AdminUser");
            string adminUserName = adminSection["UserName"];
            string adminEmail = adminSection["Email"];
            string adminPassword = adminSection["Password"];

            // Validate configuration - AdminUser section is REQUIRED
            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword) || string.IsNullOrEmpty(adminUserName))
            {
                Console.WriteLine("❌ ERROR: AdminUser configuration is incomplete or missing!");
                Console.WriteLine("   Required fields: UserName, Email, Password");
                Console.WriteLine("   Please configure AdminUser section in user-secrets or appsettings");
                throw new InvalidOperationException("⚠️ AdminUser configuration is missing. Please configure it in user-secrets or appsettings.");
            }

            Console.WriteLine($"🔧 Admin configuration loaded:");
            Console.WriteLine($"   - Username: {adminUserName}");
            Console.WriteLine($"   - Email: {adminEmail}");
            Console.WriteLine($"   - Password: {(string.IsNullOrEmpty(adminPassword) ? "EMPTY" : "CONFIGURED")}");

            // 3. Verificar se usuário admin já existe
            Console.WriteLine("🔍 Verificando usuário admin existente...");
            
            var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
            
            if (existingAdmin != null)
            {
                Console.WriteLine($"✅ Usuário admin encontrado: {existingAdmin.Email}");
                
                // Verificar se está na role Admin
                var isAdmin = await _userManager.IsInRoleAsync(existingAdmin, "Admin");
                if (isAdmin)
                {
                    Console.WriteLine("✅ Usuário já está na role Admin - mantendo existente");
                    return; // Usuário já existe e está correto
                }
                else
                {
                    Console.WriteLine("⚠️ Usuário existe mas não tem role Admin - adicionando role");
                    var roleResult = await _userManager.AddToRoleAsync(existingAdmin, "Admin");
                    if (roleResult.Succeeded)
                    {
                        Console.WriteLine("✅ Role Admin adicionada ao usuário existente");
                        return;
                    }
                    else
                    {
                        Console.WriteLine($"❌ Erro ao adicionar role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
            }
            else
            {
                Console.WriteLine("ℹ️ Usuário admin não encontrado - criando novo");
            }

            // 4. Create new admin user with fresh password from configuration
            Console.WriteLine("👤 Creating new administrator user with fresh password...");
            Console.WriteLine($"📧 Email being used: {adminEmail}");
            Console.WriteLine($"👤 Username being used: {adminUserName}");
            Console.WriteLine("🔐 Password reset confirmation: Using password from configuration");
            
            var user = new User
            {
                FirstName = "Admin",
                LastName = "System",
                Email = adminEmail,
                UserName = adminUserName,
                PhoneNumber = "000000000",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, adminPassword);
            if (result.Succeeded)
            {
                Console.WriteLine("✅ Administrator user created successfully!");
                Console.WriteLine($"   - User ID: {user.Id}");
                Console.WriteLine($"   - Email: {user.Email}");
                Console.WriteLine($"   - Username: {user.UserName}");
                Console.WriteLine($"   - Password: Using password from configuration");
                
                // Assign Admin role using UserManager
                var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                if (roleResult.Succeeded)
                {
                    Console.WriteLine("✅ Role assignment: Admin role assigned to user!");
                }
                else
                {
                    Console.WriteLine($"⚠️ Warning when assigning role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
                
                // Verify the user was created correctly
                var createdUser = await _userManager.FindByEmailAsync(adminEmail);
                if (createdUser != null)
                {
                    var isAdmin = await _userManager.IsInRoleAsync(createdUser, "Admin");
                    Console.WriteLine($"✅ Verification: User exists and is Admin: {isAdmin}");
                    Console.WriteLine($"🔐 Password: Using password from configuration (test login via AccountController)");
                }
            }
            else
            {
                Console.WriteLine($"❌ Error creating user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                Console.WriteLine("🔍 Detailed error analysis:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"   - Code: {error.Code}, Description: {error.Description}");
                }
                
                // Check if it's a password complexity issue
                var passwordErrors = result.Errors.Where(e => e.Code.Contains("Password") || e.Description.Contains("password")).ToList();
                if (passwordErrors.Any())
                {
                    Console.WriteLine("🔐 Password complexity issues detected:");
                    foreach (var error in passwordErrors)
                    {
                        Console.WriteLine($"   - {error.Description}");
                    }
                    Console.WriteLine("💡 Suggestion: Check password requirements in Program.cs or use a stronger password");
                }
                
                throw new InvalidOperationException($"❌ Could not create administrator user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            
            // 5. Seed example customers
            Console.WriteLine("👥 Seeding example customers...");
            await SeedCustomersAsync();
            
            // 6. Seed example professionals
            Console.WriteLine("👨‍⚕️ Seeding example professionals...");
            await SeedProfessionalsAsync();
            
            // 7. Seed service categories and services
            Console.WriteLine("🏷️ Seeding service categories and services...");
            await SeedCategoriesAsync();
            await SeedServicesAsync();
            
            // 8. Seed professional schedules
            Console.WriteLine("📅 Seeding professional schedules...");
            await SeedProfessionalSchedulesAsync();
            
            // 9. Seed sample appointments
            Console.WriteLine("📋 Seeding sample appointments...");
            await SeedSampleAppointmentsAsync();
            
            Console.WriteLine("🌱 Database seed completed!");
        }

        private async Task SeedCustomersAsync()
        {
            // Check if customers already exist
            if (await _context.Customers.AnyAsync())
            {
                Console.WriteLine("✅ Customers already exist - skipping customer seed");
                return;
            }

            var customers = new List<Customer>
            {
                new Customer
                {
                    Name = "Maria Silva",
                    Email = "maria.silva@email.com",
                    Phone = "+351 912 345 678",
                    Address = "Rua das Flores, 123, 1000-001 Lisboa",
                    BirthDate = new DateTime(1985, 3, 15),
                    RegistrationDate = DateTime.Now.AddDays(-30),
                    IsActive = true,
                    Notes = "Prefers morning appointments",
                    AllergyHistory = "No known allergies"
                },
                new Customer
                {
                    Name = "João Santos",
                    Email = "joao.santos@email.com",
                    Phone = "+351 923 456 789",
                    Address = "Avenida da Liberdade, 456, 1250-096 Lisboa",
                    BirthDate = new DateTime(1990, 7, 22),
                    RegistrationDate = DateTime.Now.AddDays(-25),
                    IsActive = true,
                    Notes = "Regular customer, very satisfied with services",
                    AllergyHistory = "Sensitive to certain fragrances"
                },
                new Customer
                {
                    Name = "Ana Costa",
                    Email = "ana.costa@email.com",
                    Phone = "+351 934 567 890",
                    Address = "Rua Augusta, 789, 1100-053 Lisboa",
                    BirthDate = new DateTime(1988, 11, 8),
                    RegistrationDate = DateTime.Now.AddDays(-20),
                    IsActive = true,
                    Notes = "First-time customer, interested in facial treatments",
                    AllergyHistory = "No known allergies"
                },
                new Customer
                {
                    Name = "Pedro Oliveira",
                    Email = "pedro.oliveira@email.com",
                    Phone = "+351 945 678 901",
                    Address = "Praça do Comércio, 1, 1100-148 Lisboa",
                    BirthDate = new DateTime(1992, 1, 30),
                    RegistrationDate = DateTime.Now.AddDays(-15),
                    IsActive = true,
                    Notes = "Prefers weekend appointments",
                    AllergyHistory = "No known allergies"
                },
                new Customer
                {
                    Name = "Carla Ferreira",
                    Email = "carla.ferreira@email.com",
                    Phone = "+351 956 789 012",
                    Address = "Rua de São Bento, 234, 1200-109 Lisboa",
                    BirthDate = new DateTime(1987, 9, 12),
                    RegistrationDate = DateTime.Now.AddDays(-10),
                    IsActive = true,
                    Notes = "VIP customer, always books premium services",
                    AllergyHistory = "Allergic to latex"
                },
                new Customer
                {
                    Name = "Miguel Rodrigues",
                    Email = "miguel.rodrigues@email.com",
                    Phone = "+351 967 890 123",
                    Address = "Avenida de Roma, 567, 1000-191 Lisboa",
                    BirthDate = new DateTime(1995, 5, 18),
                    RegistrationDate = DateTime.Now.AddDays(-5),
                    IsActive = true,
                    Notes = "New customer, interested in men's grooming services",
                    AllergyHistory = "No known allergies"
                },
                new Customer
                {
                    Name = "Sofia Almeida",
                    Email = "sofia.almeida@email.com",
                    Phone = "+351 978 901 234",
                    Address = "Rua Garrett, 890, 1200-203 Lisboa",
                    BirthDate = new DateTime(1991, 12, 3),
                    RegistrationDate = DateTime.Now.AddDays(-2),
                    IsActive = true,
                    Notes = "Prefers afternoon appointments",
                    AllergyHistory = "Sensitive skin, needs gentle products"
                },
                new Customer
                {
                    Name = "Ricardo Pereira",
                    Email = "ricardo.pereira@email.com",
                    Phone = "+351 989 012 345",
                    Address = "Rua do Carmo, 123, 1200-092 Lisboa",
                    BirthDate = new DateTime(1989, 8, 25),
                    RegistrationDate = DateTime.Now.AddDays(-1),
                    IsActive = false,
                    Notes = "Inactive customer - moved to another city",
                    AllergyHistory = "No known allergies"
                }
            };

            _context.Customers.AddRange(customers);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"✅ Seeded {customers.Count} example customers");
        }

        private async Task SeedProfessionalsAsync()
        {
            // Check if professionals already exist
            if (await _context.Professionals.AnyAsync())
            {
                Console.WriteLine("✅ Professionals already exist - skipping professional seed");
                return;
            }

            // First, create some professional users
            var professionalUsers = new List<User>
            {
                new User
                {
                    FirstName = "Maria",
                    LastName = "Alves",
                    Email = "maria.alves@ewellin.com",
                    UserName = "maria.alves@ewellin.com",
                    PhoneNumber = "+351 912 345 678",
                    Active = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FirstName = "João",
                    LastName = "Pereira",
                    Email = "joao.pereira@ewellin.com",
                    UserName = "joao.pereira@ewellin.com",
                    PhoneNumber = "+351 923 456 789",
                    Active = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FirstName = "Carla",
                    LastName = "Ribeiro",
                    Email = "carla.ribeiro@ewellin.com",
                    UserName = "carla.ribeiro@ewellin.com",
                    PhoneNumber = "+351 934 567 890",
                    Active = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FirstName = "Ana",
                    LastName = "Costa",
                    Email = "ana.costa@ewellin.com",
                    UserName = "ana.costa@ewellin.com",
                    PhoneNumber = "+351 945 678 901",
                    Active = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FirstName = "Rita",
                    LastName = "Sousa",
                    Email = "rita.sousa@ewellin.com",
                    UserName = "rita.sousa@ewellin.com",
                    PhoneNumber = "+351 956 789 012",
                    Active = true,
                    CreatedAt = DateTime.Now
                }
            };

            // Create users and assign Professional role
            foreach (var user in professionalUsers)
            {
                var result = await _userManager.CreateAsync(user, "Professional123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Professional");
                    Console.WriteLine($"✅ Created professional user: {user.Email}");
                }
                else
                {
                    Console.WriteLine($"❌ Failed to create professional user {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // Now create professional profiles
            var professionals = new List<Professional>
            {
                new Professional
                {
                    Name = "Maria Alves",
                    UserId = professionalUsers[0].Id,
                    Specialty = "Facial Treatments",
                    DefaultCommission = 15.0m,
                    IsActive = true
                },
                new Professional
                {
                    Name = "João Pereira",
                    UserId = professionalUsers[1].Id,
                    Specialty = "Hair Styling",
                    DefaultCommission = 20.0m,
                    IsActive = true
                },
                new Professional
                {
                    Name = "Carla Ribeiro",
                    UserId = professionalUsers[2].Id,
                    Specialty = "Nail Art",
                    DefaultCommission = 12.0m,
                    IsActive = false
                },
                new Professional
                {
                    Name = "Ana Costa",
                    UserId = professionalUsers[3].Id,
                    Specialty = "Massage Therapy",
                    DefaultCommission = 18.0m,
                    IsActive = true
                },
                new Professional
                {
                    Name = "Rita Sousa",
                    UserId = professionalUsers[4].Id,
                    Specialty = "Makeup Artist",
                    DefaultCommission = 25.0m,
                    IsActive = true
                }
            };

            _context.Professionals.AddRange(professionals);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"✅ Seeded {professionals.Count} example professionals");
        }

        private async Task SeedCategoriesAsync()
        {
            // Check if categories already exist
            if (await _context.Categories.AnyAsync())
            {
                Console.WriteLine("✅ Categories already exist - skipping category seed");
                return;
            }

            var categories = new List<Category>
            {
                new Category { Name = "Eyebrows", Description = "Professional eyebrow design and treatments" },
                new Category { Name = "Eyelashes", Description = "Eyelash extensions and treatments" },
                new Category { Name = "Makeup", Description = "Professional makeup and consultation" },
                new Category { Name = "Facial Treatments", Description = "Facial cleansing, hydration and treatments" },
                new Category { Name = "Massage", Description = "Relaxing and therapeutic massages" },
                new Category { Name = "Nails", Description = "Manicure, pedicure and nail art" },
                new Category { Name = "Hair", Description = "Hair cutting, styling, coloring and treatments" },
                new Category { Name = "Waxing", Description = "Waxing and hair removal services" }
            };

            _context.Categories.AddRange(categories);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"✅ Seeded {categories.Count} categories");
        }

        private async Task SeedServicesAsync()
        {
            // Check if services already exist
            if (await _context.Service.AnyAsync())
            {
                Console.WriteLine("✅ Services already exist - skipping service seed");
                return;
            }

            var categories = await _context.Categories.ToListAsync();
            var eyebrowsCategory = categories.FirstOrDefault(c => c.Name == "Eyebrows");
            var eyelashesCategory = categories.FirstOrDefault(c => c.Name == "Eyelashes");
            var makeupCategory = categories.FirstOrDefault(c => c.Name == "Makeup");
            var facialCategory = categories.FirstOrDefault(c => c.Name == "Facial Treatments");
            var massageCategory = categories.FirstOrDefault(c => c.Name == "Massage");
            var nailsCategory = categories.FirstOrDefault(c => c.Name == "Nails");
            var hairCategory = categories.FirstOrDefault(c => c.Name == "Hair");
            var waxingCategory = categories.FirstOrDefault(c => c.Name == "Waxing");

            var services = new List<Service>
            {
                new Service
                {
                    Name = "Eyebrow Design",
                    CategoryId = eyebrowsCategory?.Id ?? 1,
                    Price = 15.00m,
                    Duration = "30 min",
                    Description = "Professional eyebrow shaping and design",
                    IsActive = true,
                    PhotoPath = "brows1.jpg"
                },
                new Service
                {
                    Name = "Eyebrow Henna",
                    CategoryId = eyebrowsCategory?.Id ?? 1,
                    Price = 20.00m,
                    Duration = "45 min",
                    Description = "Henna tinting for eyebrows",
                    IsActive = true,
                    PhotoPath = "brows2.jpg"
                },
                new Service
                {
                    Name = "Microblading",
                    CategoryId = eyebrowsCategory?.Id ?? 1,
                    Price = 80.00m,
                    Duration = "2h",
                    Description = "Semi-permanent eyebrow tattooing",
                    IsActive = true,
                    PhotoPath = "brows3.jpg"
                },
                new Service
                {
                    Name = "Eyelash Extensions",
                    CategoryId = eyelashesCategory?.Id ?? 2,
                    Price = 35.00m,
                    Duration = "1h 30min",
                    Description = "Individual eyelash extensions",
                    IsActive = true,
                    PhotoPath = "lashes1.jpg"
                },
                new Service
                {
                    Name = "Eyelash Lift",
                    CategoryId = eyelashesCategory?.Id ?? 2,
                    Price = 25.00m,
                    Duration = "1h",
                    Description = "Eyelash lifting and curling treatment",
                    IsActive = true,
                    PhotoPath = "lashes2.jpg"
                },
                new Service
                {
                    Name = "Social Makeup",
                    CategoryId = makeupCategory?.Id ?? 3,
                    Price = 30.00m,
                    Duration = "1h",
                    Description = "Professional makeup for special occasions",
                    IsActive = true,
                    PhotoPath = "makeup1.jpg"
                },
                new Service
                {
                    Name = "Bridal Makeup",
                    CategoryId = makeupCategory?.Id ?? 3,
                    Price = 80.00m,
                    Duration = "2h",
                    Description = "Complete bridal makeup package",
                    IsActive = true,
                    PhotoPath = "makeup2.jpg"
                },
                new Service
                {
                    Name = "Facial Cleansing",
                    CategoryId = facialCategory?.Id ?? 4,
                    Price = 40.00m,
                    Duration = "1h",
                    Description = "Deep facial cleansing treatment",
                    IsActive = true,
                    PhotoPath = "facial1.jpg"
                },
                new Service
                {
                    Name = "Facial Hydration",
                    CategoryId = facialCategory?.Id ?? 4,
                    Price = 35.00m,
                    Duration = "45 min",
                    Description = "Intensive facial hydration treatment",
                    IsActive = true,
                    PhotoPath = "facial2.jpg"
                },
                new Service
                {
                    Name = "Relaxing Massage",
                    CategoryId = massageCategory?.Id ?? 5,
                    Price = 50.00m,
                    Duration = "1h",
                    Description = "Therapeutic relaxing massage",
                    IsActive = true,
                    PhotoPath = "massage1.jpg"
                },
                new Service
                {
                    Name = "Complete Manicure",
                    CategoryId = nailsCategory?.Id ?? 6,
                    Price = 20.00m,
                    Duration = "45 min",
                    Description = "Complete manicure with nail polish",
                    IsActive = true,
                    PhotoPath = "nails1.jpg"
                },
                new Service
                {
                    Name = "Complete Pedicure",
                    CategoryId = nailsCategory?.Id ?? 6,
                    Price = 25.00m,
                    Duration = "1h",
                    Description = "Complete pedicure with nail polish",
                    IsActive = true,
                    PhotoPath = "nails2.jpg"
                },
                new Service
                {
                    Name = "Hair Cut & Style",
                    CategoryId = hairCategory?.Id ?? 7,
                    Price = 25.00m,
                    Duration = "45 min",
                    Description = "Professional hair cutting and styling",
                    IsActive = true,
                    PhotoPath = "hair1.jpg"
                },
                new Service
                {
                    Name = "Hair Coloring",
                    CategoryId = hairCategory?.Id ?? 7,
                    Price = 60.00m,
                    Duration = "2h",
                    Description = "Professional hair coloring service",
                    IsActive = true,
                    PhotoPath = "hair2.jpg"
                },
                new Service
                {
                    Name = "Waxing Service",
                    CategoryId = waxingCategory?.Id ?? 8,
                    Price = 15.00m,
                    Duration = "30 min",
                    Description = "Professional waxing hair removal",
                    IsActive = true,
                    PhotoPath = "wax1.jpg"
                }
            };

            _context.Service.AddRange(services);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"✅ Seeded {services.Count} services");
        }

        private async Task SeedProfessionalSchedulesAsync()
        {
            // Check if professional schedules already exist
            if (await _context.ProfessionalSchedules.AnyAsync())
            {
                Console.WriteLine("✅ Professional schedules already exist - skipping schedule seed");
                return;
            }

            var professionals = await _context.Professionals.Where(p => p.IsActive).ToListAsync();
            var schedules = new List<ProfessionalSchedule>();

            foreach (var professional in professionals)
            {
                // Monday to Friday: 9:00 - 18:00
                for (int day = 1; day <= 5; day++)
                {
                    schedules.Add(new ProfessionalSchedule
                    {
                        ProfessionalId = professional.ProfessionalId,
                        DayOfWeek = (DayOfWeek)day,
                        StartTime = new TimeSpan(9, 0, 0),
                        EndTime = new TimeSpan(18, 0, 0)
                    });
                }

                // Saturday: 9:00 - 15:00
                schedules.Add(new ProfessionalSchedule
                {
                    ProfessionalId = professional.ProfessionalId,
                    DayOfWeek = DayOfWeek.Saturday,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(15, 0, 0)
                });

                // Sunday: Closed (no schedule added)
            }

            _context.ProfessionalSchedules.AddRange(schedules);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"✅ Seeded {schedules.Count} professional schedules");
        }

        private async Task SeedSampleAppointmentsAsync()
        {
            // Check if appointments already exist
            if (await _context.Appointments.AnyAsync())
            {
                Console.WriteLine("✅ Appointments already exist - skipping appointment seed");
                return;
            }

            var customers = await _context.Customers.Where(c => c.IsActive).Take(5).ToListAsync();
            var professionals = await _context.Professionals.Where(p => p.IsActive).Take(3).ToListAsync();
            var services = await _context.Service.Where(s => s.IsActive).Take(5).ToListAsync();

            if (!customers.Any() || !professionals.Any() || !services.Any())
            {
                Console.WriteLine("⚠️ Not enough data to create sample appointments - skipping");
                return;
            }

            var appointments = new List<Appointment>();
            var baseDate = DateTime.Now.AddDays(-7); // Start from a week ago

            // Create appointments for the past week and next week
            for (int i = 0; i < 15; i++)
            {
                var appointmentDate = baseDate.AddDays(i);
                var customer = customers[_random.Next(customers.Count)];
                var professional = professionals[_random.Next(professionals.Count)];
                var service = services[_random.Next(services.Count)];

                // Only create appointments on weekdays and during business hours
                if (appointmentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    var startTime = appointmentDate.Date.AddHours(9 + _random.Next(8)); // 9 AM to 5 PM
                    var endTime = startTime.AddMinutes(service.DurationInMinutes);

                    var statuses = new[] { "Pending", "Confirmed", "Completed", "Canceled" };
                    var status = statuses[_random.Next(statuses.Length)];

                    // Don't create future canceled appointments
                    if (startTime > DateTime.Now && status == "Canceled")
                    {
                        status = "Pending";
                    }

                    appointments.Add(new Appointment
                    {
                        CustomerId = customer.CustomerId,
                        ProfessionalId = professional.ProfessionalId,
                        ServiceId = service.Id,
                        StartTime = startTime,
                        EndTime = endTime,
                        Status = status,
                        TotalPrice = service.Price,
                        Notes = i % 3 == 0 ? "Customer requested specific time slot" : null,
                        IsActive = true
                    });
                }
            }

            _context.Appointments.AddRange(appointments);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"✅ Seeded {appointments.Count} sample appointments");
        }

    }
}