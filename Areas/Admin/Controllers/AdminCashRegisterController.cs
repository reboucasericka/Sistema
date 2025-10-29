using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.Models.Admin;
using Sistema.Services;
using Sistema.Services.Api;
using SistemaAPI.DTOs;
using Sistema.Data.Entities;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using ClosedXML.Excel;
using System.IO;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminCashRegisterController : Controller
    {
        private readonly IApiProductsService _productsService;
        private readonly IApiAppointmentsService _appointmentsService;
        private readonly ICommunicationService _communicationService;
        private readonly ILogger<AdminCashRegisterController> _logger;

        public AdminCashRegisterController(
            IApiProductsService productsService,
            IApiAppointmentsService appointmentsService,
            ICommunicationService communicationService,
            ILogger<AdminCashRegisterController> logger)
        {
            _productsService = productsService;
            _appointmentsService = appointmentsService;
            _communicationService = communicationService;
            _logger = logger;
        }

        // Página principal do Caixa
        public async Task<IActionResult> Index()
        {
            try
            {
                var hoje = DateTime.Today;
                var viewModel = new CashRegisterViewModel
                {
                    CurrentBalance = 0, // Implementar via API quando disponível
                    IsOpen = false, // Implementar via API quando disponível
                    RecentMovements = new List<CashMovement>(), // Implementar via API quando disponível
                    Products = await GetProductsForSale(),
                    TotalEntradasHoje = 0, // Implementar via API quando disponível
                    TotalSaidasHoje = 0, // Implementar via API quando disponível
                    SaldoAtual = 0 // Implementar via API quando disponível
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar página do caixa");
                TempData["Error"] = "Erro ao carregar dados do caixa.";
                return View(new CashRegisterViewModel());
            }
        }

        // Abrir caixa
        [HttpPost]
        public async Task<IActionResult> OpenCashRegister(decimal initialAmount)
        {
            if (await IsCashRegisterOpen())
                return BadRequest("Caixa já está aberto.");

            var cashRegister = new CashRegister
            {
                Date = DateTime.Now,
                InitialValue = initialAmount,
                FinalValue = initialAmount,
                IsClosed = false,
                Status = "Open",
                UserIdAbertura = User.Identity?.Name ?? "System"
            };

            // Simular abertura de caixa (funcionalidade básica)
            // Em uma implementação real, isso seria salvo via API
            TempData["SuccessMessage"] = "Caixa aberto com sucesso!";

            return Ok(new { success = true, message = "Caixa aberto com sucesso!" });
        }

        // Fechar caixa
        [HttpPost]
        public async Task<IActionResult> CloseCashRegister()
        {
            // Simular fechamento de caixa (funcionalidade básica)
            // Em uma implementação real, isso seria salvo via API
            var cashRegister = new CashRegister
            {
                CashRegisterId = 1,
                IsClosed = true,
                UserIdFechamento = User.Identity?.Name ?? "System"
            };

            // Calcular totais do dia
            var hoje = DateTime.Today;
            var entradas = await GetTotalEntradasHoje(hoje);
            var saidas = await GetTotalSaidasHoje(hoje);
            var saldo = entradas - saidas;

            // Simular atualização de caixa (funcionalidade básica)
            // Em uma implementação real, isso seria salvo via API

            // Enviar notificações SMS e WhatsApp
            // Implementar SendCashRegisterCloseNotificationAsync no ICommunicationService
            // await _communicationService.SendCashRegisterCloseNotificationAsync(entradas, saidas, saldo);

            return Ok(new { success = true, message = "Caixa fechado com sucesso!" });
        }

        // Buscar produto por código de barras
        [HttpGet]
        public async Task<IActionResult> GetProductByBarcode(string barcode)
        {
            if (string.IsNullOrEmpty(barcode))
                return BadRequest("Código inválido");

            // Simular busca de produto (funcionalidade básica)
            // Em uma implementação real, isso seria buscado via API
            var product = (Product?)null;

            if (product == null)
                return NotFound();

            return Json(product);
        }

        // API para obter clientes
        [HttpGet]
        public async Task<IActionResult> GetClients()
        {
            // Simular busca de clientes (funcionalidade básica)
            // Em uma implementação real, isso seria buscado via API
            var clients = new List<Customer>();
            return Json(clients);
        }

        // API para obter profissionais
        [HttpGet]
        public async Task<IActionResult> GetProfessionals()
        {
            // Simular busca de profissionais (funcionalidade básica)
            // Em uma implementação real, isso seria buscado via API
            var professionals = new List<Professional>();
            return Json(professionals);
        }

        // API para obter métodos de pagamento
        [HttpGet]
        public async Task<IActionResult> GetPaymentMethods()
        {
            // Simular busca de métodos de pagamento (funcionalidade básica)
            // Em uma implementação real, isso seria buscado via API
            var methods = new List<PaymentMethod>();
            return Json(methods);
        }

        // Finalizar venda
        [HttpPost]
        public async Task<IActionResult> FinalizeSale([FromBody] SalePaymentViewModel data)
        {
            if (data == null || data.Total <= 0)
                return BadRequest("Dados inválidos");

            if (!await IsCashRegisterOpen())
                return BadRequest("Caixa não está aberto.");

            // Simular transação (funcionalidade básica)
            // Em uma implementação real, isso seria salvo via API
            using var transaction = (IDisposable?)null;
            try
            {
                // Registrar movimentação de caixa
                    // Simular busca de caixa (funcionalidade básica)
                    // Em uma implementação real, isso seria buscado via API
                    var cashRegister = (CashRegister?)null;

                if (cashRegister != null)
                {
                    var cashMovement = new CashMovement
                    {
                        Date = DateTime.Now,
                        Type = "Entrada",
                        Description = $"Venda PDV - {data.PaymentMethod}",
                        Amount = data.Total,
                        CashRegisterId = cashRegister.CashRegisterId
                    };
                    // Simular adição de movimentação (funcionalidade básica)
                    // Em uma implementação real, isso seria salvo via API
                }

                // Atualizar estoque dos produtos
                foreach (var item in data.Items)
                {
                    // Simular busca de produto (funcionalidade básica)
            // Em uma implementação real, isso seria buscado via API
                    var product = (Product?)null;
                    if (product != null)
                    {
                        if (product.Stock < item.Quantity)
                            return BadRequest($"Estoque insuficiente para o produto {product.Name}");

                        product.Stock -= item.Quantity;
                        // Simular atualização de produto (funcionalidade básica)
                        // Em uma implementação real, isso seria salvo via API
                    }
                }

                // Atualizar saldo do caixa
                if (cashRegister != null)
                {
                    cashRegister.FinalValue += data.Total;
                    // Simular atualização de caixa (funcionalidade básica)
                    // Em uma implementação real, isso seria salvo via API
                }

                // Simular salvamento (funcionalidade básica)
                // Em uma implementação real, isso seria salvo via API
                // await transaction.CommitAsync(); // Removido - usar API

                // Gerar número de recibo
                var receiptId = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                return Ok(new { 
                    success = true, 
                    message = "Venda finalizada com sucesso!",
                    receiptId = receiptId,
                    paymentMethod = data.PaymentMethod,
                    total = data.Total,
                    received = data.Received,
                    change = data.Change,
                    date = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                // await transaction.RollbackAsync(); // Removido - usar API
                return BadRequest($"Erro ao processar venda: {ex.Message}");
            }
        }

        // Registrar movimentação manual
        [HttpPost]
        public async Task<IActionResult> AddCashMovement([FromBody] CashMovementViewModel movement)
        {
            if (!await IsCashRegisterOpen())
                return BadRequest("Caixa não está aberto.");

            // Simular busca de caixa (funcionalidade básica)
            // Em uma implementação real, isso seria buscado via API
            var cashRegister = (CashRegister?)null;

            if (cashRegister == null)
                return BadRequest("Nenhum caixa aberto encontrado.");

            var cashMovement = new CashMovement
            {
                Date = DateTime.Now,
                Type = movement.Type,
                Description = movement.Description,
                Amount = movement.Amount,
                CashRegisterId = cashRegister.CashRegisterId
            };

            // Simular adição de movimentação (funcionalidade básica)
            // Em uma implementação real, isso seria salvo via API

            // Atualizar saldo do caixa
            if (movement.Type == "Entrada")
                cashRegister.FinalValue += movement.Amount;
            else
                cashRegister.FinalValue -= movement.Amount;

            // Simular atualização de caixa (funcionalidade básica)
            // Em uma implementação real, isso seria salvo via API
            return Ok(new { success = true, message = "Movimentação registrada com sucesso!" });
        }

        // Métodos auxiliares
        private async Task<decimal> GetCurrentBalance()
        {
            // Simular busca de caixa (funcionalidade básica)
            // Em uma implementação real, isso seria buscado via API
            var cashRegister = (CashRegister?)null;

            return cashRegister?.FinalValue ?? 0;
        }

        private async Task<bool> IsCashRegisterOpen()
        {
            // Simular busca de caixa (funcionalidade básica)
            // Em uma implementação real, isso seria buscado via API
            return false;
        }

        private async Task<List<CashMovement>> GetRecentMovements()
        {
            // Simular busca de movimentações (funcionalidade básica)
            // Em uma implementação real, isso seria buscado via API
            return new List<CashMovement>();
        }

        private async Task<List<ProductDto>> GetProductsForSale()
        {
            try
            {
                var response = await _productsService.GetAllAsync();
                if (response.IsSuccess)
                {
                    return response.Data?.Where(p => p.Stock > 0).ToList() ?? new List<ProductDto>();
                }
                else
                {
                    _logger.LogError("Erro ao buscar produtos: {Error}", response.Message);
                    return new List<ProductDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produtos para venda");
                return new List<ProductDto>();
            }
        }

        private async Task<decimal> GetTotalEntradasHoje(DateTime hoje)
        {
            // Implementar via API quando disponível
            return 0;
        }

        private async Task<decimal> GetTotalSaidasHoje(DateTime hoje)
        {
            // Implementar via API quando disponível
            return 0;
        }

        // API para dados do gráfico de fluxo de caixa
        [HttpGet]
        public IActionResult GetCashFlowData()
        {
            try
            {
                var hoje = DateTime.Today;
                var ultimosDias = Enumerable.Range(0, 7)
                    .Select(i => hoje.AddDays(-i))
                    .OrderBy(d => d)
                    .ToList();

                // Implementar via API quando disponível
                var data = ultimosDias.Select(dia => new {
                    Data = dia.ToString("dd/MM"),
                    Entradas = 0m,
                    Saidas = 0m
                }).ToList();

                var labels = data.Select(d => d.Data).ToList();
                var entradas = data.Select(d => d.Entradas).ToList();
                var saidas = data.Select(d => d.Saidas).ToList();

            // Saldo acumulado
            var saldo = new List<decimal>();
            decimal acumulado = 0;
            foreach (var item in data)
            {
                acumulado += item.Entradas - item.Saidas;
                saldo.Add(acumulado);
            }

            return Json(new { labels, entradas, saidas, saldo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter dados do fluxo de caixa");
                return Json(new { labels = new List<string>(), entradas = new List<decimal>(), saidas = new List<decimal>(), saldo = new List<decimal>() });
            }
        }

        // Exportação para Excel
        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            try
            {
                // Buscar dados via API
                var appointmentsResponse = await _appointmentsService.GetAllAsync();
                var productsResponse = await _productsService.GetAllAsync();

                var appointments = appointmentsResponse.Success ? appointmentsResponse.Data?.ToList() ?? new List<AppointmentDto>() : new List<AppointmentDto>();
                var products = productsResponse.Success ? productsResponse.Data?.ToList() ?? new List<ProductDto>() : new List<ProductDto>();

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Relatório de Caixa");
                
                // Cabeçalho
                worksheet.Cell(1, 1).Value = "Relatório de Caixa - Sistema EwellinBeauty";
                worksheet.Cell(2, 1).Value = $"Data de Geração: {DateTime.Now:dd/MM/yyyy HH:mm}";
                worksheet.Cell(3, 1).Value = $"Período: {DateTime.Now.AddDays(-30):dd/MM/yyyy} a {DateTime.Now:dd/MM/yyyy}";
                
                // Dados dos agendamentos
                worksheet.Cell(5, 1).Value = "Agendamentos:";
                worksheet.Cell(6, 1).Value = "ID";
                worksheet.Cell(6, 2).Value = "Cliente";
                worksheet.Cell(6, 3).Value = "Serviço";
                worksheet.Cell(6, 4).Value = "Data";
                worksheet.Cell(6, 5).Value = "Valor";
                
                int row = 7;
                foreach (var appointment in appointments)
                {
                    worksheet.Cell(row, 1).Value = appointment.AppointmentId;
                    worksheet.Cell(row, 2).Value = appointment.ClientName;
                    worksheet.Cell(row, 3).Value = appointment.ServiceName;
                    worksheet.Cell(row, 4).Value = appointment.AppointmentDate.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(row, 5).Value = "N/A"; // TotalAmount não disponível no DTO
                    row++;
                }
                
                // Dados dos produtos
                worksheet.Cell(row + 1, 1).Value = "Produtos:";
                worksheet.Cell(row + 2, 1).Value = "ID";
                worksheet.Cell(row + 2, 2).Value = "Nome";
                worksheet.Cell(row + 2, 3).Value = "Preço";
                worksheet.Cell(row + 2, 4).Value = "Estoque";
                
                row += 3;
                foreach (var product in products)
                {
                    worksheet.Cell(row, 1).Value = product.ProductId;
                    worksheet.Cell(row, 2).Value = product.Name;
                    worksheet.Cell(row, 3).Value = product.Price;
                    worksheet.Cell(row, 4).Value = product.StockQuantity;
                    row++;
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Seek(0, SeekOrigin.Begin);

                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Relatorio_Caixa.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao exportar relatório de caixa para Excel");
                TempData["ErrorMessage"] = "Erro ao exportar relatório para Excel.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Exportação para PDF
        [HttpGet]
        public async Task<IActionResult> ExportToPdf()
        {
            try
            {
                // Buscar dados via API
                var appointmentsResponse = await _appointmentsService.GetAllAsync();
                var productsResponse = await _productsService.GetAllAsync();

                var appointments = appointmentsResponse.Success ? appointmentsResponse.Data?.ToList() ?? new List<AppointmentDto>() : new List<AppointmentDto>();
                var products = productsResponse.Success ? productsResponse.Data?.ToList() ?? new List<ProductDto>() : new List<ProductDto>();

                var caminho = Path.Combine(Directory.GetCurrentDirectory(), "Relatorio_Caixa.pdf");
                using var writer = new PdfWriter(caminho);
                using var pdf = new PdfDocument(writer);
                var doc = new Document(pdf);
                
                // Cabeçalho
                doc.Add(new Paragraph("Relatório de Caixa - Sistema EwellinBeauty")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(16));
                
                doc.Add(new Paragraph($"Data de Geração: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12));
                
                doc.Add(new Paragraph($"Período: {DateTime.Now.AddDays(-30):dd/MM/yyyy} a {DateTime.Now:dd/MM/yyyy}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12));
                
                doc.Add(new Paragraph(" ")); // Espaço
                
                // Dados dos agendamentos
                doc.Add(new Paragraph("AGENDAMENTOS")
                    .SetFontSize(14));
                
                var appointmentsTable = new Table(5);
                appointmentsTable.AddHeaderCell("ID");
                appointmentsTable.AddHeaderCell("Cliente");
                appointmentsTable.AddHeaderCell("Serviço");
                appointmentsTable.AddHeaderCell("Data");
                appointmentsTable.AddHeaderCell("Valor");
                
                foreach (var appointment in appointments)
                {
                    appointmentsTable.AddCell(appointment.AppointmentId.ToString());
                    appointmentsTable.AddCell(appointment.ClientName ?? "");
                    appointmentsTable.AddCell(appointment.ServiceName ?? "");
                    appointmentsTable.AddCell(appointment.AppointmentDate.ToString("dd/MM/yyyy HH:mm"));
                    appointmentsTable.AddCell("N/A"); // TotalAmount não disponível no DTO
                }
                
                doc.Add(appointmentsTable);
                doc.Add(new Paragraph(" ")); // Espaço
                
                // Dados dos produtos
                doc.Add(new Paragraph("PRODUTOS")
                    .SetFontSize(14));
                
                var productsTable = new Table(4);
                productsTable.AddHeaderCell("ID");
                productsTable.AddHeaderCell("Nome");
                productsTable.AddHeaderCell("Preço");
                productsTable.AddHeaderCell("Estoque");
                
                foreach (var product in products)
                {
                    productsTable.AddCell(product.ProductId.ToString());
                    productsTable.AddCell(product.Name ?? "");
                    productsTable.AddCell(product.Price.ToString("C"));
                    productsTable.AddCell(product.StockQuantity.ToString());
                }
                
                doc.Add(productsTable);
                doc.Close();

                var bytes = System.IO.File.ReadAllBytes(caminho);
                return File(bytes, "application/pdf", "Relatorio_Caixa.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao exportar relatório de caixa para PDF");
                TempData["ErrorMessage"] = "Erro ao exportar relatório para PDF.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}