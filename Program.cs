using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Implementations;
using Sistema.Data.Repository.Interfaces;
using Sistema.Helpers;
using Sistema.Services;

// 1. Cria o builder da aplicação (substitui o antigo Startup.cs)
var builder = WebApplication.CreateBuilder(args);

// 2. Configura o DbContext para conectar ao banco de dados SQL Server
builder.Services.AddDbContext<SistemaDbContext>(cfg =>
    cfg.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Configura o Identity com as regras de senha e email único
builder.Services.AddIdentity<User, IdentityRole>(cfg =>
{
    // Regras para usuários
    cfg.User.RequireUniqueEmail = true; // Exige que cada email seja único

    // Regras para senhas (configuração simplificada para desenvolvimento)
    cfg.Password.RequireDigit = false; // Não exige dígitos (números)
    cfg.Password.RequiredUniqueChars = 0; // Não exige caracteres únicos
    cfg.Password.RequireUppercase = false; // Não exige letras maiúsculas
    cfg.Password.RequireLowercase = false; // Não exige letras minúsculas
    cfg.Password.RequireNonAlphanumeric = false; // Não exige caracteres especiais
    cfg.Password.RequiredLength = 4; // Tamanho mínimo da senha (alterado de 3 para 4)

    
    
})
.AddEntityFrameworkStores<SistemaDbContext>() // Armazena os dados do Identity no banco de dados
.AddDefaultTokenProviders(); // Adiciona os provedores de token padrão (necessário para reset de senha)

// 4. Configura o tempo de expiração do token de reset de senha (opcional)
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(3); // Token de reset de senha expira em 3 horas
});

// 5. Configuração de cookies movida para a seção de autenticação social abaixo

// Configuração de autenticação social (Facebook e Google)
builder.Services.AddAuthentication()
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/NotAuthorized";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    })
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        options.CallbackPath = "/Account/ExternalLoginCallback";
    })
    .AddFacebook(FacebookDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = builder.Configuration["Authentication:Facebook:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Facebook:ClientSecret"];
        options.CallbackPath = "/Account/ExternalLoginCallback";
    });







//// 5.1. Configura autenticação social (Facebook e Google)
//builder.Services.AddAuthentication()
//    .AddFacebook("Facebook", options =>
//    {
//        options.ClientId = builder.Configuration["Authentication:Facebook:ClientId"];
//        options.ClientSecret = builder.Configuration["Authentication:Facebook:ClientSecret"];
//        options.CallbackPath = "/Account/FacebookCallback";
//    })
//    .AddGoogle("Google", options =>
//    {
//        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
//        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
//        options.CallbackPath = "/Account/GoogleCallback";
//    });

// 6. Adiciona os serviços personalizados (repositórios, helpers, etc.)
builder.Services.AddTransient<SeedDb>();
builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddScoped<IRoleHelper, RoleHelper>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<IConverterHelper, ConverterHelper>();
// Adicione esta linha no Program.cs, na seção de registro de serviços
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IPriceTableRepository, PriceTableRepository>();
builder.Services.AddScoped<IImageHelper, ImageHelper>();
builder.Services.AddScoped<IBlobHelper, BlobHelper>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<INotificationService, NotificationService>();



//builder.Services.AddScoped<IEmailHelper, EmailHelper>();


// Adicione esta linha no Program.cs, na seção de registro de serviços
builder.Services.AddScoped<SeedDb>();


// 7. Adiciona suporte a controllers e views
builder.Services.AddControllersWithViews();

// 7.1. Adiciona SignalR para notificações em tempo real
builder.Services.AddSignalR();

// 7.1. Configura cultura portuguesa para formatação de moeda
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "pt-PT" };
    options.SetDefaultCulture("pt-PT");
    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedCultures);
});

// 8. Constrói a aplicação
var app = builder.Build();

// 9. Configura o pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Página de erro detalhada em desenvolvimento
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Página de erro genérica em produção
    app.UseHsts(); // Security headers para HTTPS
}

app.UseStatusCodePagesWithReExecute("/error/{0}"); // Tratamento de erros HTTP
app.UseHttpsRedirection(); // Redireciona HTTP para HTTPS
app.UseStaticFiles(); // Permite o uso de arquivos estáticos (CSS, JS, imagens)

// Middleware para controlar cache em páginas de autenticação
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();
    
    // Aplicar headers de no-cache para páginas de autenticação
    if (path != null && (path.Contains("/account") || path.Contains("/admin")))
    {
        context.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
        context.Response.Headers.Add("Pragma", "no-cache");
        context.Response.Headers.Add("Expires", "0");
    }
    
    await next();
});

app.UseRouting(); // Habilita o roteamento
app.UseAuthentication(); // Habilita a autenticação
app.UseAuthorization(); // Habilita a autorização
app.UseRequestLocalization(); // Habilita a localização

// 10. Configura as rotas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mapear SignalR Hub
app.MapHub<NotificationHub>("/notificationHub");

// 11. Valida configuração AdminUser antes do seed
var adminSection = builder.Configuration.GetSection("AdminUser");
var adminUserName = adminSection["UserName"];
var adminEmail = adminSection["Email"];
var adminPassword = adminSection["Password"];

if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword) || string.IsNullOrEmpty(adminUserName))
{
    Console.WriteLine("❌ ERROR: AdminUser configuration is missing!");
    Console.WriteLine("   Required fields: UserName, Email, Password");
    Console.WriteLine("   Please configure AdminUser section in user-secrets or appsettings");
    throw new InvalidOperationException("⚠️ AdminUser configuration is missing. Please configure it in user-secrets or appsettings.");
}

Console.WriteLine("✅ AdminUser configuration validated successfully");

// 12. Popula o banco de dados (seed)
using (var scope = app.Services.CreateScope())
{
    var seedDb = scope.ServiceProvider.GetRequiredService<SeedDb>();
    await seedDb.SeedAsync(builder.Configuration);// Aguarda a conclusão do seed
}

// 12. Inicia a aplicação
app.Run();
