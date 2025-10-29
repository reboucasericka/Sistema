using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sistema.Services.Api;
using SistemaAPI.DTOs;
using Sistema.Helpers;
using Sistema.Models.Admin;
using Microsoft.Extensions.Logging;
using Sistema.Data.Entities;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminProfessionalsController : Controller
    {
        private readonly ApiStaffService _staffService;
        private readonly IStorageHelper _storageHelper;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminProfessionalsController> _logger;

        public AdminProfessionalsController(
            ApiStaffService staffService,
            IStorageHelper storageHelper,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminProfessionalsController> logger)
        {
            _staffService = staffService;
            _storageHelper = storageHelper;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: Profissionais
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _staffService.GetAllAsync();
                
                if (response.Success && response.Data != null)
                {
                    var professionals = response.Data.OrderBy(p => p.Name).ToList();
                    return View(professionals);
                }
                else
                {
                    _logger.LogError("Failed to fetch professionals: {Message}", response.Message);
                    TempData["ErrorMessage"] = "Failed to load professionals. Please try again.";
                    return View(new List<ProfessionalDto>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading professionals");
                TempData["ErrorMessage"] = "An error occurred while loading professionals.";
                return View(new List<ProfessionalDto>());
            }
        }

        // GET: Profissionais/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _staffService.GetByIdAsync(id.Value);

                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    _logger.LogError("Failed to fetch professional {Id}: {Message}", id, response.Message);
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching professional {Id}", id);
                return NotFound();
            }
        }

        // GET: Profissionais/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Buscar usuários via API quando o endpoint estiver disponível
                // Por enquanto, usar lista vazia
                ViewData["ExistingUsers"] = new SelectList(new List<object>(), "Id", "Email");
                return View(new AdminProfessionalCreateViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create professional data");
                ViewData["ExistingUsers"] = new SelectList(new List<object>(), "Id", "Email");
                return View(new AdminProfessionalCreateViewModel());
            }
        }

        // POST: Profissionais/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminProfessionalCreateViewModel model)
        {
            try
            {
                // Validação: deve ter usuário existente OU criar novo usuário
                if (string.IsNullOrEmpty(model.ExistingUserId) && 
                    (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password)))
                {
                    ModelState.AddModelError("", "Selecione um usuário existente ou crie um novo.");
                    ViewData["ExistingUsers"] = new SelectList(new List<object>(), "Id", "Email");
                    return View(model);
                }

                string userId = string.Empty;

                // Se usuário existente foi selecionado
                if (!string.IsNullOrEmpty(model.ExistingUserId))
                {
                    userId = model.ExistingUserId;
                }
                // Se email e senha foram fornecidos, criar novo usuário
                else if (!string.IsNullOrEmpty(model.Email) && !string.IsNullOrEmpty(model.Password))
                {
                    var user = new User
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);
                        ViewData["ExistingUsers"] = new SelectList(new List<object>(), "Id", "Email");
                        return View(model);
                    }

                    userId = user.Id;
                    await _userManager.AddToRoleAsync(user, "Professional");
                }

                // Upload da foto se fornecida
                int? imageId = null;
                if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                {
                    try
                    {
                        string photoPath = await _storageHelper.UploadAsync(model.PhotoFile, "professionals");
                        if (!string.IsNullOrEmpty(photoPath))
                        {
                            imageId = int.Parse(photoPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading professional photo");
                        TempData["WarningMessage"] = "Imagem não foi enviada, profissional criado sem foto.";
                    }
                }

                // Criar o profissional via API
                var professionalDto = new ProfessionalDto
                {
                    Name = model.Name,
                    CommissionPercent = model.DefaultCommission,
                    Specialization = model.Specialty,
                    IsActive = model.IsActive
                };

                var response = await _staffService.CreateAsync(professionalDto);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Profissional criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogError("Failed to create professional: {Message}", response.Message);
                    ModelState.AddModelError("", $"Erro ao criar profissional: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating professional");
                ModelState.AddModelError("", "Erro ao salvar profissional: " + ex.Message);
            }

            ViewData["ExistingUsers"] = new SelectList(new List<object>(), "Id", "Email");
            return View(model);
        }


        // GET: Profissionais/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _staffService.GetByIdAsync(id.Value);

                if (response.Success && response.Data != null)
                {
                    // Buscar usuários via API quando o endpoint estiver disponível
                    ViewData["Id"] = new SelectList(new List<object>(), "Id", "Email", response.Data.ProfessionalId);
                    return View(response.Data);
                }
                else
                {
                    _logger.LogError("Failed to fetch professional for edit {Id}: {Message}", id, response.Message);
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching professional for edit {Id}", id);
                return NotFound();
            }
        }

        // POST: Profissionais/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProfessionalDto professional, IFormFile? photoFile)
        {
            if (id != professional.ProfessionalId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Upload da nova foto se fornecida
                    if (photoFile != null && photoFile.Length > 0)
                    {
                        // Implementar upload de imagem quando necessário
                        // Deletar a imagem antiga se existir
                        // if (professional.ImageId != null)
                        // {
                        //     await _storageHelper.DeleteAsync(professional.ImageId.ToString(), "professionals");
                        // }
                        
                        // string photoPath = await _storageHelper.UploadAsync(photoFile, "professionals");
                        // if (!string.IsNullOrEmpty(photoPath))
                        // {
                        //     professional.ImageId = int.Parse(photoPath);
                        // }
                    }

                    var response = await _staffService.UpdateAsync(id, professional);

                    if (response.Success)
                    {
                        TempData["SuccessMessage"] = "Profissional atualizado com sucesso!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError("Failed to update professional: {Message}", response.Message);
                        TempData["ErrorMessage"] = $"Failed to update professional: {response.Message}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating professional");
                    TempData["ErrorMessage"] = "An error occurred while updating the professional.";
                }
            }

            // Buscar usuários via API quando o endpoint estiver disponível
            ViewData["Id"] = new SelectList(new List<object>(), "Id", "Email", professional.ProfessionalId);
            return View(professional);
        }

        // GET: Profissionais/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _staffService.GetByIdAsync(id.Value);

                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    _logger.LogError("Failed to fetch professional for delete {Id}: {Message}", id, response.Message);
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching professional for delete {Id}", id);
                return NotFound();
            }
        }

        // POST: Profissionais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var response = await _staffService.DeleteAsync(id);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Profissional deletado com sucesso!";
                }
                else
                {
                    _logger.LogError("Failed to delete professional: {Message}", response.Message);
                    TempData["ErrorMessage"] = $"Failed to delete professional: {response.Message}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting professional");
                TempData["ErrorMessage"] = "An error occurred while deleting the professional.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Profissionais/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                // Implementar endpoint na API para toggle status
                // Por enquanto, retornar sucesso simulado
                TempData["SuccessMessage"] = "Status toggle feature not yet implemented in API";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling professional status");
                TempData["ErrorMessage"] = "An error occurred while toggling professional status.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

