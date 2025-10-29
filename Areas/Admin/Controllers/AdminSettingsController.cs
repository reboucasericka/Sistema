using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.Services.Api;
using SistemaAPI.DTOs;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminSettingsController : Controller
    {
        private readonly IApiSettingsService _settingsService;
        private readonly ILogger<AdminSettingsController> _logger;

        public AdminSettingsController(IApiSettingsService settingsService, ILogger<AdminSettingsController> logger)
        {
            _settingsService = settingsService;
            _logger = logger;
        }

        // GET: Settings
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Configurações";
            
            try
            {
                var response = await _settingsService.GetAllAsync();
                if (response.IsSuccess)
                {
                    return View(response.Data);
                }
                else
                {
                    _logger.LogError("Erro ao buscar configurações: {Error}", response.Message);
                    TempData["Error"] = "Erro ao carregar configurações.";
                    return View(new List<SettingDto>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar configurações");
                TempData["Error"] = "Erro interno do servidor.";
                return View(new List<SettingDto>());
            }
        }

        // GET: Settings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _settingsService.GetByIdAsync(id.Value);
                if (response.IsSuccess && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar configuração {Id}", id);
                return NotFound();
            }
        }

        // GET: Settings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Settings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SettingId,ClinicName,Email,LandlinePhone,WhatsAppPhone,Address,Logo,Icon,ReportLogo,ReportType,Instagram,CommissionType,DefaultExtensionCommission,DefaultDesignCommission,ImagesFolder,BusinessHours,DefaultServiceDuration")] SettingDto setting)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _settingsService.CreateAsync(setting);
                    if (response.IsSuccess)
                    {
                        TempData["SuccessMessage"] = "Configuração criada com sucesso!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Erro ao criar configuração.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao criar configuração");
                    TempData["ErrorMessage"] = "Erro interno do servidor.";
                }
            }
            return View(setting);
        }

        // GET: Settings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _settingsService.GetByIdAsync(id.Value);
                if (response.IsSuccess && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar configuração para edição {Id}", id);
                return NotFound();
            }
        }

        // POST: Settings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SettingId,ClinicName,Email,LandlinePhone,WhatsAppPhone,Address,Logo,Icon,ReportLogo,ReportType,Instagram,CommissionType,DefaultExtensionCommission,DefaultDesignCommission,ImagesFolder,BusinessHours,DefaultServiceDuration")] SettingDto setting)
        {
            if (id != setting.SettingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _settingsService.UpdateAsync(id, setting);
                    if (response.IsSuccess)
                    {
                        TempData["SuccessMessage"] = "Configuração atualizada com sucesso!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Erro ao atualizar configuração.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar configuração {Id}", id);
                    TempData["ErrorMessage"] = "Erro interno do servidor.";
                }
            }
            return View(setting);
        }

        // GET: Settings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _settingsService.GetByIdAsync(id.Value);
                if (response.IsSuccess && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar configuração para exclusão {Id}", id);
                return NotFound();
            }
        }

        // POST: Settings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var response = await _settingsService.DeleteAsync(id);
                if (response.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Configuração excluída com sucesso!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erro ao excluir configuração.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir configuração {Id}", id);
                TempData["ErrorMessage"] = "Erro interno do servidor.";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
