using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sistema.Data.Entities;
using Sistema.Models.Account;

namespace Sistema.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<User> userManager, 
            SignInManager<User> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Admin", new { area = "Admin" });

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            try
            {
                if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                {
                    TempData["ErrorMessage"] = "E-mail e senha são obrigatórios.";
                    return View();
                }

                _logger.LogInformation("Tentativa de login administrativo: {Email}", Email);

                // Buscar usuário por email
                var user = await _userManager.FindByEmailAsync(Email);
                if (user == null)
                {
                    _logger.LogWarning("Usuário não encontrado: {Email}", Email);
                    TempData["ErrorMessage"] = "E-mail ou senha incorretos.";
                    return View();
                }

                // Verificar se o usuário tem role Admin
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Admin"))
                {
                    _logger.LogWarning("Usuário {Email} não tem role Admin. Roles: {Roles}", Email, string.Join(", ", roles));
                    TempData["ErrorMessage"] = "Acesso restrito ao administrador.";
                    return View();
                }

                // Tentar fazer login
                var result = await _signInManager.PasswordSignInAsync(user, Password, false, lockoutOnFailure: false);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Falha na autenticação para {Email}: {Result}", Email, result);
                    TempData["ErrorMessage"] = "E-mail ou senha incorretos.";
                    return View();
                }

                // Armazenar informações na sessão para validação JWT
                HttpContext.Session.SetString("JwtToken", "local-admin-token"); // Token local para validação
                HttpContext.Session.SetString("AdminEmail", Email);
                HttpContext.Session.SetString("Role", "Admin");

                _logger.LogInformation("Login administrativo bem-sucedido para: {Email}", Email);
                return RedirectToAction("Index", "Admin", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no login administrativo para {Email}", Email);
                TempData["ErrorMessage"] = "Erro interno. Tente novamente.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            
            // Limpar sessão
            HttpContext.Session.Clear();
            
            return RedirectToAction("Login", "Account");
        }
    }
}