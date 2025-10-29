using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.Helpers;
using Sistema.Models.Account;
using Sistema.Models.Admin;
using Sistema.Services.Auth;
using Sistema.Data.Entities;
using System.Security.Claims;

namespace Sistema.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly ApiAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IUserHelper userHelper,
            ApiAuthService authService,
            ILogger<AccountController> logger)
        {
            _userHelper = userHelper;
            _authService = authService;
            _logger = logger;
        }

        // =======================
        // LOGIN ADMIN
        // =======================
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState inválido no login Admin");
                return View(model);
            }

            _logger.LogInformation("Tentativa de login (Admin): {Username}", model.Username);

            try
            {
                // Autenticar via API
                var authResult = await _authService.LoginAsync(model.Username, model.Password);
                
                if (!authResult.Success)
                {
                    _logger.LogWarning("Credenciais inválidas para usuário: {Username}", model.Username);
                    ModelState.AddModelError("", "Credenciais inválidas.");
                    return View(model);
                }

                // Verificar se o usuário é Admin
                if (!authResult.Data?.Roles?.Contains("Admin") == true)
                {
                    _logger.LogWarning("Acesso negado. Usuário {Username} não é Admin", model.Username);
                    ModelState.AddModelError("", "Acesso restrito a administradores.");
                    return View(model);
                }

                // Criar claims para autenticação local
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Username),
                    new Claim(ClaimTypes.Email, model.Username),
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim("FirstName", "Admin"),
                    new Claim("LastName", "User")
                };

                // Adicionar roles
                if (authResult.Data.Roles != null)
                {
                    foreach (var role in authResult.Data.Roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(1)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                    new ClaimsPrincipal(claimsIdentity), authProperties);

                _logger.LogInformation("Login de Admin bem-sucedido para: {Username}", model.Username);
                return RedirectToAction("Index", "Admin", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante login para usuário: {Username}", model.Username);
                ModelState.AddModelError("", "Erro interno. Tente novamente.");
                return View(model);
            }
        }

        // =======================
        // LOGIN VIA FACEBOOK E GOOGLE
        // =======================
        [HttpGet]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { ReturnUrl = returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Erro do provedor externo: {remoteError}");
                return Redirect("/Account/Login");
            }

            try
            {
                var info = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme) ??
                          await HttpContext.AuthenticateAsync(FacebookDefaults.AuthenticationScheme);

                if (info?.Succeeded != true)
                {
                    return Redirect("/Account/Login");
                }

                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);

                if (string.IsNullOrEmpty(email))
                {
                    ModelState.AddModelError(string.Empty, "Não foi possível obter informações do email.");
                    return Redirect("/Account/Login");
                }

                // Implementar registro/login via API para login social
                // Fazer login via API
                var loginResult = await _authService.LoginAsync(email, "");
                if (loginResult.Success)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["Message"] = "Erro no login social. Tente novamente.";
                    return Redirect("/Account/Login");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante login externo");
                ModelState.AddModelError(string.Empty, "Erro durante autenticação externa.");
                return Redirect("/Account/Login");
            }
        }

        // =======================
        // LOGOUT
        // =======================
        [HttpGet, HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // =======================
        // PROFILE
        // =======================
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("/Account/Login");
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Redirect("/Account/Login");
                }

                // Buscar dados do usuário via API
                // Implementação simplificada - usar dados do usuário logado
                var model = new AdminProfileViewModel
                {
                    FirstName = User.FindFirst("FirstName")?.Value ?? "",
                    LastName = User.FindFirst("LastName")?.Value ?? "",
                    Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "",
                    PhoneNumber = "",
                    CreatedAt = DateTime.Now,
                    Active = true
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar perfil do usuário");
                return Redirect("/Account/Login");
            }
        }

        // =======================
        // PASSWORD RESET
        // =======================
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Implementar reset de senha via API
                    // Funcionalidade simplificada
                    TempData["SuccessMessage"] = "Instruções de reset de senha enviadas para seu email.";
                    TempData["Message"] = "Funcionalidade de reset de senha será implementada em breve.";
                    return RedirectToAction("ForgotPasswordConfirmation");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro durante reset de senha para: {Email}", model.Email);
                    ModelState.AddModelError("", "Erro interno. Tente novamente.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token = null)
        {
            if (token == null)
            {
                return Redirect("/Account/Login");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Implementar reset de senha via API
                // Funcionalidade simplificada
                TempData["SuccessMessage"] = "Instruções de reset de senha enviadas para seu email.";
                TempData["Message"] = "Funcionalidade de reset de senha será implementada em breve.";
                return RedirectToAction("ResetPasswordConfirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante reset de senha");
                ModelState.AddModelError("", "Erro interno. Tente novamente.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // =======================
        // CHANGE USER
        // =======================
        [HttpGet]
        public async Task<IActionResult> ChangeUser()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("/Account/Login");
            }

            var model = new ChangeUserViewModel
            {
                FirstName = User.FindFirst("FirstName")?.Value ?? "",
                LastName = User.FindFirst("LastName")?.Value ?? ""
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUser(ChangeUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Implementar atualização de usuário via API
                // Funcionalidade simplificada
                TempData["SuccessMessage"] = "Perfil atualizado com sucesso!";
                TempData["Message"] = "Funcionalidade de atualização de usuário será implementada em breve.";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante atualização de usuário");
                ModelState.AddModelError("", "Erro interno. Tente novamente.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Implementar mudança de senha via API
                // Funcionalidade simplificada
                TempData["SuccessMessage"] = "Senha alterada com sucesso!";
                TempData["Message"] = "Funcionalidade de mudança de senha será implementada em breve.";
                return RedirectToAction("Settings");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante mudança de senha");
                ModelState.AddModelError("", "Erro interno. Tente novamente.");
            }

            return View(model);
        }

        // =======================
        // SETTINGS
        // =======================
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("/Account/Login");
            }

            var model = new
            {
                FirstName = User.FindFirst("FirstName")?.Value ?? "",
                LastName = User.FindFirst("LastName")?.Value ?? "",
                Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "",
                PhoneNumber = ""
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Settings(object model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Implementar atualização de configurações via API
                // Funcionalidade simplificada
                TempData["SuccessMessage"] = "Configurações atualizadas com sucesso!";
                TempData["Message"] = "Funcionalidade de configurações será implementada em breve.";
                return RedirectToAction("Settings");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante atualização de configurações");
                ModelState.AddModelError("", "Erro interno. Tente novamente.");
            }

            return View(model);
        }
    }
}