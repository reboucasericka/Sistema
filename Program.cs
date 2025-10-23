using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Implementations;
using Sistema.Data.Repository.Interfaces;
using Sistema.Helpers;
using Sistema.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================================================
// 1️⃣ CONFIGURAÇÃO DE BASE DE DADOS E IDENTITY
// =====================================================================
builder.Services.AddDbContext<SistemaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>(cfg =>
{
    cfg.User.RequireUniqueEmail = true;

    // Regras de senha simples (desenvolvimento/produção ajustável)
    cfg.Password.RequireDigit = false;
    cfg.Password.RequiredUniqueChars = 0;
    cfg.Password.RequireUppercase = false;
    cfg.Password.RequireLowercase = false;
    cfg.Password.RequireNonAlphanumeric = false;
    cfg.Password.RequiredLength = 4;
})
.AddEntityFrameworkStores<SistemaDbContext>()
.AddDefaultTokenProviders();

// Tempo de expiração de tokens (ex: recuperação de senha)
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(3);
});

// =====================================================================
// 2️⃣ CONFIGURAÇÃO DE COOKIES DE AUTENTICAÇÃO
// =====================================================================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";  // admin (padrão)
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ReturnUrlParameter = "ReturnUrl";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.Name = "EwellinBeauty.Auth";
    options.Cookie.IsEssential = true;
    
    // Configuração para redirecionamento inteligente baseado na área
    options.Events.OnRedirectToLogin = context =>
    {
        var path = context.Request.Path.Value?.ToLower();
        
        // Se está tentando acessar área Public, redireciona para login público
        if (path != null && path.StartsWith("/public"))
        {
            context.Response.Redirect("/Public/PublicAccount/Login");
        }
        else
        {
            // Para todas as outras áreas (Admin, etc.), usa o login padrão
            context.Response.Redirect("/Account/Login");
        }
        
        return Task.CompletedTask;
    };
});

// =====================================================================
// 3️⃣ AUTENTICAÇÃO SOCIAL (GOOGLE / FACEBOOK)
// =====================================================================
builder.Services.AddAuthentication()
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/Account/ExternalLoginCallback";
})
.AddFacebook(FacebookDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["Authentication:Facebook:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Facebook:clientSecret"];
    options.CallbackPath = "/Account/ExternalLoginCallback";
});

// =====================================================================
// 4️⃣ REGISTRO DE SERVIÇOS, HELPERS E REPOSITÓRIOS
// =====================================================================
builder.Services.AddTransient<SeedDb>();

builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddScoped<IRoleHelper, RoleHelper>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IConverterHelper, ConverterHelper>();
builder.Services.AddScoped<ICashRegisterHelper, CashRegisterHelper>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IPriceTableRepository, PriceTableRepository>();
builder.Services.AddScoped<IPayableRepository, PayableRepository>();
builder.Services.AddScoped<IReceivableRepository, ReceivableRepository>();
builder.Services.AddScoped<IProfessionalScheduleRepository, ProfessionalScheduleRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProfessionalRepository, ProfessionalRepository>();

// ===== Helpers =====
builder.Services.AddScoped<IImageHelper, ImageHelper>();
builder.Services.AddScoped<IBlobHelper, BlobHelper>();

builder.Services.AddScoped<IStorageHelper, StorageHelper>();


builder.Services.AddScoped<IExcelExportService, ExcelExportService>();
builder.Services.AddScoped<IPdfExportService, PdfExportService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Serviços de comunicação
builder.Services.AddScoped<ICommunicationService, CommunicationService>();
builder.Services.AddScoped<IAppointmentNotificationService, AppointmentNotificationService>();

// Serviços Google Calendar e Lembretes
//builder.Services.AddSingleton<IGoogleCalendarSyncService, GoogleCalendarSyncService>();
builder.Services.AddScoped<IGoogleCalendarSyncService, GoogleCalendarSyncService>();

builder.Services.AddHostedService<AppointmentReminderService>();

// HttpClient para serviços que fazem requisições HTTP
builder.Services.AddHttpClient();

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Cultura padrão pt-PT
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultures = new[] { "pt-PT" };
    options.SetDefaultCulture("pt-PT");
    options.AddSupportedCultures(cultures);
    options.AddSupportedUICultures(cultures);
});



// =====================================================================
// 5️⃣ CONSTRUÇÃO DO APLICATIVO
// =====================================================================
var app = builder.Build();

// =====================================================================
// 6️⃣ CONFIGURAÇÃO DO PIPELINE HTTP
// =====================================================================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Middleware de debug para rotas (ANTES da autenticação)
app.Use(async (context, next) =>
{
    Console.WriteLine($"➡️ Rota chamada: {context.Request.Path}");
    Console.WriteLine($"🔐 User Authenticated: {context.User.Identity?.IsAuthenticated}");
    Console.WriteLine($"🔐 User Roles: {string.Join(", ", context.User.Claims.Where(c => c.Type.Contains("role")).Select(c => c.Value))}");
    await next();
});

app.UseAuthentication();
app.UseAuthorization();



// Middleware: evita cache em páginas de autenticação e área admin
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();
    if (path != null && (path.Contains("/account") || path.Contains("/admin")))
    {
        context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
        context.Response.Headers.Append("Pragma", "no-cache");
        context.Response.Headers.Append("Expires", "0");
    }
    await next();
});


app.UseRequestLocalization();




// =====================================================================
// 7️⃣ ROTAS MVC + ÁREAS (CORRIGIDAS E NORMALIZADAS)
// =====================================================================

// 1️⃣ ÁREAS — sempre primeiro
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// 2️⃣ ROTA PADRÃO (Admin e pública) — por último
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.MapHub<Sistema.Services.NotificationHub>("/notificationHub");

// =====================================================================
// 8️⃣ LOGS DE ENDPOINTS (DEBUG)
// =====================================================================
var endpointDataSource = app.Services.GetRequiredService<EndpointDataSource>();
Console.WriteLine("🛠️ ===== ROTAS REGISTRADAS =====");
foreach (var endpoint in endpointDataSource.Endpoints)
{
    Console.WriteLine($"🛠️ Rota registrada: {endpoint.DisplayName}");
}
Console.WriteLine("🛠️ ================================");

// =====================================================================
// 9️⃣ SEED AUTOMÁTICO (modo produção avançado)
// =====================================================================
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var configuration = services.GetRequiredService<IConfiguration>();
        var seeder = services.GetRequiredService<SeedDb>();

        // Valida se as credenciais estão configuradas
        var adminSection = configuration.GetSection("AdminUser");
        var email = adminSection["Email"];
        var password = adminSection["Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(" AdminUser configuration is missing in appsettings or user-secrets!");
            Console.ResetColor();
            throw new InvalidOperationException("Missing AdminUser configuration.");
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(" AdminUser configuration validated. Running Seed...");
        Console.ResetColor();

        await seeder.SeedAsync();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ SeedDb executado com sucesso");
        Console.ResetColor();
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"⚠️ SeedDb falhou: {ex.Message}");
    Console.WriteLine("⚠️ Sistema continuará sem dados iniciais. Configure manualmente se necessário.");
    Console.ResetColor();
}

// =====================================================================
// 9️⃣ EXECUTA O APP
// =====================================================================
app.Run();
