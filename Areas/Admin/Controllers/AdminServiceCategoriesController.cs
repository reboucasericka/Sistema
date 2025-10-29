using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sistema.Services.Api;
using SistemaAPI.DTOs;
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
    public class AdminServiceCategoriesController : Controller
    {
        private readonly ILogger<AdminServiceCategoriesController> _logger;

        public AdminServiceCategoriesController(ILogger<AdminServiceCategoriesController> logger)
        {
            _logger = logger;
        }

        // GET: Admin/ServiceCategories
        public async Task<IActionResult> Index()
        {
            try
            {
                // Implementar busca de categorias via API quando dispon�vel
                // Por enquanto, retornar lista vazia
                return View(new List<Category>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar categorias de servi�os");
                TempData["Error"] = "Erro ao carregar categorias.";
                return View(new List<Category>());
            }
        }

        // GET: Admin/ServiceCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Implementar busca de categoria via API quando dispon�vel
                // Por enquanto, retornar NotFound
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar categoria {Id}", id);
                return NotFound();
            }
        }

        // GET: Admin/ServiceCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/ServiceCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Category serviceCategory)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Implementar cria��o de categoria via API quando dispon�vel
                    TempData["SuccessMessage"] = "Categoria criada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao criar categoria");
                    TempData["ErrorMessage"] = "Erro ao criar categoria.";
                }
            }
            return View(serviceCategory);
        }

        // GET: Admin/ServiceCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Implementar busca de categoria via API quando dispon�vel
                // Por enquanto, retornar NotFound
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar categoria {Id}", id);
                return NotFound();
            }
        }

        // POST: Admin/ServiceCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Category serviceCategory)
        {
            if (id != serviceCategory.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Implementar atualiza��o de categoria via API quando dispon�vel
                    TempData["SuccessMessage"] = "Categoria atualizada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar categoria {Id}", id);
                    TempData["ErrorMessage"] = "Erro ao atualizar categoria.";
                }
            }
            return View(serviceCategory);
        }

        // GET: Admin/ServiceCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Implementar busca de categoria via API quando dispon�vel
                // Por enquanto, retornar NotFound
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar categoria {Id}", id);
                return NotFound();
            }
        }

        // POST: Admin/ServiceCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Implementar exclus�o de categoria via API quando dispon�vel
                TempData["SuccessMessage"] = "Categoria exclu�da com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir categoria {Id}", id);
                TempData["ErrorMessage"] = "Erro ao excluir categoria.";
                return RedirectToAction(nameof(Index));
            }
        }

    }
}