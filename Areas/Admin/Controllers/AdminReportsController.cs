using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.Models.Admin;
using Sistema.Services;
using Sistema.Services.Api;
using SistemaAPI.DTOs;
using ClosedXML.Excel;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminReportsController : Controller
    {
        private readonly IApiAppointmentsService _appointmentsService;
        private readonly IApiClientsService _clientsService;
        private readonly IApiServicesService _servicesService;
        private readonly IApiStaffService _staffService;
        private readonly IExcelExportService _excelExportService;
        private readonly IPdfExportService _pdfExportService;
        private readonly ILogger<AdminReportsController> _logger;

        public AdminReportsController(
            IApiAppointmentsService appointmentsService,
            IApiClientsService clientsService,
            IApiServicesService servicesService,
            IApiStaffService staffService,
            IExcelExportService excelExportService,
            IPdfExportService pdfExportService,
            ILogger<AdminReportsController> logger)
        {
            _appointmentsService = appointmentsService;
            _clientsService = clientsService;
            _servicesService = servicesService;
            _staffService = staffService;
            _excelExportService = excelExportService;
            _pdfExportService = pdfExportService;
            _logger = logger;
        }

        // GET: Admin/Reports
        public async Task<IActionResult> Index()
        {
            try
            {
                // Buscar dados via API
                var appointmentsResponse = await _appointmentsService.GetAllAsync();
                var clientsResponse = await _clientsService.GetAllAsync();
                var servicesResponse = await _servicesService.GetAllAsync();

                var appointments = appointmentsResponse.Success ? appointmentsResponse.Data?.ToList() ?? new List<AppointmentDto>() : new List<AppointmentDto>();
                var clients = clientsResponse.Success ? clientsResponse.Data?.ToList() ?? new List<ClientDto>() : new List<ClientDto>();
                var services = servicesResponse.Success ? servicesResponse.Data?.ToList() ?? new List<ServiceDto>() : new List<ServiceDto>();

                // Calcular estatísticas básicas
                var reportData = new
                {
                    TotalAppointments = appointments.Count,
                    PendingAppointments = appointments.Count(a => a.Status == "Pendente"),
                    ConfirmedAppointments = appointments.Count(a => a.Status == "Confirmado"),
                    CompletedAppointments = appointments.Count(a => a.Status == "Concluído"),
                    TotalClients = clients.Count,
                    NewClientsThisMonthCount = clients.Count(c => c.RegistrationDate >= DateTime.Today.AddMonths(-1)),
                    TotalServices = services.Count,
                    MostPopularService = services.OrderByDescending(s => s.Name).FirstOrDefault()?.Name ?? "N/A",
                    RevenueThisMonth = appointments
                        .Where(a => a.AppointmentDate >= DateTime.Today.AddMonths(-1) && a.TotalPrice.HasValue)
                        .Sum(a => a.TotalPrice.Value),
                    AppointmentsByService = appointments
                        .GroupBy(a => a.ServiceName)
                        .Select(g => new ReportItem 
                        { 
                            Name = g.Key, 
                            Value = g.Count() 
                        })
                        .OrderByDescending(x => x.Value)
                        .Take(5)
                        .ToList(),
                    NewClientsThisMonth = clients
                        .Where(c => c.RegistrationDate >= DateTime.Today.AddMonths(-1))
                        .Count(),
                    Last7Days = Enumerable.Range(0, 7)
                        .Select(i => DateTime.Today.AddDays(-i))
                        .OrderBy(d => d)
                        .ToList()
                };

                var data = reportData.Last7Days.Select(day => new
                {
                    Date = day.ToString("dd/MM"),
                    Appointments = appointments.Count(a => a.AppointmentDate.Date == day.Date),
                    Revenue = appointments
                        .Where(a => a.AppointmentDate.Date == day.Date && a.TotalPrice.HasValue)
                        .Sum(a => a.TotalPrice.Value)
                }).ToList();

                ViewBag.ReportData = reportData;
                ViewBag.ChartData = data;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar relatórios");
                TempData["ErrorMessage"] = "Erro ao carregar dados dos relatórios.";
                return View();
            }
        }

        // Relatório de agendamentos
        public async Task<IActionResult> AppointmentsReport(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Now.AddDays(-30);
                var end = endDate ?? DateTime.Now;

                var appointmentsResponse = await _appointmentsService.GetAllAsync();
                var appointments = appointmentsResponse.Success ? 
                    appointmentsResponse.Data?.Where(a => a.AppointmentDate >= start && a.AppointmentDate <= end).ToList() ?? new List<AppointmentDto>() : 
                    new List<AppointmentDto>();

                var reportData = new
                {
                    StartDate = start,
                    EndDate = end,
                    TotalAppointments = appointments.Count,
                    PendingAppointments = appointments.Count(a => a.Status == "Pendente"),
                    ConfirmedAppointments = appointments.Count(a => a.Status == "Confirmado"),
                    CompletedAppointments = appointments.Count(a => a.Status == "Concluído"),
                    CancelledAppointments = appointments.Count(a => a.Status == "Cancelado"),
                    TotalRevenue = appointments.Where(a => a.TotalPrice.HasValue).Sum(a => a.TotalPrice.Value),
                    AppointmentsByService = appointments
                        .GroupBy(a => a.ServiceName)
                        .Select(g => new { Service = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .ToList(),
                    AppointmentsByProfessional = appointments
                        .GroupBy(a => a.ProfessionalName)
                        .Select(g => new { Professional = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .ToList()
                };

                ViewBag.ReportData = reportData;
                ViewBag.Appointments = appointments;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar relatório de agendamentos");
                TempData["ErrorMessage"] = "Erro ao gerar relatório de agendamentos.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Relatório de clientes
        public async Task<IActionResult> ClientsReport(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Now.AddDays(-30);
                var end = endDate ?? DateTime.Now;

                var clientsResponse = await _clientsService.GetAllAsync();
                var clients = clientsResponse.Success ? 
                    clientsResponse.Data?.Where(c => c.RegistrationDate >= start && c.RegistrationDate <= end).ToList() ?? new List<ClientDto>() : 
                    new List<ClientDto>();

                // Buscar agendamentos para calcular clientes ativos
                var appointmentsResponse = await _appointmentsService.GetAllAsync();
                var activeClients = new HashSet<string>();

                if (appointmentsResponse.IsSuccess && appointmentsResponse.Data != null)
                {
                    var recentAppointments = appointmentsResponse.Data
                        .Where(a => a.AppointmentDate >= start && a.AppointmentDate <= end)
                        .ToList();

                    activeClients = recentAppointments
                        .Select(a => a.ClientEmail)
                        .Where(email => !string.IsNullOrEmpty(email))
                        .ToHashSet();
                }

                var reportData = new
                {
                    StartDate = start,
                    EndDate = end,
                    TotalClients = clients.Count,
                    NewClients = clients.Count(c => c.RegistrationDate >= start && c.RegistrationDate <= end),
                    ActiveClients = activeClients.Count,
                    ClientsByMonth = clients
                        .GroupBy(c => new { c.RegistrationDate.Year, c.RegistrationDate.Month })
                        .Select(g => new { 
                            Month = $"{g.Key.Month:00}/{g.Key.Year}", 
                            Count = g.Count() 
                        })
                        .OrderBy(x => x.Month)
                        .ToList()
                };

                ViewBag.ReportData = reportData;
                ViewBag.Clients = clients;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar relatório de clientes");
                TempData["ErrorMessage"] = "Erro ao gerar relatório de clientes.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Relatório financeiro
        public async Task<IActionResult> FinancialReport(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Now.AddDays(-30);
                var end = endDate ?? DateTime.Now;

                // Buscar agendamentos para calcular receita
                var appointmentsResponse = await _appointmentsService.GetAllAsync();
                var appointments = appointmentsResponse.Success ? 
                    appointmentsResponse.Data?.Where(a => a.AppointmentDate >= start && a.AppointmentDate <= end).ToList() ?? new List<AppointmentDto>() : 
                    new List<AppointmentDto>();

                // Buscar serviços para calcular preços
                var servicesResponse = await _servicesService.GetAllAsync();
                var services = servicesResponse.Success ? 
                    servicesResponse.Data?.ToList() ?? new List<ServiceDto>() : 
                    new List<ServiceDto>();

                // Calcular receita baseada nos agendamentos
                var totalRevenue = 0m;
                var revenueByMonth = new Dictionary<string, decimal>();
                var paymentMethods = new Dictionary<string, int>();

                foreach (var appointment in appointments)
                {
                    var service = services.FirstOrDefault(s => s.ServiceId == appointment.ServiceId);
                    if (service != null)
                    {
                        totalRevenue += service.Price;
                        
                        var monthKey = $"{appointment.AppointmentDate.Month:00}/{appointment.AppointmentDate.Year}";
                        if (!revenueByMonth.ContainsKey(monthKey))
                            revenueByMonth[monthKey] = 0;
                        revenueByMonth[monthKey] += service.Price;
                    }
                }

                var reportData = new
                {
                    StartDate = start,
                    EndDate = end,
                    TotalRevenue = totalRevenue,
                    TotalExpenses = 0m, // Despesas não estão disponíveis via API ainda
                    NetProfit = totalRevenue,
                    RevenueByMonth = revenueByMonth.Select(kvp => new { Month = kvp.Key, Amount = kvp.Value }).OrderBy(x => x.Month).ToList(),
                    ExpensesByMonth = new List<object>(), // Despesas não estão disponíveis via API ainda
                    PaymentMethods = new List<object>() // Métodos de pagamento não estão disponíveis via API ainda
                };

                ViewBag.ReportData = reportData;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar relatório financeiro");
                TempData["ErrorMessage"] = "Erro ao gerar relatório financeiro.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Exportar relatório para Excel
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(string reportType, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Relatório Administrativo");
                
                // Cabeçalho
                worksheet.Cell(1, 1).Value = "Relatório Administrativo - Sistema EwellinBeauty";
                worksheet.Cell(2, 1).Value = $"Data de Geração: {DateTime.Now:dd/MM/yyyy HH:mm}";
                worksheet.Cell(3, 1).Value = "Este relatório foi gerado automaticamente pelo painel administrativo.";
                
                // Dados do relatório
                worksheet.Cell(5, 1).Value = "Resumo do Sistema:";
                worksheet.Cell(6, 1).Value = "Total de Agendamentos:";
                worksheet.Cell(6, 2).Value = "N/A"; // TotalAppointments não disponível
                worksheet.Cell(7, 1).Value = "Total de Clientes:";
                worksheet.Cell(7, 2).Value = "N/A"; // TotalClients não disponível
                worksheet.Cell(8, 1).Value = "Total de Serviços:";
                worksheet.Cell(8, 2).Value = "N/A"; // TotalServices não disponível
                worksheet.Cell(9, 1).Value = "Total de Profissionais:";
                worksheet.Cell(9, 2).Value = "N/A"; // TotalProfessionals não disponível
                
                // Formatação
                worksheet.Range(1, 1, 1, 2).Merge().Style.Font.Bold = true;
                worksheet.Range(1, 1, 1, 2).Style.Font.FontSize = 16;
                worksheet.Range(5, 1, 9, 1).Style.Font.Bold = true;

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Seek(0, SeekOrigin.Begin);

                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Relatorio_Admin.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao exportar relatório para Excel");
                TempData["ErrorMessage"] = "Erro ao exportar relatório para Excel.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Exportar relatório para PDF
        [HttpGet]
        public async Task<IActionResult> ExportToPdf(string reportType, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var caminho = Path.Combine(Directory.GetCurrentDirectory(), "Relatorio_Admin.pdf");
                using var writer = new PdfWriter(caminho);
                using var pdf = new PdfDocument(writer);
                var doc = new Document(pdf);
                
                // Cabeçalho
                doc.Add(new Paragraph("Relatório Administrativo - Sistema EwellinBeauty")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(16));
                
                doc.Add(new Paragraph($"Data de Geração: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12));
                
                doc.Add(new Paragraph("Este relatório foi gerado automaticamente pelo painel administrativo.")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10));
                
                doc.Add(new Paragraph(" ")); // Espaço
                
                // Resumo do Sistema
                doc.Add(new Paragraph("RESUMO DO SISTEMA")
                    .SetFontSize(14));
                
                var summaryTable = new Table(2);
                summaryTable.AddHeaderCell("Métrica");
                summaryTable.AddHeaderCell("Valor");
                
                summaryTable.AddCell("Total de Agendamentos");
                summaryTable.AddCell("N/A"); // TotalAppointments não disponível
                summaryTable.AddCell("Total de Clientes");
                summaryTable.AddCell("N/A"); // TotalClients não disponível
                summaryTable.AddCell("Total de Serviços");
                summaryTable.AddCell("N/A"); // TotalServices não disponível
                summaryTable.AddCell("Total de Profissionais");
                summaryTable.AddCell("N/A"); // TotalProfessionals não disponível
                
                doc.Add(summaryTable);
                doc.Close();

                var bytes = System.IO.File.ReadAllBytes(caminho);
                return File(bytes, "application/pdf", "Relatorio_Admin.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao exportar relatório para PDF");
                TempData["ErrorMessage"] = "Erro ao exportar relatório para PDF.";
                return RedirectToAction(nameof(Index));
            }
        }
    }

    public class ReportItem
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}