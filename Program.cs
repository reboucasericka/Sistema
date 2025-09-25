using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;
using Sistema.Data.Repository.Implementations;

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
            builder.Services.AddDbContext<SistemaDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddControllersWithViews();
            builder.Services.AddTransient<SeedDb>();
            builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
            //builder.Services.AddScoped<IFornecedorRepository, FornecedorRepository>();
            //builder.Services.AddScoped<ICategoriaProdutoRepository, CategoriaProdutoRepository>();
            //builder.Services.AddScoped<ICategoriaServicoRepository, CategoriaServicoRepository>();
            // Helpers e serviços customizados

            //builder.Services.AddScoped<IUserHelper, UserHelper>();
            //builder.Services.AddScoped<IBlobHelper, BlobHelper>();
            //builder.Services.AddScoped<IConverterHelper, ConverterHelper>();
            //builder.Services.AddScoped<IProductRepository, ProductRepository>();

            // Configuração de cookies (login/logout)
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });
            builder.Services.AddAuthorization();


            // MVC
            builder.Services.AddControllersWithViews();

            using var app = builder.Build();

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

            app.UseStatusCodePagesWithReExecute("/error/{0}");
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
