using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Helpers;
using Sistema.Models.Admin;
using Sistema.Services;
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
        private readonly SistemaDbContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IExcelExportService _excelExportService;
        private readonly IPdfExportService _pdfExportService;

        public AdminReportsController(SistemaDbContext context, IUserHelper userHelper, IExcelExportService excelExportService, IPdfExportService pdfExportService)
        {
            _context = context;
            _userHelper = userHelper;
            _excelExportService = excelExportService;
            _pdfExportService = pdfExportService;
        }

        // GET: Admin/Reports
        public IActionResult Index()
        {
            var model = new AdminReportsViewModel
            {
                TotalEntradas = _context.CashMovements
                    .Where(m => m.Type == "Entrada")
                    .Sum(m => (decimal?)m.Amount) ?? 0,
                TotalSaidas = _context.CashMovements
                    .Where(m => m.Type == "Saída")
                    .Sum(m => (decimal?)m.Amount) ?? 0,
                ServicosMaisVendidos = _context.Appointments
                    .Include(a => a.Service)
                    .GroupBy(a => a.Service.Name)
                    .Select(g => new ReportItem { Label = g.Key, Value = g.Count() })
                    .OrderByDescending(x => x.Value)
                    .Take(5)
                    .ToList(),
                ClientesNovos = _context.Customers
                    .Where(c => c.RegistrationDate >= DateTime.Today.AddMonths(-1))
                    .Count(),
                ClientesTotais = _context.Customers.Count()
            };

            model.SaldoAtual = model.TotalEntradas - model.TotalSaidas;

            return View(model);
        }

        // API para gráfico financeiro (últimos 7 dias)
        [HttpGet]
        public IActionResult GetCashFlowChartData()
        {
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            var data = last7Days.Select(day => new
            {
                Day = day.ToString("dd/MM"),
                Entradas = _context.CashMovements
                    .Where(m => m.Type == "Entrada" && m.Date.Date == day)
                    .Sum(m => (decimal?)m.Amount) ?? 0,
                Saidas = _context.CashMovements
                    .Where(m => m.Type == "Saída" && m.Date.Date == day)
                    .Sum(m => (decimal?)m.Amount) ?? 0
            }).ToList();

            return Json(data);
        }

        // Exportação simples para Excel (GET)
        [HttpGet]
        public IActionResult ExportToExcel()
        {
            try
            {
                // Gerar DataTable com resumo financeiro
                var entries = _context.CashMovements
                    .OrderByDescending(m => m.Date)
                    .Select(m => new
                    {
                        m.Date,
                        m.Type,
                        m.Description,
                        m.Amount
                    })
                    .ToList();

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Movimentações");
                
                // Cabeçalhos
                worksheet.Cell(1, 1).Value = "Data";
                worksheet.Cell(1, 2).Value = "Tipo";
                worksheet.Cell(1, 3).Value = "Descrição";
                worksheet.Cell(1, 4).Value = "Valor";
                
                // Dados
                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    worksheet.Cell(i + 2, 1).Value = entry.Date.ToString("dd/MM/yyyy");
                    worksheet.Cell(i + 2, 2).Value = entry.Type;
                    worksheet.Cell(i + 2, 3).Value = entry.Description;
                    worksheet.Cell(i + 2, 4).Value = entry.Amount;
                }
                
                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Relatorio_Financeiro.xlsx");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao gerar relatório Excel: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Exportação simples para PDF (GET)
        [HttpGet]
        public IActionResult ExportToPDF()
        {
            try
            {
                var summary = new
                {
                    TotalEntradas = _context.CashMovements
                        .Where(m => m.Type == "Entrada")
                        .Sum(m => (decimal?)m.Amount) ?? 0,
                    TotalSaidas = _context.CashMovements
                        .Where(m => m.Type == "Saída")
                        .Sum(m => (decimal?)m.Amount) ?? 0,
                    ClientesTotais = _context.Customers.Count(),
                    ClientesNovos = _context.Customers
                        .Where(c => c.RegistrationDate >= DateTime.Today.AddMonths(-1))
                        .Count()
                };

                var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Relatório Administrativo</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .summary-grid {{ display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }}
        .summary-item {{ background: #f8f9fa; padding: 15px; border-radius: 8px; border-left: 4px solid #007bff; }}
        .summary-item h4 {{ margin: 0 0 10px 0; color: #007bff; }}
        .summary-item .value {{ font-size: 18px; font-weight: bold; color: #333; }}
        .footer {{ margin-top: 30px; text-align: center; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>Relatório Administrativo</h1>
        <p>Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}</p>
    </div>

    <div class='summary-grid'>
        <div class='summary-item'>
            <h4>Total Entradas</h4>
            <div class='value'>€ {summary.TotalEntradas:F2}</div>
        </div>
        <div class='summary-item'>
            <h4>Total Saídas</h4>
            <div class='value'>€ {summary.TotalSaidas:F2}</div>
        </div>
        <div class='summary-item'>
            <h4>Saldo Atual</h4>
            <div class='value'>€ {(summary.TotalEntradas - summary.TotalSaidas):F2}</div>
        </div>
        <div class='summary-item'>
            <h4>Total Clientes</h4>
            <div class='value'>{summary.ClientesTotais}</div>
        </div>
        <div class='summary-item'>
            <h4>Novos Clientes (30 dias)</h4>
            <div class='value'>{summary.ClientesNovos}</div>
        </div>
    </div>

    <div class='footer'>
        <p>Relatório gerado pelo Sistema EwellinBeauty</p>
    </div>
</body>
</html>";

                var bytes = System.Text.Encoding.UTF8.GetBytes(html);
                return File(bytes, "text/html", "Relatorio_Administrativo.html");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao gerar relatório PDF: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Exportação para PDF com iText7
        [HttpGet]
        public IActionResult ExportToPdf()
        {
            try
            {
                var entradas = _context.CashMovements
                    .Where(m => m.Type == "Entrada")
                    .Sum(m => (decimal?)m.Amount) ?? 0;
                var saidas = _context.CashMovements
                    .Where(m => m.Type == "Saída")
                    .Sum(m => (decimal?)m.Amount) ?? 0;
                var saldo = entradas - saidas;

                var servicosMaisVendidos = _context.Appointments
                    .Include(a => a.Service)
                    .GroupBy(a => a.Service.Name)
                    .Select(g => new { Servico = g.Key, Quantidade = g.Count() })
                    .OrderByDescending(x => x.Quantidade)
                    .Take(5)
                    .ToList();

                var clientesTotais = _context.Customers.Count();
                var clientesNovos = _context.Customers
                    .Where(c => c.RegistrationDate >= DateTime.Today.AddMonths(-1))
                    .Count();

                using var memoryStream = new MemoryStream();
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Cabeçalho
                document.Add(new Paragraph("Ewellin Beauty - Relatório Administrativo")
                    .SetFontSize(18)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontColor(ColorConstants.DARK_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph($"Emitido em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontColor(ColorConstants.GRAY));

                document.Add(new Paragraph("\n"));

                // Resumo Financeiro
                document.Add(new Paragraph("Resumo Financeiro")
                    .SetFontSize(14)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontColor(ColorConstants.BLUE));
                var table = new Table(UnitValue.CreatePercentArray(3)).UseAllAvailableWidth();
                table.AddHeaderCell(new Cell().Add(new Paragraph("Entradas (€)")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Saídas (€)")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Saldo (€)")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))));

                table.AddCell(entradas.ToString("N2"));
                table.AddCell(saidas.ToString("N2"));
                table.AddCell(saldo.ToString("N2"));

                document.Add(table);
                document.Add(new Paragraph("\n"));

                // Serviços mais realizados
                document.Add(new Paragraph("Serviços Mais Realizados")
                    .SetFontSize(14)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontColor(ColorConstants.BLUE));
                var servicosTable = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth();
                servicosTable.AddHeaderCell(new Cell().Add(new Paragraph("Serviço")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))));
                servicosTable.AddHeaderCell(new Cell().Add(new Paragraph("Quantidade")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))));

                foreach (var s in servicosMaisVendidos)
                {
                    servicosTable.AddCell(s.Servico);
                    servicosTable.AddCell(s.Quantidade.ToString());
                }

                document.Add(servicosTable);
                document.Add(new Paragraph("\n"));

                // Relatório de Clientes
                document.Add(new Paragraph("Relatório de Clientes")
                    .SetFontSize(14)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontColor(ColorConstants.BLUE));
                var clientesTable = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth();
                clientesTable.AddHeaderCell(new Cell().Add(new Paragraph("Indicador")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))));
                clientesTable.AddHeaderCell(new Cell().Add(new Paragraph("Valor")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))));

                clientesTable.AddCell("Total de Clientes");
                clientesTable.AddCell(clientesTotais.ToString());

                clientesTable.AddCell("Novos no Último Mês");
                clientesTable.AddCell(clientesNovos.ToString());

                document.Add(clientesTable);
                document.Add(new Paragraph("\n\n"));

                // Assinatura
                document.Add(new Paragraph("_________________________________________")
                    .SetTextAlignment(TextAlignment.CENTER));
                document.Add(new Paragraph("Gestor Responsável")
                    .SetFontSize(11)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontColor(ColorConstants.GRAY));

                document.Close();

                return File(memoryStream.ToArray(), "application/pdf", "Relatorio_Administrativo.pdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao gerar relatório PDF: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Reports/SalesReport
        public async Task<IActionResult> SalesReport(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;

            var sales = await _context.Sales
                .Include(s => s.Customer)
                    .ThenInclude(c => c.User)
                .Include(s => s.Professional)
                .Include(s => s.PaymentMethod)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Product)
                .Where(s => s.SaleDate >= start && s.SaleDate <= end)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            var summary = new
            {
                TotalSales = sales.Count,
                TotalAmount = sales.Sum(s => s.FinalTotal),
                TotalItems = sales.Sum(s => s.Items.Sum(i => i.Quantity)),
                AverageTicket = sales.Any() ? sales.Average(s => s.FinalTotal) : 0,
                SalesByPaymentMethod = sales.GroupBy(s => s.PaymentMethod.Name)
                    .Select(g => new { Method = g.Key, Count = g.Count(), Total = g.Sum(s => s.FinalTotal) })
                    .ToList(),
                SalesByProfessional = sales.GroupBy(s => s.Professional.Name)
                    .Select(g => new { Professional = g.Key, Count = g.Count(), Total = g.Sum(s => s.FinalTotal) })
                    .ToList()
            };

            ViewBag.StartDate = start;
            ViewBag.EndDate = end;
            ViewBag.Summary = summary;

            return View(sales);
        }

        // GET: Admin/Reports/CashFlowReport
        public async Task<IActionResult> CashFlowReport(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;

            var receivables = await _context.Receivables
                .Include(r => r.Customer)
                    .ThenInclude(c => c.User)
                .Include(r => r.Professional)
                .Include(r => r.PaymentMethod)
                .Where(r => r.CreatedAt >= start && r.CreatedAt <= end)
                .ToListAsync();

            var payables = await _context.Payables
                .Include(p => p.Professional)
                .Include(p => p.Supplier)
                .Include(p => p.PaymentMethod)
                .Where(p => p.CreatedAt >= start && p.CreatedAt <= end)
                .ToListAsync();

            var cashMovements = await _context.CashMovements
                .Include(cm => cm.CashRegister)
                .Where(cm => cm.Date >= start && cm.Date <= end)
                .ToListAsync();

            var summary = new
            {
                TotalReceivables = receivables.Sum(r => r.Amount),
                TotalPaidReceivables = receivables.Where(r => r.IsPaid).Sum(r => r.Amount),
                TotalPendingReceivables = receivables.Where(r => !r.IsPaid).Sum(r => r.Amount),
                TotalPayables = payables.Sum(p => p.Amount),
                TotalPaidPayables = payables.Where(p => p.IsPaid).Sum(p => p.Amount),
                TotalPendingPayables = payables.Where(p => !p.IsPaid).Sum(p => p.Amount),
                TotalEntradas = cashMovements.Where(cm => cm.Type == "Entrada").Sum(cm => cm.Amount),
                TotalSaidas = cashMovements.Where(cm => cm.Type == "Saída").Sum(cm => cm.Amount),
                SaldoLiquido = cashMovements.Where(cm => cm.Type == "Entrada").Sum(cm => cm.Amount) - 
                              cashMovements.Where(cm => cm.Type == "Saída").Sum(cm => cm.Amount)
            };

            ViewBag.StartDate = start;
            ViewBag.EndDate = end;
            ViewBag.Summary = summary;
            ViewBag.Receivables = receivables;
            ViewBag.Payables = payables;
            ViewBag.CashMovements = cashMovements;

            return View();
        }

        // GET: Admin/Reports/CommissionsReport
        public async Task<IActionResult> CommissionsReport(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;

            var commissions = await _context.Payables
                .Include(p => p.Professional)
                .Include(p => p.Sale)
                .Where(p => p.Type == "Commission" && p.CreatedAt >= start && p.CreatedAt <= end)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var summary = new
            {
                TotalCommissions = commissions.Sum(p => p.Amount),
                TotalPaidCommissions = commissions.Where(p => p.IsPaid).Sum(p => p.Amount),
                TotalPendingCommissions = commissions.Where(p => !p.IsPaid).Sum(p => p.Amount),
                CommissionsByProfessional = commissions.GroupBy(p => p.Professional.Name)
                    .Select(g => new { 
                        Professional = g.Key, 
                        Total = g.Sum(p => p.Amount),
                        Paid = g.Where(p => p.IsPaid).Sum(p => p.Amount),
                        Pending = g.Where(p => !p.IsPaid).Sum(p => p.Amount),
                        Count = g.Count()
                    })
                    .ToList()
            };

            ViewBag.StartDate = start;
            ViewBag.EndDate = end;
            ViewBag.Summary = summary;

            return View(commissions);
        }

        // GET: Admin/Reports/FinancialSummary
        public async Task<IActionResult> FinancialSummary(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;

            // Dados consolidados
            var receivables = await _context.Receivables
                .Where(r => r.CreatedAt >= start && r.CreatedAt <= end)
                .ToListAsync();

            var payables = await _context.Payables
                .Where(p => p.CreatedAt >= start && p.CreatedAt <= end)
                .ToListAsync();

            var sales = await _context.Sales
                .Where(s => s.SaleDate >= start && s.SaleDate <= end)
                .ToListAsync();

            var cashMovements = await _context.CashMovements
                .Where(cm => cm.Date >= start && cm.Date <= end)
                .ToListAsync();

            var summary = new
            {
                // Receitas
                TotalReceitas = receivables.Sum(r => r.Amount),
                ReceitasPagas = receivables.Where(r => r.IsPaid).Sum(r => r.Amount),
                ReceitasPendentes = receivables.Where(r => !r.IsPaid).Sum(r => r.Amount),

                // Despesas
                TotalDespesas = payables.Sum(p => p.Amount),
                DespesasPagas = payables.Where(p => p.IsPaid).Sum(p => p.Amount),
                DespesasPendentes = payables.Where(p => !p.IsPaid).Sum(p => p.Amount),

                // Vendas
                TotalVendas = sales.Sum(s => s.FinalTotal),
                QuantidadeVendas = sales.Count,

                // Caixa
                TotalEntradas = cashMovements.Where(cm => cm.Type == "Entrada").Sum(cm => cm.Amount),
                TotalSaidas = cashMovements.Where(cm => cm.Type == "Saída").Sum(cm => cm.Amount),
                SaldoLiquido = cashMovements.Where(cm => cm.Type == "Entrada").Sum(cm => cm.Amount) - 
                              cashMovements.Where(cm => cm.Type == "Saída").Sum(cm => cm.Amount),

                // Comissões
                TotalComissoes = payables.Where(p => p.Type == "Commission").Sum(p => p.Amount),
                ComissoesPagas = payables.Where(p => p.Type == "Commission" && p.IsPaid).Sum(p => p.Amount),
                ComissoesPendentes = payables.Where(p => p.Type == "Commission" && !p.IsPaid).Sum(p => p.Amount)
            };

            ViewBag.StartDate = start;
            ViewBag.EndDate = end;
            ViewBag.Summary = summary;

            return View();
        }

        // POST: Admin/Reports/ExportToPDF
        [HttpPost]
        public async Task<IActionResult> ExportToPDF(string reportType, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Now.AddDays(-30);
                var end = endDate ?? DateTime.Now;

                byte[] pdfBytes;
                string fileName;

                switch (reportType.ToLower())
                {
                    case "summary":
                        var summary = await GetFinancialSummary(start, end);
                        pdfBytes = _pdfExportService.ExportFinancialSummaryToPdf(summary, start, end);
                        fileName = $"Resumo_Financeiro_{start:yyyyMMdd}_{end:yyyyMMdd}.pdf";
                        break;

                    case "sales":
                        var sales = await GetSalesData(start, end);
                        var salesSummary = await GetSalesSummary(start, end);
                        pdfBytes = _pdfExportService.ExportSalesReportToPdf(sales, salesSummary);
                        fileName = $"Relatorio_Vendas_{start:yyyyMMdd}_{end:yyyyMMdd}.pdf";
                        break;

                    case "cashflow":
                        var cashFlowSummary = await GetCashFlowSummary(start, end);
                        var receivables = await GetReceivablesData(start, end);
                        var payables = await GetPayablesData(start, end);
                        var cashMovements = await GetCashMovementsData(start, end);
                        pdfBytes = _pdfExportService.ExportCashFlowReportToPdf(cashFlowSummary, receivables, payables, cashMovements);
                        fileName = $"Fluxo_Caixa_{start:yyyyMMdd}_{end:yyyyMMdd}.pdf";
                        break;

                    case "commissions":
                        var commissions = await GetCommissionsData(start, end);
                        var commissionsSummary = await GetCommissionsSummary(start, end);
                        pdfBytes = _pdfExportService.ExportCommissionsReportToPdf(commissions, commissionsSummary);
                        fileName = $"Comissoes_{start:yyyyMMdd}_{end:yyyyMMdd}.pdf";
                        break;

                    default:
                        TempData["ErrorMessage"] = "Tipo de relatório não reconhecido.";
                        return RedirectToAction(nameof(Index));
                }

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao gerar relatório PDF: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Reports/ExportToExcel
        [HttpPost]
        public async Task<IActionResult> ExportToExcel(string reportType, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Now.AddDays(-30);
                var end = endDate ?? DateTime.Now;

                byte[] excelBytes;
                string fileName;

                switch (reportType.ToLower())
                {
                    case "summary":
                        var summary = await GetFinancialSummary(start, end);
                        excelBytes = _excelExportService.ExportFinancialSummaryToExcel(summary, start, end);
                        fileName = $"Resumo_Financeiro_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx";
                        break;

                    case "sales":
                        var sales = await GetSalesData(start, end);
                        var salesSummary = await GetSalesSummary(start, end);
                        excelBytes = _excelExportService.ExportSalesReportToExcel(sales, salesSummary);
                        fileName = $"Relatorio_Vendas_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx";
                        break;

                    case "cashflow":
                        var cashFlowSummary = await GetCashFlowSummary(start, end);
                        var receivables = await GetReceivablesData(start, end);
                        var payables = await GetPayablesData(start, end);
                        var cashMovements = await GetCashMovementsData(start, end);
                        excelBytes = _excelExportService.ExportCashFlowReportToExcel(cashFlowSummary, receivables, payables, cashMovements);
                        fileName = $"Fluxo_Caixa_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx";
                        break;

                    case "commissions":
                        var commissions = await GetCommissionsData(start, end);
                        var commissionsSummary = await GetCommissionsSummary(start, end);
                        excelBytes = _excelExportService.ExportCommissionsReportToExcel(commissions, commissionsSummary);
                        fileName = $"Comissoes_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx";
                        break;

                    default:
                        TempData["ErrorMessage"] = "Tipo de relatório não reconhecido.";
                        return RedirectToAction(nameof(Index));
                }

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao gerar relatório Excel: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Reports/GetChartData
        public async Task<IActionResult> GetChartData(string chartType, DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;

            switch (chartType.ToLower())
            {
                case "sales":
                    var salesData = await _context.Sales
                        .Where(s => s.SaleDate >= start && s.SaleDate <= end)
                        .GroupBy(s => s.SaleDate.Date)
                        .Select(g => new { Date = g.Key, Total = g.Sum(s => s.FinalTotal) })
                        .OrderBy(x => x.Date)
                        .ToListAsync();

                    return Json(salesData);

                case "cashflow":
                    var cashFlowData = await _context.CashMovements
                        .Where(cm => cm.Date >= start && cm.Date <= end)
                        .GroupBy(cm => new { cm.Date.Date, cm.Type })
                        .Select(g => new { 
                            Date = g.Key.Date, 
                            Type = g.Key.Type, 
                            Total = g.Sum(cm => cm.Amount) 
                        })
                        .OrderBy(x => x.Date)
                        .ToListAsync();

                    return Json(cashFlowData);

                case "commissions":
                    var commissionsData = await _context.Payables
                        .Where(p => p.Type == "Commission" && p.CreatedAt >= start && p.CreatedAt <= end)
                        .GroupBy(p => p.Professional.Name)
                        .Select(g => new { Professional = g.Key, Total = g.Sum(p => p.Amount) })
                        .ToListAsync();

                    return Json(commissionsData);

                default:
                    return Json(new { error = "Tipo de gráfico não reconhecido" });
            }
        }

        // Métodos auxiliares para buscar dados
        private async Task<dynamic> GetFinancialSummary(DateTime start, DateTime end)
        {
            var receivables = await _context.Receivables
                .Where(r => r.CreatedAt >= start && r.CreatedAt <= end)
                .ToListAsync();

            var payables = await _context.Payables
                .Where(p => p.CreatedAt >= start && p.CreatedAt <= end)
                .ToListAsync();

            var sales = await _context.Sales
                .Where(s => s.SaleDate >= start && s.SaleDate <= end)
                .ToListAsync();

            var cashMovements = await _context.CashMovements
                .Where(cm => cm.Date >= start && cm.Date <= end)
                .ToListAsync();

            return new
            {
                TotalReceitas = receivables.Sum(r => r.Amount),
                ReceitasPagas = receivables.Where(r => r.IsPaid).Sum(r => r.Amount),
                ReceitasPendentes = receivables.Where(r => !r.IsPaid).Sum(r => r.Amount),
                TotalDespesas = payables.Sum(p => p.Amount),
                DespesasPagas = payables.Where(p => p.IsPaid).Sum(p => p.Amount),
                DespesasPendentes = payables.Where(p => !p.IsPaid).Sum(p => p.Amount),
                TotalVendas = sales.Sum(s => s.FinalTotal),
                QuantidadeVendas = sales.Count,
                TotalEntradas = cashMovements.Where(cm => cm.Type == "Entrada").Sum(cm => cm.Amount),
                TotalSaidas = cashMovements.Where(cm => cm.Type == "Saída").Sum(cm => cm.Amount),
                SaldoLiquido = cashMovements.Where(cm => cm.Type == "Entrada").Sum(cm => cm.Amount) - 
                              cashMovements.Where(cm => cm.Type == "Saída").Sum(cm => cm.Amount),
                TotalComissoes = payables.Where(p => p.Type == "Commission").Sum(p => p.Amount),
                ComissoesPagas = payables.Where(p => p.Type == "Commission" && p.IsPaid).Sum(p => p.Amount),
                ComissoesPendentes = payables.Where(p => p.Type == "Commission" && !p.IsPaid).Sum(p => p.Amount)
            };
        }

        private async Task<IEnumerable<dynamic>> GetSalesData(DateTime start, DateTime end)
        {
            return await _context.Sales
                .Include(s => s.Customer)
                    .ThenInclude(c => c.User)
                .Include(s => s.Professional)
                .Include(s => s.PaymentMethod)
                .Where(s => s.SaleDate >= start && s.SaleDate <= end)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        private async Task<dynamic> GetSalesSummary(DateTime start, DateTime end)
        {
            var sales = await _context.Sales
                .Include(s => s.Items)
                .Where(s => s.SaleDate >= start && s.SaleDate <= end)
                .ToListAsync();

            return new
            {
                TotalSales = sales.Count,
                TotalAmount = sales.Sum(s => s.FinalTotal),
                TotalItems = sales.Sum(s => s.Items.Sum(i => i.Quantity)),
                AverageTicket = sales.Any() ? sales.Average(s => s.FinalTotal) : 0
            };
        }

        private async Task<dynamic> GetCashFlowSummary(DateTime start, DateTime end)
        {
            var receivables = await _context.Receivables
                .Where(r => r.CreatedAt >= start && r.CreatedAt <= end)
                .ToListAsync();

            var payables = await _context.Payables
                .Where(p => p.CreatedAt >= start && p.CreatedAt <= end)
                .ToListAsync();

            var cashMovements = await _context.CashMovements
                .Where(cm => cm.Date >= start && cm.Date <= end)
                .ToListAsync();

            return new
            {
                TotalReceivables = receivables.Sum(r => r.Amount),
                TotalPaidReceivables = receivables.Where(r => r.IsPaid).Sum(r => r.Amount),
                TotalPendingReceivables = receivables.Where(r => !r.IsPaid).Sum(r => r.Amount),
                TotalPayables = payables.Sum(p => p.Amount),
                TotalPaidPayables = payables.Where(p => p.IsPaid).Sum(p => p.Amount),
                TotalPendingPayables = payables.Where(p => !p.IsPaid).Sum(p => p.Amount),
                TotalEntradas = cashMovements.Where(cm => cm.Type == "Entrada").Sum(cm => cm.Amount),
                TotalSaidas = cashMovements.Where(cm => cm.Type == "Saída").Sum(cm => cm.Amount),
                SaldoLiquido = cashMovements.Where(cm => cm.Type == "Entrada").Sum(cm => cm.Amount) - 
                              cashMovements.Where(cm => cm.Type == "Saída").Sum(cm => cm.Amount)
            };
        }

        private async Task<IEnumerable<dynamic>> GetReceivablesData(DateTime start, DateTime end)
        {
            return await _context.Receivables
                .Include(r => r.Customer)
                    .ThenInclude(c => c.User)
                .Include(r => r.Professional)
                .Include(r => r.PaymentMethod)
                .Where(r => r.CreatedAt >= start && r.CreatedAt <= end)
                .ToListAsync();
        }

        private async Task<IEnumerable<dynamic>> GetPayablesData(DateTime start, DateTime end)
        {
            return await _context.Payables
                .Include(p => p.Professional)
                .Include(p => p.Supplier)
                .Include(p => p.PaymentMethod)
                .Where(p => p.CreatedAt >= start && p.CreatedAt <= end)
                .ToListAsync();
        }

        private async Task<IEnumerable<dynamic>> GetCashMovementsData(DateTime start, DateTime end)
        {
            return await _context.CashMovements
                .Include(cm => cm.CashRegister)
                .Where(cm => cm.Date >= start && cm.Date <= end)
                .ToListAsync();
        }

        private async Task<IEnumerable<dynamic>> GetCommissionsData(DateTime start, DateTime end)
        {
            return await _context.Payables
                .Include(p => p.Professional)
                .Include(p => p.Sale)
                .Where(p => p.Type == "Commission" && p.CreatedAt >= start && p.CreatedAt <= end)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        private async Task<dynamic> GetCommissionsSummary(DateTime start, DateTime end)
        {
            var commissions = await _context.Payables
                .Where(p => p.Type == "Commission" && p.CreatedAt >= start && p.CreatedAt <= end)
                .ToListAsync();

            return new
            {
                TotalCommissions = commissions.Sum(p => p.Amount),
                TotalPaidCommissions = commissions.Where(p => p.IsPaid).Sum(p => p.Amount),
                TotalPendingCommissions = commissions.Where(p => !p.IsPaid).Sum(p => p.Amount)
            };
        }
    }
}