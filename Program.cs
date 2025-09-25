using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Implementations;
using Sistema.Data.Repository.Interfaces;
using Sistema.Helpers;

namespace Sistema
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ======================
            // 1. Serviços (ConfigureServices antigo)
            // ======================

            // DbContext

            builder.Services.AddIdentity<Usuario, IdentityRole>(cfg =>
            {
                cfg.User.RequireUniqueEmail = true;
                cfg.Password.RequireDigit = false;
                cfg.Password.RequiredUniqueChars = 0;
                cfg.Password.RequireLowercase = false;
                cfg.Password.RequireNonAlphanumeric = false;
                cfg.Password.RequireUppercase = false;
                cfg.Password.RequiredLength = 6;

            })
              .AddEntityFrameworkStores<SistemaDbContext>();

            builder.Services.AddDbContext<SistemaDbContext>(cfg =>
                cfg.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            
            builder.Services.AddTransient<SeedDb>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
            builder.Services.AddScoped<IFornecedorRepository, FornecedorRepository>();              
            builder.Services.AddScoped<ICategoriaProdutoRepository, CategoriaProdutoRepository>();



            // Helpers e serviços customizados
            builder.Services.AddScoped<IUsuarioHelper, UsuarioHelper>();
            builder.Services.AddScoped<IImageHelper, ImageHelper>();
            builder.Services.AddScoped<IConverterHelper, ConverterHelper>();

            //builder.Services.AddScoped<ICategoriaServicoRepository, CategoriaServicoRepository>();



            //builder.Services.AddScoped<IBlobHelper, BlobHelper>();





            builder.Services.AddControllersWithViews();

            // Configuração de cookies (login/logout)
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(cfg =>
                {
                    cfg.LoginPath = "/Account/Login";
                    cfg.AccessDeniedPath = "/Account/AccessDenied";
                });
            builder.Services.AddAuthorization();


            // MVC
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // ======================
            // 2. Seed inicial (Roles + Admin)
            // ======================
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var seeder = services.GetRequiredService<SeedDb>();
                await seeder.SeedAsync();
            }

            // ======================
            // 3. Pipeline (Configure antigo)
            // ======================
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }



            // ======================
            // 4. Chamar os serviços
            // ======================
            //app.UseStatusCodePagesWithReExecute("/error/{0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
