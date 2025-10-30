using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Sistema.Middleware
{
    public class JwtValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Session.GetString("JwtToken");

            // Se não estiver autenticado e tentar acessar /Admin/
            if (string.IsNullOrEmpty(token) && context.Request.Path.StartsWithSegments("/Admin"))
            {
                context.Response.Redirect("/Account/Login");
                return;
            }

            // Valida o token (JWT real ou token local)
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    // Se for token local, validar via sessão
                    if (token == "local-admin-token")
                    {
                        var role = context.Session.GetString("Role");
                        if (context.Request.Path.StartsWithSegments("/Admin") && role != "Admin")
                        {
                            context.Session.Clear();
                            context.Response.Redirect("/Account/Login");
                            return;
                        }
                    }
                    else
                    {
                        // Validação JWT real (para futuras implementações com API)
                        var jwtHandler = new JwtSecurityTokenHandler();
                        if (!jwtHandler.CanReadToken(token))
                        {
                            context.Session.Clear();
                            context.Response.Redirect("/Account/Login");
                            return;
                        }

                        var jwtToken = jwtHandler.ReadJwtToken(token);
                        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

                        // Bloqueia acesso à área Admin se o token não for Admin
                        if (context.Request.Path.StartsWithSegments("/Admin") && roleClaim != "Admin")
                        {
                            context.Response.Redirect("/Account/Login");
                            return;
                        }
                    }
                }
                catch
                {
                    context.Session.Clear();
                    context.Response.Redirect("/Account/Login");
                    return;
                }
            }

            await _next(context);
        }
    }
}
