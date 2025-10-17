using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using System.Security.Claims;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    [Authorize(Roles = "Customer")]
    public class PublicClientPanelController : Controller
    {
        private readonly SistemaDbContext _context;

        public PublicClientPanelController(SistemaDbContext context)
        {
            _context = context;
        }

        // GET: Public/PublicClientPanel/Index ou /minhaarea/
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // Buscar dados do cliente
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == int.Parse(userId));

            if (customer == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // Buscar categorias para filtros
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.CustomerName = customer.Name;

            return View();
        }

        // GET: Public/PublicClientPanel/MyAppointments ou /minhaarea/meuscompromissos
        public async Task<IActionResult> MyAppointments(DateTime? startDate, DateTime? endDate, string? status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var query = _context.Appointments
                .Include(a => a.Service)
                    .ThenInclude(s => s.Category)
                .Include(a => a.Professional)
                .Include(a => a.Customer)
                .Where(a => a.CustomerId == int.Parse(userId));

            // Aplicar filtros
            if (startDate.HasValue)
            {
                query = query.Where(a => a.StartTime.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.StartTime.Date <= endDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status.ToLower() == status.ToLower());
            }

            var appointments = await query
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.Status = status;

            return View(appointments);
        }

        // GET: Public/PublicClientPanel/GetCalendarData
        [HttpGet]
        public async Task<IActionResult> GetCalendarData()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { error = "Usuário não autenticado" });
            }

            var appointments = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Professional)
                .Where(a => a.CustomerId == int.Parse(userId))
                .Select(a => new
                {
                    id = a.AppointmentId,
                    title = a.Service.Name,
                    start = a.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = a.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    color = GetStatusColor(a.Status),
                    extendedProps = new
                    {
                        professional = a.Professional != null ? a.Professional.Name : "Não definido",
                        price = a.Service.Price,
                        status = a.Status,
                        duration = a.Service.Duration
                    }
                })
                .ToListAsync();

            return Json(appointments);
        }

        // POST: Public/PublicClientPanel/CancelAppointment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Usuário não autenticado" });
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.CustomerId == int.Parse(userId));

            if (appointment == null)
            {
                return Json(new { success = false, message = "Agendamento não encontrado" });
            }

            if (appointment.Status.ToLower() == "cancelado")
            {
                return Json(new { success = false, message = "Agendamento já está cancelado" });
            }

            appointment.Status = "Cancelado";
            _context.Update(appointment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Agendamento cancelado com sucesso!" });
        }

        // GET: Public/PublicClientPanel/LinkGoogleCalendar
        public IActionResult LinkGoogleCalendar()
        {
            // TODO: Implementar integração OAuth2 com Google Calendar
            // Por enquanto, retorna uma mensagem informativa
            TempData["Info"] = "Integração com Google Calendar será implementada em breve.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Public/PublicClientPanel/ExportToGoogleCalendar
        [HttpGet]
        public async Task<IActionResult> ExportToGoogleCalendar()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Usuário não autenticado" });
            }

            var appointments = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Professional)
                .Where(a => a.CustomerId == int.Parse(userId))
                .Select(a => new
                {
                    title = a.Service.Name,
                    start = a.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = a.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    description = $"Serviço: {a.Service.Name}\nProfissional: {a.Professional.Name}\nPreço: €{a.Service.Price}",
                    location = "Estabelecimento de Beleza"
                })
                .ToListAsync();

            return Json(new { success = true, appointments = appointments });
        }

        // GET: Public/PublicClientPanel/GetServices
        [HttpGet]
        public async Task<IActionResult> GetServices(decimal? minPrice, decimal? maxPrice, int? categoryId)
        {
            var query = _context.Services
                .Include(s => s.Category)
                .Where(s => s.IsActive);

            if (minPrice.HasValue)
            {
                query = query.Where(s => s.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(s => s.Price <= maxPrice.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(s => s.CategoryId == categoryId.Value);
            }

            var services = await query
                .OrderBy(s => s.Category.Name)
                .ThenBy(s => s.Name)
                .Select(s => new
                {
                    id = s.ServiceId,
                    name = s.Name,
                    description = s.Description,
                    price = s.Price,
                    duration = s.Duration,
                    category = s.Category.Name,
                    imagePath = s.ImageFullPath
                })
                .ToListAsync();

            return Json(services);
        }

        // GET: Public/PublicClientPanel/GetCategories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    id = c.CategoryId,
                    name = c.Name
                })
                .ToListAsync();

            return Json(categories);
        }

        // GET: Public/PublicClientPanel/ExportHistoryPdf
        [HttpGet]
        public async Task<IActionResult> ExportHistoryPdf()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var clientId = int.Parse(userId);
            var customer = await _context.Customers.FindAsync(clientId);
            if (customer == null)
            {
                return NotFound();
            }

            // Carregar todos os agendamentos do cliente
            var appointments = await _context.Appointments
                .Include(a => a.Service)
                    .ThenInclude(s => s.Category)
                .Include(a => a.Professional)
                .Include(a => a.Customer)
                .Where(a => a.CustomerId == clientId)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();

            // Gerar PDF
            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Configurar fontes
            var headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var smallFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Cores da paleta Ewellin Jordão
            var primaryColor = new DeviceRgb(201, 125, 93); // --accent-terracotta
            var secondaryColor = new DeviceRgb(138, 145, 109); // --secondary-olive
            var lightColor = new DeviceRgb(230, 198, 182); // --primary-beige

            // Cabeçalho
            var header = new Paragraph("Relatório de Agendamentos – Ewellin Jordão")
                .SetFont(headerFont)
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(primaryColor)
                .SetMarginBottom(20);

            document.Add(header);

            // Informações do cliente
            var clientInfo = new Paragraph($"Cliente: {customer.Name}")
                .SetFont(normalFont)
                .SetFontSize(12)
                .SetMarginBottom(10);

            document.Add(clientInfo);

            var dateInfo = new Paragraph($"Data de geração: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFont(smallFont)
                .SetFontSize(10)
                .SetMarginBottom(20);

            document.Add(dateInfo);

            // Tabela de agendamentos
            if (appointments.Any())
            {
                var table = new Table(5).UseAllAvailableWidth();

                // Cabeçalho da tabela
                var headers = new[] { "Data", "Serviço", "Profissional", "Valor", "Status" };
                foreach (var headerText in headers)
                {
                    var cell = new Cell()
                        .Add(new Paragraph(headerText)
                            .SetFont(headerFont)
                            .SetFontSize(10))
                        .SetBackgroundColor(lightColor)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetPadding(8);
                    table.AddCell(cell);
                }

                // Dados dos agendamentos
                foreach (var appointment in appointments)
                {
                    var culture = new System.Globalization.CultureInfo("pt-PT");
                    
                    table.AddCell(new Cell()
                        .Add(new Paragraph(appointment.StartTime.ToString("dd/MM/yyyy HH:mm", culture))
                            .SetFont(normalFont)
                            .SetFontSize(9))
                        .SetPadding(6));

                    table.AddCell(new Cell()
                        .Add(new Paragraph(appointment.Service?.Name ?? "N/A")
                            .SetFont(normalFont)
                            .SetFontSize(9))
                        .SetPadding(6));

                    table.AddCell(new Cell()
                        .Add(new Paragraph(appointment.Professional?.Name ?? "N/A")
                            .SetFont(normalFont)
                            .SetFontSize(9))
                        .SetPadding(6));

                    table.AddCell(new Cell()
                        .Add(new Paragraph(appointment.Service?.Price.ToString("C2", culture) ?? "N/A")
                            .SetFont(normalFont)
                            .SetFontSize(9))
                        .SetPadding(6));

                    var statusCell = new Cell()
                        .Add(new Paragraph(appointment.Status ?? "N/A")
                            .SetFont(normalFont)
                            .SetFontSize(9))
                        .SetPadding(6);

                    // Colorir status
                    switch (appointment.Status?.ToLower())
                    {
                        case "concluído":
                        case "completed":
                            statusCell.SetFontColor(new DeviceRgb(23, 162, 184));
                            break;
                        case "cancelado":
                        case "canceled":
                            statusCell.SetFontColor(new DeviceRgb(220, 53, 69));
                            break;
                        case "confirmado":
                        case "confirmed":
                            statusCell.SetFontColor(new DeviceRgb(40, 167, 69));
                            break;
                        default:
                            statusCell.SetFontColor(new DeviceRgb(108, 117, 125));
                            break;
                    }

                    table.AddCell(statusCell);
                }

                document.Add(table);

                // Resumo
                var totalAppointments = appointments.Count;
                var totalSpent = appointments.Where(a => a.Service != null).Sum(a => a.Service.Price);
                var completedAppointments = appointments.Count(a => a.Status?.ToLower() == "concluído" || a.Status?.ToLower() == "completed");

                document.Add(new Paragraph("\n"));
                
                var summary = new Paragraph("Resumo:")
                    .SetFont(headerFont)
                    .SetFontSize(12)
                    .SetFontColor(secondaryColor)
                    .SetMarginTop(20);

                document.Add(summary);

                var summaryText = $"Total de agendamentos: {totalAppointments}\n" +
                                 $"Agendamentos concluídos: {completedAppointments}\n" +
                                 $"Valor total gasto: {totalSpent.ToString("C2", new System.Globalization.CultureInfo("pt-PT"))}";

                document.Add(new Paragraph(summaryText)
                    .SetFont(normalFont)
                    .SetFontSize(10)
                    .SetMarginTop(10));
            }
            else
            {
                document.Add(new Paragraph("Nenhum agendamento encontrado.")
                    .SetFont(normalFont)
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(50));
            }

            // Rodapé
            document.Add(new Paragraph($"\n\nRelatório gerado em {DateTime.Now:dd/MM/yyyy HH:mm} para {customer.Name}")
                .SetFont(smallFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(secondaryColor));

            document.Close();

            var fileName = $"Historico_Agendamentos_{customer.Name.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(memoryStream.ToArray(), "application/pdf", fileName);
        }

        // GET: Public/PublicClientPanel/Stats
        public async Task<IActionResult> Stats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var clientId = int.Parse(userId);
            var customer = await _context.Customers.FindAsync(clientId);
            if (customer == null)
            {
                return NotFound();
            }

            ViewBag.CustomerName = customer.Name;
            return View();
        }

        // GET: Public/PublicClientPanel/GetStatsData
        [HttpGet]
        public async Task<IActionResult> GetStatsData()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { error = "Usuário não autenticado" });
            }

            var clientId = int.Parse(userId);

            // Estatísticas básicas
            var appointments = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Professional)
                .Where(a => a.CustomerId == clientId)
                .ToListAsync();

            var totalAppointments = appointments.Count;
            var totalSpent = appointments.Where(a => a.Service != null).Sum(a => a.Service.Price);
            var completedAppointments = appointments.Count(a => a.Status?.ToLower() == "concluído" || a.Status?.ToLower() == "completed");

            // Calcular média de frequência
            var appointmentDates = appointments
                .Where(a => a.StartTime.Date <= DateTime.Now.Date)
                .OrderBy(a => a.StartTime)
                .Select(a => a.StartTime.Date)
                .Distinct()
                .ToList();

            double averageFrequency = 0;
            if (appointmentDates.Count > 1)
            {
                var totalDays = (appointmentDates.Last() - appointmentDates.First()).TotalDays;
                averageFrequency = totalDays / (appointmentDates.Count - 1);
            }

            // Avaliação média dos feedbacks
            var feedbacks = await _context.Feedbacks
                .Where(f => f.ClientId == clientId)
                .ToListAsync();

            var averageRating = feedbacks.Any() ? feedbacks.Average(f => f.Rating) : 0.0;

            // Serviços mais utilizados
            var topServices = appointments
                .Where(a => a.Service != null)
                .GroupBy(a => a.Service.Name)
                .Select(g => new { name = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .Take(5)
                .ToList();

            // Profissionais mais escolhidos
            var topProfessionals = appointments
                .Where(a => a.Professional != null)
                .GroupBy(a => a.Professional.Name)
                .Select(g => new { name = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .Take(5)
                .ToList();

            return Json(new
            {
                totalAppointments,
                totalSpent,
                completedAppointments,
                averageFrequency = Math.Round(averageFrequency, 1),
                averageRating,
                topServices,
                topProfessionals
            });
        }

        private string GetStatusColor(string status)
        {
            return status?.ToLower() switch
            {
                "agendado" or "scheduled" => "#007bff", // Azul
                "confirmado" or "confirmed" => "#28a745", // Verde
                "cancelado" or "canceled" => "#dc3545", // Vermelho
                "concluído" or "completed" => "#17a2b8", // Azul claro
                _ => "#6c757d" // Cinza
            };
        }
    }
}
