using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Helpers;
using Sistema.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================================================
// 1️⃣ CONFIGURAÇÃO DE AUTENTICAÇÃO BASEADA EM API
// =====================================================================
// Configuração de autenticação via cookies para manter sessão do usuário
// A autenticação real será feita via API

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
    options.Cookie.Name = "Sistema.Auth";
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
    options.ClientSecret = builder.Configuration["Authentication:Facebook:ClientSecret"];
    options.CallbackPath = "/Account/ExternalLoginCallback";
});

// =====================================================================
// 4️⃣ REGISTRO DE SERVIÇOS E HELPERS
// =====================================================================
// Helpers básicos que não dependem do banco de dados
builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddScoped<IRoleHelper, RoleHelper>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IConverterHelper, ConverterHelper>();
builder.Services.AddScoped<ICashRegisterHelper, CashRegisterHelper>();

// ===== Helpers =====
builder.Services.AddScoped<IImageHelper, ImageHelper>();
builder.Services.AddScoped<IBlobHelper, BlobHelper>();
builder.Services.AddScoped<IStorageHelper, StorageHelper>();

// Serviços de exportação e comunicação
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();
builder.Services.AddScoped<IPdfExportService, PdfExportService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<ICommunicationService, CommunicationService>();
builder.Services.AddScoped<IAppointmentNotificationService, AppointmentNotificationService>();

// Serviços de notificação
builder.Services.AddHostedService<AppointmentReminderService>();

// HttpClient para serviços que fazem requisições HTTP
builder.Services.AddHttpClient();

// HttpClient específico para SistemaAPI
builder.Services.AddHttpClient("SistemaAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7001/api/");
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// Habilitar sessão
builder.Services.AddSession();

// =====================================================================
// 5️⃣ CONFIGURAÇÃO DO BANCO DE DADOS SQLITE
// =====================================================================
builder.Services.AddDbContext<SistemaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Garante apenas que o banco existe (não migra nem recria tabelas)
using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SistemaDbContext>();
    dbContext.Database.EnsureCreated();
}

// Identity com Entity Framework
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Permitir senhas simples (ex: 123456)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3; // mínimo de 3 caracteres
    
    // Configurações de usuário
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<SistemaDbContext>()
.AddDefaultTokenProviders();

// ===== API Integration Services =====
// Configure HttpClient with HTTPS and certificate handling for local development
builder.Services.AddHttpClient<Sistema.Services.Api.IApiClientService, Sistema.Services.Api.ApiClientService>(client =>
{
    var apiSettings = builder.Configuration.GetSection("ApiSettings");
    client.BaseAddress = new Uri(apiSettings["BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(int.Parse(apiSettings["Timeout"] ?? "30"));
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    
    // Ignore certificate errors in development (localhost)
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
    }
    
    return handler;
});

builder.Services.AddScoped<Sistema.Services.Api.IApiClientService, Sistema.Services.Api.ApiClientService>();
builder.Services.AddScoped<Sistema.Services.Api.IApiAppointmentService, Sistema.Services.Api.ApiAppointmentService>();
builder.Services.AddScoped<Sistema.Services.Api.IApiUserService, Sistema.Services.Api.ApiUserService>();

// ===== MVC API Services (New Architecture) =====
// Configure HttpClient for API communication with JWT Authentication and HTTPS
builder.Services.AddHttpClient<Sistema.Services.Auth.ApiAuthService>(client =>
{
    var apiSettings = builder.Configuration.GetSection("ApiSettings");
    client.BaseAddress = new Uri(apiSettings["BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(int.Parse(apiSettings["Timeout"] ?? "30"));
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    
    // Ignore certificate errors in development (localhost)
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
    }
    
    return handler;
});

// Configure all API services with HTTPS
var configureApiClient = (HttpClient client) =>
{
    var apiSettings = builder.Configuration.GetSection("ApiSettings");
    client.BaseAddress = new Uri(apiSettings["BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(int.Parse(apiSettings["Timeout"] ?? "30"));
};

var configureApiHandler = () =>
{
    var handler = new HttpClientHandler();
    
    // Ignore certificate errors in development (localhost)
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
    }
    
    return handler;
};

builder.Services.AddHttpClient<Sistema.Services.Api.ApiClientsService>(configureApiClient)
    .ConfigurePrimaryHttpMessageHandler(configureApiHandler);

builder.Services.AddScoped<Sistema.Services.Api.IApiClientsService, Sistema.Services.Api.ApiClientsService>();
builder.Services.AddScoped<Sistema.Services.Api.IApiSettingsService, Sistema.Services.Api.ApiSettingsService>();
builder.Services.AddScoped<Sistema.Services.Api.IApiNotificationsService, Sistema.Services.Api.ApiNotificationsService>();
builder.Services.AddScoped<Sistema.Services.Api.IApiAppointmentsService, Sistema.Services.Api.ApiAppointmentsService>();
builder.Services.AddScoped<Sistema.Services.Api.IApiServicesService, Sistema.Services.Api.ApiServicesService>();
builder.Services.AddScoped<Sistema.Services.Api.IApiProductsService, Sistema.Services.Api.ApiProductsService>();
builder.Services.AddScoped<Sistema.Services.Api.IApiStaffService, Sistema.Services.Api.ApiStaffService>();
builder.Services.AddScoped<Sistema.Services.Api.IApiPaymentsService, Sistema.Services.Api.ApiPaymentsService>();
builder.Services.AddScoped<Sistema.Services.Api.IApiSuppliersService, Sistema.Services.Api.ApiSuppliersService>();
builder.Services.AddScoped<Sistema.Services.Api.IApiProductCategoriesService, Sistema.Services.Api.ApiProductCategoriesService>();

builder.Services.AddHttpClient<Sistema.Services.Api.ApiAppointmentsService>(configureApiClient)
    .ConfigurePrimaryHttpMessageHandler(configureApiHandler);

builder.Services.AddHttpClient<Sistema.Services.Api.ApiServicesService>(configureApiClient)
    .ConfigurePrimaryHttpMessageHandler(configureApiHandler);

builder.Services.AddHttpClient<Sistema.Services.Api.ApiStaffService>(configureApiClient)
    .ConfigurePrimaryHttpMessageHandler(configureApiHandler);

builder.Services.AddHttpClient<Sistema.Services.Api.ApiPaymentsService>(configureApiClient)
    .ConfigurePrimaryHttpMessageHandler(configureApiHandler);

builder.Services.AddHttpClient<Sistema.Services.Api.ApiProductsService>(configureApiClient)
    .ConfigurePrimaryHttpMessageHandler(configureApiHandler);

builder.Services.AddHttpClient<Sistema.Services.Api.ApiSuppliersService>(configureApiClient)
    .ConfigurePrimaryHttpMessageHandler(configureApiHandler);

builder.Services.AddHttpClient<Sistema.Services.Api.ApiProductCategoriesService>(configureApiClient)
    .ConfigurePrimaryHttpMessageHandler(configureApiHandler);

// Memory Cache for API Gateway
builder.Services.AddMemoryCache();

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// =====================================================================
// 4️⃣ CONFIGURAÇÃO DO CORS PARA MAUI
// =====================================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMauiApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// =====================================================================
// 5️⃣ CONFIGURAÇÃO DO SWAGGER/OPENAPI
// =====================================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Beauty Salon Management API",
        Version = "v1",
        Description = "Complete API for beauty salon management system including appointments, clients, products, and authentication",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "System Support",
            Email = "support@beautysalon.com"
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
    
    // Filtrar apenas controladores de API (ignorar controladores MVC)
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        // Incluir apenas controladores que estão na pasta API
        return apiDesc.RelativePath?.StartsWith("api/") == true;
    });
    
    // Configuração de segurança JWT (se necessário)
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // Incluir comentários XML se existirem
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

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
    
    // =====================================================================
    // SWAGGER UI - APENAS EM DESENVOLVIMENTO
    // =====================================================================
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Beauty Salon API v1");
        c.RoutePrefix = "swagger"; // Define a rota como /swagger
        c.DocumentTitle = "Beauty Salon Management API Documentation";
        c.DefaultModelsExpandDepth(-1); // Oculta os modelos por padrão
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.EnableValidator();
    });
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

// Habilitar sessão
app.UseSession();

// Middleware de validação JWT
app.UseMiddleware<Sistema.Middleware.JwtValidationMiddleware>();

// =====================================================================
// CORS MIDDLEWARE - ANTES DA AUTENTICAÇÃO
// =====================================================================
app.UseCors("AllowMauiApp");

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


// app.MapHub<Sistema.Services.NotificationHub>("/notificationHub"); // Moved to API Business Services

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
// 9️⃣ INICIALIZAÇÃO DO SISTEMA
// =====================================================================
// 8️⃣ SEED DO BANCO DE DADOS
// =====================================================================
// Seeding removido do MVC: responsabilidade da API

// =====================================================================
// O seed de dados agora é responsabilidade da API
// O Sistema MVC apenas consome os dados via API
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("✅ Sistema MVC inicializado - consumindo dados via API");
Console.ResetColor();

// =====================================================================
// 9️⃣ EXECUTA O APP
// =====================================================================
app.Run();
