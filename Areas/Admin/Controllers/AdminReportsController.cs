using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Helpers;
using Sistema.Services;

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
            return View();
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