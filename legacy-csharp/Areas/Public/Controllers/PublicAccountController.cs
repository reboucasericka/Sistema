using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.Helpers;
using Sistema.Models.Account;
using Sistema.Services;
using Microsoft.Extensions.Logging;
using Sistema.Data.Entities;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class PublicAccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IEmailService _emailService;
        private readonly ILogger<PublicAccountController> _logger;

        public PublicAccountController(
            IUserHelper userHelper, 
            IEmailService emailService,
            ILogger<PublicAccountController> logger)
        {
            _userHelper = userHelper;
            _emailService = emailService;
            _logger = logger;
        }

        // =======================
        // REGISTRO DE CLIENTE
        // =======================
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "PublicAppointment", new { area = "Public" });

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterNewUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Verificar se o e-mail já existe
                var existingUser = await _userHelper.GetUserByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Este e-mail já está registrado.");
                    return View(model);
                }

                // Verificar se o username já existe
                var existingUsername = await _userHelper.GetUserByUsernameAsync(model.Username);
                if (existingUsername != null)
                {
                    ModelState.AddModelError("Username", "Este username já está em uso.");
                    return View(model);
                }

                // Log do novo registro
                _logger.LogInformation("Novo registro de cliente: {Email}", model.Email);

                // Criar novo usuário
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.Phone,
                    UserName = model.Username,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.Now
                };

                // Adicionar usuário
                var result = await _userHelper.AddUserAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

                // Adicionar role de Customer
                await _userHelper.AddUserToRoleAsync(user, "Customer");

                // Gerar token de ativação
                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);

                // Montar link de ativação
                var activationLink = Url.Action("ActivateAccount", "PublicAccount", 
                    new { area = "Public", userId = user.Id, token }, Request.Scheme);

                // Enviar e-mail de ativação
                await _emailService.SendActivationEmailAsync(user.Email, user.FirstName, activationLink);

                // Log de registro
                _logger.LogInformation($"Novo cliente registrado: {user.Email}");

                // Armazenar e-mail no TempData para exibir na tela de pendência
                TempData["UserEmail"] = user.Email;

                return RedirectToAction("ActivationPending");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar cliente");
                ModelState.AddModelError("", "Ocorreu um erro ao criar sua conta. Tente novamente.");
                return View(model);
            }
        }

        // =======================
        // ATIVAÇÃO DE CONTA
        // =======================
        [HttpGet]
        public IActionResult ActivationPending()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ActivateAccount(string userId, string token)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Link de ativação inválido.";
                    return RedirectToAction("ActivationPending");
                }

                var user = await _userHelper.GetUserByIdAsync(userId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Usuário não encontrado.";
                    return RedirectToAction("ActivationPending");
                }

                if (user.EmailConfirmed)
                {
                    TempData["InfoMessage"] = "Sua conta já está ativada.";
                    return RedirectToAction("ActivateSuccess");
                }

                var result = await _userHelper.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Conta ativada com sucesso: {user.Email}");
                    return RedirectToAction("ActivateSuccess");
                }
                else
                {
                    _logger.LogWarning($"Falha na ativação da conta: {user.Email}");
                    TempData["ErrorMessage"] = "Falha ao ativar a conta. O link pode ter expirado.";
                    return RedirectToAction("ActivationPending");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao ativar conta");
                TempData["ErrorMessage"] = "Ocorreu um erro ao ativar sua conta.";
                return RedirectToAction("ActivationPending");
            }
        }

        [HttpGet]
        public IActionResult ActivateSuccess()
        {
            return View();
        }
    }
}
