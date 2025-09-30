using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Implementations;
using Sistema.Data.Repository.Interfaces;
using Sistema.Helpers;

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

    // Regras para senhas (você pode ajustar conforme necessário)
    cfg.Password.RequireDigit = false; // Não exige dígitos (números)
    cfg.Password.RequiredUniqueChars = 0; // Não exige caracteres únicos
    cfg.Password.RequireUppercase = false; // Não exige letras maiúsculas
    cfg.Password.RequireLowercase = false; // Não exige letras minúsculas
    cfg.Password.RequireNonAlphanumeric = false; // Não exige caracteres especiais
    cfg.Password.RequiredLength = 6; // Tamanho mínimo da senha

    
    
})
.AddEntityFrameworkStores<SistemaDbContext>() // Armazena os dados do Identity no banco de dados
.AddDefaultTokenProviders(); // Adiciona os provedores de token padrão (necessário para reset de senha)

// 4. Configura o tempo de expiração do token de reset de senha (opcional)
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(3); // Token de reset de senha expira em 3 horas
});

// 5. Configura os cookies de autenticação
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/NotAuthorized"; // Página de login
    options.AccessDeniedPath = "/Account/NotAuthorized"; // Página de acesso negado
});

// 6. Adiciona os serviços personalizados (repositórios, helpers, etc.)
builder.Services.AddTransient<SeedDb>();
builder.Services.AddScoped<IUserHelper, UserHelper>();

builder.Services.AddScoped<IConverterHelper, ConverterHelper>();
// Adicione esta linha no Program.cs, na seção de registro de serviços
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
//builder.Services.AddScoped<IImageHelper, ImageHelper>();
builder.Services.AddScoped<IBlobHelper, BlobHelper>();



//builder.Services.AddScoped<IEmailHelper, EmailHelper>();


// Adicione esta linha no Program.cs, na seção de registro de serviços
builder.Services.AddScoped<SeedDb>();


// 7. Adiciona suporte a controllers e views
builder.Services.AddControllersWithViews();

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
app.UseRouting(); // Habilita o roteamento
app.UseAuthentication(); // Habilita a autenticação
app.UseAuthorization(); // Habilita a autorização

// 10. Configura as rotas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 11. Popula o banco de dados (seed)
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<SeedDb>();
    seeder.SeedAsync().Wait(); // Aguarda a conclusão do seed
}

// 12. Inicia a aplicação
app.Run();
