using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Services;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminReportsController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IExcelExportService _excelExportService;

        public AdminReportsController(SistemaDbContext context, IExcelExportService excelExportService)
        {
            _context = context;
            _excelExportService = excelExportService;
        }

        // GET: Dashboard de Relatórios
        public IActionResult Index()
        {
            ViewData["Title"] = "Relatórios";
            return View();
        }

        // GET: Relatório de Vendas
        public async Task<IActionResult> Sales(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            ViewData["Title"] = "Relatório de Vendas";
            ViewBag.StartDate = start;
            ViewBag.EndDate = end;

            // Faturação do período
            var billing = await _context.Billings
                .Include(b => b.User)
                .Where(b => b.Date >= start && b.Date <= end)
                .OrderBy(b => b.Date)
                .ToListAsync();

            // Detalhes da faturação
            var billingDetails = await _context.BillingDetails
                .Include(bd => bd.Billing)
                .Include(bd => bd.Product)
                .Include(bd => bd.Service)
                .Where(bd => bd.Billing.Date >= start && bd.Billing.Date <= end)
                .ToListAsync();

            // Estatísticas
            ViewBag.TotalBilling = billing.Sum(b => b.TotalValue);
            ViewBag.TotalServices = billing.Sum(b => b.ServicesQuantity);
            ViewBag.TotalProducts = billing.Sum(b => b.ProductsQuantity);
            ViewBag.TotalTransactions = billing.Count;

            // Vendas por dia
            var salesByDay = billing
                .GroupBy(b => b.Date.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(b => b.TotalValue),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            ViewBag.SalesByDay = salesByDay;

            // Vendas por tipo
            var salesByType = billingDetails
                .GroupBy(bd => bd.ItemType)
                .Select(g => new
                {
                    Type = g.Key,
                    Total = g.Sum(bd => bd.TotalValue),
                    Count = g.Sum(bd => bd.Quantity)
                })
                .ToList();

            ViewBag.SalesByType = salesByType;

            return View(billing);
        }

        // GET: Relatório de Agendamentos
        public async Task<IActionResult> Appointments(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            ViewData["Title"] = "Relatório de Agendamentos";
            ViewBag.StartDate = start;
            ViewBag.EndDate = end;

            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Professional)
                .Include(a => a.Service)
                .Where(a => a.Date >= start && a.Date <= end)
                .OrderBy(a => a.Date)
                .ToListAsync();

            // Estatísticas
            ViewBag.TotalAppointments = appointments.Count;
            ViewBag.ConfirmedAppointments = appointments.Count(a => a.Status == "Confirmado");
            ViewBag.PendingAppointments = appointments.Count(a => a.Status == "Pendente");
            ViewBag.CancelledAppointments = appointments.Count(a => a.Status == "Cancelado");
            ViewBag.CompletedAppointments = appointments.Count(a => a.Status == "Concluído");

            // Agendamentos por dia
            var appointmentsByDay = appointments
                .GroupBy(a => a.StartTime.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Confirmed = g.Count(a => a.Status == "Confirmado"),
                    Pending = g.Count(a => a.Status == "Pendente"),
                    Cancelled = g.Count(a => a.Status == "Cancelado")
                })
                .OrderBy(x => x.Date)
                .ToList();

            ViewBag.AppointmentsByDay = appointmentsByDay;

            // Agendamentos por profissional
            var appointmentsByProfessional = appointments
                .GroupBy(a => a.Professional.Name)
                .Select(g => new
                {
                    Professional = g.Key,
                    Count = g.Count(),
                    Confirmed = g.Count(a => a.Status == "Confirmado")
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            ViewBag.AppointmentsByProfessional = appointmentsByProfessional;

            return View(appointments);
        }

        // GET: Relatório Financeiro
        public async Task<IActionResult> Financial(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            ViewData["Title"] = "Relatório Financeiro";
            ViewBag.StartDate = start;
            ViewBag.EndDate = end;

            // Recebimentos
            var receivables = await _context.Receivables
                .Include(r => r.PaymentMethod)
                .Where(r => r.LaunchDate >= start && r.LaunchDate <= end)
                .ToListAsync();

            // Pagamentos
            var payments = await _context.Payables
                .Include(p => p.PaymentMethod)
                .Where(p => p.LaunchDate >= start && p.LaunchDate <= end)
                .ToListAsync();

            // Estatísticas
            ViewBag.TotalReceivables = receivables.Sum(r => r.Value);
            ViewBag.TotalPaid = receivables.Where(r => r.IsPaid).Sum(r => r.Value);
            ViewBag.TotalPending = receivables.Where(r => !r.IsPaid).Sum(r => r.Value);

            ViewBag.TotalPayments = payments.Sum(p => p.Value);
            ViewBag.TotalPaidOut = payments.Where(p => p.IsPaid).Sum(p => p.Value);
            ViewBag.TotalPendingPayments = payments.Where(p => !p.IsPaid).Sum(p => p.Value);

            // Saldo
            ViewBag.Balance = ViewBag.TotalPaid - ViewBag.TotalPaidOut;

            // Recebimentos por forma de pagamento
            var receivablesByPaymentMethod = receivables
                .Where(r => r.IsPaid)
                .GroupBy(r => r.PaymentMethod.Description)
                .Select(g => new
                {
                    PaymentMethod = g.Key,
                    Total = g.Sum(r => r.Value),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            ViewBag.ReceivablesByPaymentMethod = receivablesByPaymentMethod;

            // Pagamentos por forma de pagamento
            var paymentsByPaymentMethod = payments
                .Where(p => p.IsPaid)
                .GroupBy(p => p.PaymentMethod.Description)
                .Select(g => new
                {
                    PaymentMethod = g.Key,
                    Total = g.Sum(p => p.Value),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            ViewBag.PaymentsByPaymentMethod = paymentsByPaymentMethod;

            return View(new { Receivables = receivables, Payments = payments });
        }

        // GET: Relatório de Clientes
        public async Task<IActionResult> Clients()
        {
            ViewData["Title"] = "Relatório de Clientes";

            var clients = await _context.Customers
                .OrderBy(c => c.Name)
                .ToListAsync();

            // Estatísticas
            ViewBag.TotalClients = clients.Count;
            ViewBag.ActiveClients = clients.Count(c => c.IsActive);
            ViewBag.NewThisMonth = clients.Count(c => c.RegistrationDate >= DateTime.Today.AddDays(-30));
            ViewBag.NewThisWeek = clients.Count(c => c.RegistrationDate >= DateTime.Today.AddDays(-7));

            // Clientes por mês de cadastro
            var clientsByMonth = clients
                .GroupBy(c => new { c.RegistrationDate.Year, c.RegistrationDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            ViewBag.ClientsByMonth = clientsByMonth;

            return View(clients);
        }

        // GET: Relatório de Produtos
        public async Task<IActionResult> Products()
        {
            ViewData["Title"] = "Relatório de Produtos";

            var products = await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.Supplier)
                .OrderBy(p => p.Name)
                .ToListAsync();

            // Estatísticas
            ViewBag.TotalProducts = products.Count;
            ViewBag.ActiveProducts = products.Count;
            ViewBag.OutOfStock = products.Count(p => p.Stock <= 0);

            // Produtos por categoria
            var productsByCategory = products
                .GroupBy(p => p.ProductCategory.Name)
                .Select(g => new
                {
                    Category = g.Key,
                    Count = g.Count(),
                    TotalValue = g.Sum(p => p.SalePrice * p.Stock)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            ViewBag.ProductsByCategory = productsByCategory;

            return View(products);
        }

        // GET: Balanço Mensal de Caixa
        public async Task<IActionResult> MonthlyTrialBalance(int? year, int? month)
        {
            var targetYear = year ?? DateTime.Now.Year;
            var targetMonth = month ?? DateTime.Now.Month;
            
            var startDate = new DateTime(targetYear, targetMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            ViewData["Title"] = "Monthly Trial Balance";
            ViewBag.Year = targetYear;
            ViewBag.Month = targetMonth;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            // Get all cash registers for the month
            var cashRegisters = await _context.CashRegisters
                .Include(c => c.OpeningUser)
                .Include(c => c.ClosingUser)
                .Where(c => c.Date >= startDate && c.Date <= endDate)
                .OrderBy(c => c.Date)
                .ToListAsync();

            // Get all movements for the month
            var movements = await _context.CashMovements
                .Include(m => m.CashRegister)
                .Where(m => m.Date >= startDate && m.Date <= endDate)
                .OrderBy(m => m.Date)
                .ToListAsync();

            // Calculate monthly totals
            var totalInitialAmount = cashRegisters.Sum(c => c.InitialValue);
            var totalFinalAmount = cashRegisters.Sum(c => c.FinalValue);
            var totalEntries = movements.Where(m => m.Type == "entry").Sum(m => m.Amount);
            var totalExits = movements.Where(m => m.Type == "exit").Sum(m => m.Amount);
            var totalExpected = totalInitialAmount + totalEntries - totalExits;
            var totalDifference = totalFinalAmount - totalExpected;

            // Group by day
            var dailySummaries = cashRegisters.Select(cr => new
            {
                Date = cr.Date,
                InitialAmount = cr.InitialValue,
                FinalAmount = cr.FinalValue,
                DayMovements = movements.Where(m => m.CashRegisterId == cr.CashRegisterId).ToList(),
                DayEntries = movements.Where(m => m.CashRegisterId == cr.CashRegisterId && m.Type == "entry").Sum(m => m.Amount),
                DayExits = movements.Where(m => m.CashRegisterId == cr.CashRegisterId && m.Type == "exit").Sum(m => m.Amount)
            }).OrderBy(x => x.Date).ToList();

            ViewBag.CashRegisters = cashRegisters;
            ViewBag.Movements = movements;
            ViewBag.DailySummaries = dailySummaries;
            ViewBag.TotalInitialAmount = totalInitialAmount;
            ViewBag.TotalFinalAmount = totalFinalAmount;
            ViewBag.TotalEntries = totalEntries;
            ViewBag.TotalExits = totalExits;
            ViewBag.TotalExpected = totalExpected;
            ViewBag.TotalDifference = totalDifference;

            return View();
        }

        // GET: Exportar Relatório
        public async Task<IActionResult> Export(string reportType, DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            byte[] excelData;
            string fileName;

            switch (reportType.ToLower())
            {
                case "sales":
                    var sales = await _context.Billings
                        .Include(b => b.User)
                        .Where(b => b.Date >= start && b.Date <= end)
                        .ToListAsync();
                    excelData = _excelExportService.ExportSalesToExcel(sales);
                    fileName = $"Vendas_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx";
                    break;

                case "appointments":
                    var appointments = await _context.Appointments
                        .Include(a => a.Customer)
                        .Include(a => a.Professional)
                        .Include(a => a.Service)
                        .Where(a => a.Date >= start && a.Date <= end)
                        .ToListAsync();
                    excelData = _excelExportService.ExportAppointmentsToExcel(appointments);
                    fileName = $"Agendamentos_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx";
                    break;

                case "clients":
                    var clients = await _context.Customers.ToListAsync();
                    excelData = _excelExportService.ExportClientsToExcel(clients);
                    fileName = $"Clientes_{DateTime.Now:yyyyMMdd}.xlsx";
                    break;

                case "cash":
                    var cashRegisters = await _context.CashRegisters
                        .Include(c => c.OpeningUser)
                        .Include(c => c.ClosingUser)
                        .Where(c => c.Date >= start && c.Date <= end)
                        .ToListAsync();
                    excelData = _excelExportService.ExportCashRegisterToExcel(cashRegisters);
                    fileName = $"Caixa_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx";
                    break;

                default:
                    TempData["ErrorMessage"] = "Tipo de relatório inválido.";
                    return RedirectToAction(nameof(Index));
            }

            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
