using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sistema.Services.Api;
using SistemaAPI.DTOs;
using Microsoft.Extensions.Logging;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminClientsController : Controller
    {
        private readonly ApiClientsService _apiService;
        private readonly ILogger<AdminClientsController> _logger;

        public AdminClientsController(ApiClientsService apiService, ILogger<AdminClientsController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Admin/Customers
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _apiService.GetAllAsync();
                
                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    _logger.LogError("Failed to fetch clients: {Message}", response.Message);
                    TempData["ErrorMessage"] = "Failed to load clients. Please try again.";
                    return View(new List<ClientDto>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching clients");
                TempData["ErrorMessage"] = "An error occurred while loading clients.";
                return View(new List<ClientDto>());
            }
        }

        // GET: Admin/Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _apiService.GetByIdAsync(id.Value);
                
                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    _logger.LogError("Failed to fetch client {Id}: {Message}", id, response.Message);
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching client {Id}", id);
                return NotFound();
            }
        }

        // GET: Admin/Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,Name,Email,Phone,CreatedAt")] ClientDto client)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _apiService.CreateAsync(client);
                    
                    if (response.Success)
                    {
                        TempData["SuccessMessage"] = "Customer created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError("Failed to create client: {Message}", response.Message);
                        TempData["ErrorMessage"] = $"Failed to create client: {response.Message}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating client");
                    TempData["ErrorMessage"] = "An error occurred while creating the client.";
                }
            }
            return View(client);
        }

        // GET: Admin/Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _apiService.GetByIdAsync(id.Value);
                
                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    _logger.LogError("Failed to fetch client for edit {Id}: {Message}", id, response.Message);
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching client for edit {Id}", id);
                return NotFound();
            }
        }

        // POST: Admin/Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClientId,Name,Email,Phone,CreatedAt")] ClientDto client)
        {
            if (id != client.ClientId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _apiService.UpdateAsync(id, client);
                    
                    if (response.Success)
                    {
                        TempData["SuccessMessage"] = "Customer updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError("Failed to update client: {Message}", response.Message);
                        TempData["ErrorMessage"] = $"Failed to update client: {response.Message}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating client");
                    TempData["ErrorMessage"] = "An error occurred while updating the client.";
                }
            }
            return View(client);
        }

        // GET: Admin/Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _apiService.GetByIdAsync(id.Value);
                
                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    _logger.LogError("Failed to fetch client for delete {Id}: {Message}", id, response.Message);
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching client for delete {Id}", id);
                return NotFound();
            }
        }

        // POST: Admin/Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var response = await _apiService.DeleteAsync(id);
                
                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Customer deleted successfully!";
                }
                else
                {
                    _logger.LogError("Failed to delete client: {Message}", response.Message);
                    TempData["ErrorMessage"] = $"Failed to delete client: {response.Message}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client");
                TempData["ErrorMessage"] = "An error occurred while deleting the client.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

