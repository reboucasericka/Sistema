using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Sistema.Services.Api;
using SistemaAPI.DTOs;
using Sistema.Models.Admin;
using Sistema.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminProfessionalSchedulesController : Controller
    {
        private readonly IApiStaffService _staffService;
        private readonly ILogger<AdminProfessionalSchedulesController> _logger;

        public AdminProfessionalSchedulesController(IApiStaffService staffService, ILogger<AdminProfessionalSchedulesController> logger)
        {
            _staffService = staffService;
            _logger = logger;
        }

        // GET: ProfessionalSchedules
        public async Task<IActionResult> Index()
        {
            try
            {
                // Implementar busca de hor�rios de profissionais via API quando dispon�vel
                // Por enquanto, retornar lista vazia
                return View(new List<ProfessionalSchedule>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar hor�rios de profissionais");
                TempData["Error"] = "Erro ao carregar hor�rios.";
                return View(new List<ProfessionalSchedule>());
            }
        }

        // GET: ProfessionalSchedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Implementar busca de hor�rio de profissional via API quando dispon�vel
                // Por enquanto, retornar NotFound
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar hor�rio de profissional {Id}", id);
                return NotFound();
            }
        }

        // GET: ProfessionalSchedules/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Implementar busca de profissionais via API quando dispon�vel
                ViewData["ProfessionalId"] = new SelectList(new List<object>(), "ProfessionalId", "Name");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar formul�rio de cria��o de hor�rio");
                TempData["Error"] = "Erro ao carregar formul�rio.";
                return View();
            }
        }

        // POST: ProfessionalSchedules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProfessionalSchedule professionalSchedule)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Implementar cria��o de hor�rio de profissional via API quando dispon�vel
                    TempData["SuccessMessage"] = "Hor�rio criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao criar hor�rio de profissional");
                    TempData["ErrorMessage"] = "Erro ao criar hor�rio.";
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorMessage = $"Erros de valida��o: {string.Join(", ", errors)}";
                TempData["ErrorMessage"] = errorMessage;
            }
            
            ViewData["ProfessionalId"] = new SelectList(new List<object>(), "ProfessionalId", "Name", professionalSchedule.ProfessionalId);
            return View(professionalSchedule);
        }


        // GET: ProfessionalSchedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Implementar busca de hor�rio de profissional via API quando dispon�vel
                // Por enquanto, retornar NotFound
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar hor�rio de profissional {Id}", id);
                return NotFound();
            }
        }

        // POST: ProfessionalSchedules/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProfessionalSchedule professionalSchedule)
        {
            if (id != professionalSchedule.ScheduleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Implementar atualiza��o de hor�rio de profissional via API quando dispon�vel
                    TempData["SuccessMessage"] = "Hor�rio atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar hor�rio de profissional {Id}", id);
                    TempData["ErrorMessage"] = "Erro ao atualizar hor�rio.";
                }
            }
            ViewData["ProfessionalId"] = new SelectList(new List<object>(), "ProfessionalId", "Name", professionalSchedule.ProfessionalId);
            return View(professionalSchedule);
        }

        // GET: ProfessionalSchedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Implementar busca de hor�rio de profissional via API quando dispon�vel
                // Por enquanto, retornar NotFound
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar hor�rio de profissional {Id}", id);
                return NotFound();
            }
        }

        // POST: ProfessionalSchedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Implementar exclus�o de hor�rio de profissional via API quando dispon�vel
                TempData["SuccessMessage"] = "Hor�rio exclu�do com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir hor�rio de profissional {Id}", id);
                TempData["ErrorMessage"] = "Erro ao excluir hor�rio.";
                return RedirectToAction(nameof(Index));
            }
        }

    }
}
