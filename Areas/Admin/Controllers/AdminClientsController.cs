using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sistema.Services.Api;
using SistemaAPI.DTOs;
using Microsoft.Extensions.Logging;
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
    public class AdminClientsController : Controller
    {
        private readonly IApiClientsService _apiService;
        private readonly ILogger<AdminClientsController> _logger;

        public AdminClientsController(IApiClientsService apiService, ILogger<AdminClientsController> logger)
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

        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            try
            {
                var response = await _apiService.GetAllAsync();
                var clients = response.Success ? response.Data?.ToList() ?? new List<ClientDto>() : new List<ClientDto>();

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Lista de Clientes");
                
                // Cabeçalho
                worksheet.Cell(1, 1).Value = "Lista de Clientes - Sistema EwellinBeauty";
                worksheet.Cell(2, 1).Value = $"Data de Geração: {DateTime.Now:dd/MM/yyyy HH:mm}";
                worksheet.Cell(3, 1).Value = $"Total de Clientes: {clients.Count}";
                
                // Cabeçalhos da tabela
                worksheet.Cell(5, 1).Value = "ID";
                worksheet.Cell(5, 2).Value = "Nome";
                worksheet.Cell(5, 3).Value = "Email";
                worksheet.Cell(5, 4).Value = "Telefone";
                worksheet.Cell(5, 5).Value = "Data de Cadastro";
                worksheet.Cell(5, 6).Value = "Status";
                
                // Dados dos clientes
                int row = 6;
                foreach (var client in clients)
                {
                    worksheet.Cell(row, 1).Value = client.ClientId;
                    worksheet.Cell(row, 2).Value = client.FirstName + " " + client.LastName;
                    worksheet.Cell(row, 3).Value = client.Email;
                    worksheet.Cell(row, 4).Value = client.Phone ?? "N/A";
                    worksheet.Cell(row, 5).Value = client.CreatedAt.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 6).Value = client.IsActive ? "Ativo" : "Inativo";
                    row++;
                }
                
                // Formatação
                worksheet.Range(1, 1, 1, 6).Merge().Style.Font.Bold = true;
                worksheet.Range(1, 1, 1, 6).Style.Font.FontSize = 16;
                worksheet.Range(5, 1, 5, 6).Style.Font.Bold = true;

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Seek(0, SeekOrigin.Begin);

                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Lista_Clientes.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao exportar lista de clientes para Excel");
                TempData["ErrorMessage"] = "Erro ao exportar lista de clientes para Excel.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportToPdf()
        {
            try
            {
                var response = await _apiService.GetAllAsync();
                var clients = response.Success ? response.Data?.ToList() ?? new List<ClientDto>() : new List<ClientDto>();

                var caminho = Path.Combine(Directory.GetCurrentDirectory(), "Lista_Clientes.pdf");
                using var writer = new PdfWriter(caminho);
                using var pdf = new PdfDocument(writer);
                var doc = new Document(pdf);
                
                // Cabeçalho
                doc.Add(new Paragraph("Lista de Clientes - Sistema EwellinBeauty")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(16));
                
                doc.Add(new Paragraph($"Data de Geração: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12));
                
                doc.Add(new Paragraph($"Total de Clientes: {clients.Count}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12));
                
                doc.Add(new Paragraph(" ")); // Espaço
                
                // Tabela de clientes
                var clientsTable = new Table(6);
                clientsTable.AddHeaderCell("ID");
                clientsTable.AddHeaderCell("Nome");
                clientsTable.AddHeaderCell("Email");
                clientsTable.AddHeaderCell("Telefone");
                clientsTable.AddHeaderCell("Data de Cadastro");
                clientsTable.AddHeaderCell("Status");
                
                foreach (var client in clients)
                {
                    clientsTable.AddCell(client.ClientId.ToString());
                    clientsTable.AddCell(client.FirstName + " " + client.LastName);
                    clientsTable.AddCell(client.Email ?? "");
                    clientsTable.AddCell(client.Phone ?? "N/A");
                    clientsTable.AddCell(client.CreatedAt.ToString("dd/MM/yyyy"));
                    clientsTable.AddCell(client.IsActive ? "Ativo" : "Inativo");
                }
                
                doc.Add(clientsTable);
                doc.Close();

                var bytes = System.IO.File.ReadAllBytes(caminho);
                return File(bytes, "application/pdf", "Lista_Clientes.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao exportar lista de clientes para PDF");
                TempData["ErrorMessage"] = "Erro ao exportar lista de clientes para PDF.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

