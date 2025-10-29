using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sistema.Services.Api;
using SistemaAPI.DTOs;
using Sistema.Helpers;
using Sistema.Models.Admin;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using ClosedXML.Excel;
using System.IO;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminServicesController : Controller
    {
        private readonly ApiServicesService _servicesService;
        private readonly IStorageHelper _storageHelper;
        private readonly ILogger<AdminServicesController> _logger;

        public AdminServicesController(ApiServicesService servicesService, IStorageHelper storageHelper, ILogger<AdminServicesController> logger)
        {
            _servicesService = servicesService;
            _storageHelper = storageHelper;
            _logger = logger;
        }

        // GET: Admin/Services
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _servicesService.GetAllAsync();
                
                if (response.Success && response.Data != null)
                {
                    var services = response.Data.OrderBy(s => s.Name).ToList();
                    return View(services);
                }
                else
                {
                    _logger.LogError("Failed to fetch services: {Message}", response.Message);
                    TempData["ErrorMessage"] = "Failed to load services. Please try again.";
                    return View(new List<ServiceDto>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading services");
                TempData["ErrorMessage"] = "An error occurred while loading services.";
                return View(new List<ServiceDto>());
            }
        }

        // GET: Admin/Services/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _servicesService.GetByIdAsync(id.Value);

                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    _logger.LogError("Failed to fetch service {Id}: {Message}", id, response.Message);
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching service {Id}", id);
                return NotFound();
            }
        }

        // GET: Admin/Services/Create
        [HttpGet]
        public IActionResult Create()
        {
            // Buscar categorias via API quando o endpoint estiver dispon�vel
            ViewData["ServiceCategories"] = new SelectList(new List<object>(), "CategoryId", "Name");
            return View();
        }

        // POST: Admin/Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminServiceCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string photoPath = string.Empty;
                    if (model.file != null)
                    {
                        photoPath = await _storageHelper.UploadAsync(model.file, "services");
                    }

                    var serviceDto = new ServiceDto
                    {
                        Name = model.Name,
                        Description = model.Description,
                        Price = model.Price,
                        DurationMinutes = int.TryParse(model.Duration, out int duration) ? duration : 0,
                        Category = model.ServiceCategoryId.ToString(),
                        IsActive = model.IsActive
                    };

                    var response = await _servicesService.CreateAsync(serviceDto);

                    if (response.Success)
                    {
                        // Log access
                        await LogAccess("CREATE", $"Service created: {model.Name}");

                        TempData["SuccessMessage"] = "Service created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError("Failed to create service: {Message}", response.Message);
                        TempData["ErrorMessage"] = $"Failed to create service: {response.Message}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating service");
                    TempData["ErrorMessage"] = "An error occurred while creating the service.";
                }
            }

            // Buscar categorias via API quando o endpoint estiver dispon�vel
            ViewData["ServiceCategories"] = new SelectList(new List<object>(), "CategoryId", "Name", model.ServiceCategoryId);
            return View(model);
        }

        // GET: Admin/Services/Edit/5
        public async Task<IActionResult> Edit(int? serviceId)
        {
            if (serviceId == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _servicesService.GetByIdAsync(serviceId.Value);

                if (response.Success && response.Data != null)
                {
                    var model = new AdminServiceEditViewModel
                    {
                        ServiceId = response.Data.ServiceId,
                        Name = response.Data.Name,
                        Description = response.Data.Description,
                        Price = response.Data.Price,
                        Duration = response.Data.DurationMinutes.ToString(),
                        ServiceCategoryId = int.TryParse(response.Data.Category, out int catId) ? catId : 0,
                        IsActive = response.Data.IsActive
                    };

                    // Buscar categorias via API quando o endpoint estiver dispon�vel
                    ViewData["ServiceCategories"] = new SelectList(new List<object>(), "CategoryId", "Name", model.ServiceCategoryId);
                    return View(model);
                }
                else
                {
                    _logger.LogError("Failed to fetch service for edit {Id}: {Message}", serviceId, response.Message);
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching service for edit {Id}", serviceId);
                return NotFound();
            }
        }

        // POST: Admin/Services/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int serviceId, AdminServiceEditViewModel model)
        {
            if (serviceId != model.ServiceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Se uma nova foto foi enviada, fazer upload
                    int? imageId = model.ImageId != null ? int.Parse(model.ImageId) : null;
                    if (model.file != null)
                    {
                        if (imageId != null)
                        {
                            await _storageHelper.DeleteAsync(imageId.ToString(), "services");
                        }
                        string photoPath = await _storageHelper.UploadAsync(model.file, "services");
                        imageId = string.IsNullOrEmpty(photoPath) ? null : int.Parse(photoPath);
                    }

                    var serviceDto = new ServiceDto
                    {
                        ServiceId = model.ServiceId,
                        Name = model.Name,
                        Description = model.Description,
                        Price = model.Price,
                        DurationMinutes = int.TryParse(model.Duration, out int duration) ? duration : 0,
                        Category = model.ServiceCategoryId.ToString(),
                        IsActive = model.IsActive
                    };

                    var response = await _servicesService.UpdateAsync(serviceId, serviceDto);

                    if (response.Success)
                    {
                        // Log access
                        await LogAccess("UPDATE", $"Service updated: {model.Name}");

                        TempData["SuccessMessage"] = "Service updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError("Failed to update service: {Message}", response.Message);
                        TempData["ErrorMessage"] = $"Failed to update service: {response.Message}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating service");
                    TempData["ErrorMessage"] = "An error occurred while updating the service.";
                }
            }

            // Buscar categorias via API quando o endpoint estiver dispon�vel
            ViewData["ServiceCategories"] = new SelectList(new List<object>(), "CategoryId", "Name", model.ServiceCategoryId);
            return View(model);
        }

        // GET: Admin/Services/Delete/5
        public async Task<IActionResult> Delete(int? serviceId)
        {
            if (serviceId == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _servicesService.GetByIdAsync(serviceId.Value);

                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    _logger.LogError("Failed to fetch service for delete {Id}: {Message}", serviceId, response.Message);
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching service for delete {Id}", serviceId);
                return NotFound();
            }
        }

        // POST: Admin/Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int serviceId)
        {
            try
            {
                // Buscar o servi�o primeiro para deletar a imagem
                // var service = await _servicesService.GetByIdAsync(serviceId);
                // if (service.Success && service.Data?.ImageId != Guid.Empty)
                // {
                //     await _storageHelper.DeleteAsync(service.Data.ImageId.ToString(), "services");
                // }

                var response = await _servicesService.DeleteAsync(serviceId);

                if (response.Success)
                {
                    // Log access
                    await LogAccess("DELETE", $"Service deleted: {serviceId}");

                    TempData["SuccessMessage"] = "Service deleted successfully!";
                }
                else
                {
                    _logger.LogError("Failed to delete service: {Message}", response.Message);
                    TempData["ErrorMessage"] = $"Failed to delete service: {response.Message}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service");
                TempData["ErrorMessage"] = "An error occurred while deleting the service.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LogAccess(string action, string details)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    // Implementar log via API quando o endpoint estiver dispon�vel
                    _logger.LogInformation("Access Log: {Action} - {Details} by User {UserId}", action, details, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging access");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            try
            {
                var response = await _servicesService.GetAllAsync();
                var services = response.Success ? response.Data?.ToList() ?? new List<ServiceDto>() : new List<ServiceDto>();

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Lista de Serviços");
                
                // Cabeçalho
                worksheet.Cell(1, 1).Value = "Lista de Serviços - Sistema EwellinBeauty";
                worksheet.Cell(2, 1).Value = $"Data de Geração: {DateTime.Now:dd/MM/yyyy HH:mm}";
                worksheet.Cell(3, 1).Value = $"Total de Serviços: {services.Count}";
                
                // Cabeçalhos da tabela
                worksheet.Cell(5, 1).Value = "ID";
                worksheet.Cell(5, 2).Value = "Nome";
                worksheet.Cell(5, 3).Value = "Descrição";
                worksheet.Cell(5, 4).Value = "Preço";
                worksheet.Cell(5, 5).Value = "Duração (min)";
                worksheet.Cell(5, 6).Value = "Categoria";
                worksheet.Cell(5, 7).Value = "Status";
                
                // Dados dos serviços
                int row = 6;
                foreach (var service in services)
                {
                    worksheet.Cell(row, 1).Value = service.ServiceId;
                    worksheet.Cell(row, 2).Value = service.Name;
                    worksheet.Cell(row, 3).Value = service.Description;
                    worksheet.Cell(row, 4).Value = service.Price.ToString("C");
                    worksheet.Cell(row, 5).Value = service.DurationMinutes;
                    worksheet.Cell(row, 6).Value = service.CategoryName;
                    worksheet.Cell(row, 7).Value = service.IsActive ? "Ativo" : "Inativo";
                    row++;
                }
                
                // Formatação
                worksheet.Range(1, 1, 1, 7).Merge().Style.Font.Bold = true;
                worksheet.Range(1, 1, 1, 7).Style.Font.FontSize = 16;
                worksheet.Range(5, 1, 5, 7).Style.Font.Bold = true;

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Seek(0, SeekOrigin.Begin);

                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Lista_Servicos.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao exportar lista de serviços para Excel");
                TempData["ErrorMessage"] = "Erro ao exportar lista de serviços para Excel.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportToPdf()
        {
            try
            {
                var response = await _servicesService.GetAllAsync();
                var services = response.Success ? response.Data?.ToList() ?? new List<ServiceDto>() : new List<ServiceDto>();

                var caminho = Path.Combine(Directory.GetCurrentDirectory(), "Lista_Servicos.pdf");
                using var writer = new PdfWriter(caminho);
                using var pdf = new PdfDocument(writer);
                var doc = new Document(pdf);
                
                // Cabeçalho
                doc.Add(new Paragraph("Lista de Serviços - Sistema EwellinBeauty")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(16));
                
                doc.Add(new Paragraph($"Data de Geração: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12));
                
                doc.Add(new Paragraph($"Total de Serviços: {services.Count}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12));
                
                doc.Add(new Paragraph(" ")); // Espaço
                
                // Tabela de serviços
                var servicesTable = new Table(7);
                servicesTable.AddHeaderCell("ID");
                servicesTable.AddHeaderCell("Nome");
                servicesTable.AddHeaderCell("Descrição");
                servicesTable.AddHeaderCell("Preço");
                servicesTable.AddHeaderCell("Duração (min)");
                servicesTable.AddHeaderCell("Categoria");
                servicesTable.AddHeaderCell("Status");
                
                foreach (var service in services)
                {
                    servicesTable.AddCell(service.ServiceId.ToString());
                    servicesTable.AddCell(service.Name ?? "");
                    servicesTable.AddCell(service.Description ?? "");
                    servicesTable.AddCell(service.Price.ToString("C"));
                    servicesTable.AddCell(service.DurationMinutes.ToString());
                    servicesTable.AddCell(service.CategoryName ?? "");
                    servicesTable.AddCell(service.IsActive ? "Ativo" : "Inativo");
                }
                
                doc.Add(servicesTable);
                doc.Close();

                var bytes = System.IO.File.ReadAllBytes(caminho);
                return File(bytes, "application/pdf", "Lista_Servicos.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao exportar lista de serviços para PDF");
                TempData["ErrorMessage"] = "Erro ao exportar lista de serviços para PDF.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}