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
        private readonly ILogger<PublicClientPanelController> _logger;

        public PublicClientPanelController(SistemaDbContext context, ILogger<PublicClientPanelController> logger)
        {
            _context = context;
            _logger = logger;
        }



        #region Painel

        // GET: Public/PublicClientPanel/Index ou /minhaarea/
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Acessando painel do cliente");
                
                // Limpar o ChangeTracker para forçar atualização
                _context.ChangeTracker.Clear();
                
                // Verificações de segurança
                _logger.LogDebug("Verificando autenticação do usuário. IsAuthenticated: {IsAuthenticated}, IsInRole: {IsInRole}", 
                    User.Identity?.IsAuthenticated, User.IsInRole("Customer"));

                // Verificação dupla de autenticação
                if (User?.Identity == null || !User.Identity.IsAuthenticated)
                {
                    _logger.LogWarning("Usuário não autenticado, redirecionando para login");
                    return RedirectToAction("Login", "PublicAccount", new { area = "Public" });
                }

                if (!User.IsInRole("Customer"))
                {
                    _logger.LogWarning("Usuário sem role Customer, redirecionando para login");
                    return RedirectToAction("Login", "PublicAccount", new { area = "Public" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("UserId não encontrado nas claims");
                    return RedirectToAction("Login", "PublicAccount", new { area = "Public" });
                }

                _logger.LogDebug("Buscando cliente no banco de dados para UserId: {UserId}", userId);
                
                // Busca segura do cliente
                var customer = await _context.Customers
                    .AsNoTracking()
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (customer == null)
                {
                    _logger.LogWarning("Cliente não encontrado para UserId: {UserId}. Tentando criar automaticamente.", userId);
                    
                    // Buscar dados do usuário para criar o cliente
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null)
                    {
                        _logger.LogError("Usuário não encontrado no banco de dados para UserId: {UserId}", userId);
                        TempData["ErrorMessage"] = "Erro de autenticação. Faça login novamente.";
                        return RedirectToAction("Login", "PublicAccount", new { area = "Public" });
                    }

                    // Criar cliente automaticamente
                    customer = new Customer
                    {
                        Name = $"{user.FirstName} {user.LastName}".Trim(),
                        Email = user.Email,
                        Phone = user.PhoneNumber,
                        UserId = userId,
                        IsActive = true,
                        RegistrationDate = DateTime.Now
                    };

                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Cliente criado automaticamente para UserId: {UserId}, CustomerId: {CustomerId}", 
                        userId, customer.CustomerId);
                }

                _logger.LogDebug("Cliente encontrado, verificando conta ativa para CustomerId: {CustomerId}", customer.CustomerId);

                // Verificação de conta ativa
                if (customer.User != null && !customer.User.Active)
                {
                    _logger.LogWarning("Conta inativa para email: {Email}", customer.User.Email);
                    TempData["ErrorMessage"] = "Sua conta está inativa. Entre em contato com o suporte.";
                    return RedirectToAction("Login", "PublicAccount", new { area = "Public" });
                }

                _logger.LogDebug("Carregando dados do painel para CustomerId: {CustomerId}", customer.CustomerId);

                // Sucesso - carregar dados do painel
                ViewBag.CustomerName = $"{customer.User?.FirstName} {customer.User?.LastName}";
                
                _logger.LogDebug("Carregando categorias para filtros");
                
                // Carregar categorias para filtros
                var categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                ViewBag.Categories = categories;

                _logger.LogInformation($"Painel carregado com sucesso para cliente: {customer.CustomerId}, UserId: {customer.UserId}, Email: {customer.User?.Email}");
                
                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar painel do cliente. Mensagem: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Erro interno. Tente novamente.";
                return RedirectToAction("Login", "PublicAccount", new { area = "Public" });
            }
        }

        #endregion


        #region Agendamentos

        // GET: Public/PublicClientPanel/MyAppointments ou /minhaarea/meuscompromissos
        public async Task<IActionResult> MyAppointments(DateTime? startDate, DateTime? endDate, string? status)
        {
            try
            {
                _logger.LogInformation($"Acessando meus agendamentos com filtros - StartDate: {startDate}, EndDate: {endDate}, Status: {status}");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UserId não encontrado, redirecionando para login");
                    return RedirectToAction("Login", "PublicAccount", new { area = "Public" });
                }

                // Limpar o ChangeTracker para forçar atualização
                _context.ChangeTracker.Clear();

                // Buscar o cliente vinculado ao usuário
                var customer = await _context.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                if (customer == null)
                {
                    _logger.LogError("Cliente não encontrado para UserId: {UserId}", userId);
                    return RedirectToAction("Login", "PublicAccount", new { area = "Public" });
                }

                _logger.LogDebug("Buscando agendamentos para CustomerId: {CustomerId}", customer.CustomerId);

                var query = _context.Appointments
                    .AsNoTracking()
                    .Include(a => a.Service)
                        .ThenInclude(s => s.Category)
                    .Include(a => a.Professional)
                    .Include(a => a.Customer)
                    .Where(a => a.CustomerId == customer.CustomerId);

                // Aplicar filtros
                if (startDate.HasValue)
                {
                    query = query.Where(a => a.StartTime.Date >= startDate.Value.Date);
                    _logger.LogDebug("Filtro aplicado - StartDate: {StartDate}", startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(a => a.StartTime.Date <= endDate.Value.Date);
                    _logger.LogDebug("Filtro aplicado - EndDate: {EndDate}", endDate.Value);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(a => a.Status.ToLower() == status.ToLower());
                    _logger.LogDebug("Filtro aplicado - Status: {Status}", status);
                }

                var appointments = await query
                    .OrderByDescending(a => a.StartTime)
                    .ToListAsync();

                _logger.LogInformation($"Encontrados {appointments.Count} agendamentos para CustomerId: {customer.CustomerId}");

                // Verificar se há agendamentos
                if (appointments == null || !appointments.Any())
                {
                    ViewBag.InfoMessage = "Nenhum agendamento encontrado. Faça seu primeiro agendamento agora!";
                }

                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.Status = status;

                return View(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar agendamentos. Mensagem: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Erro ao carregar agendamentos. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Calendário

        // GET: Public/PublicClientPanel/GetCalendarData
        [HttpGet]
        public async Task<IActionResult> GetCalendarData()
        {
            try
            {
                _logger.LogDebug("Solicitando dados do calendário");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Usuário não autenticado ao solicitar dados do calendário");
                    return Json(new { error = "Usuário não autenticado" });
                }

                // Limpar o ChangeTracker para forçar atualização
                _context.ChangeTracker.Clear();

                // Buscar o cliente vinculado ao usuário
                var customer = await _context.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                if (customer == null)
                {
                    _logger.LogError("Cliente não encontrado para UserId: {UserId} ao solicitar dados do calendário", userId);
                    return Json(new { error = "Cliente não encontrado" });
                }

                _logger.LogDebug("Buscando agendamentos para calendário - CustomerId: {CustomerId}", customer.CustomerId);

                var appointments = await _context.Appointments
                    .AsNoTracking()
                    .Include(a => a.Service)
                    .Include(a => a.Professional)
                    .Where(a => a.CustomerId == customer.CustomerId)
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

                _logger.LogInformation($"Retornando {appointments.Count} agendamentos para calendário - CustomerId: {customer.CustomerId}");

                return Json(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados do calendário. Mensagem: {Message}", ex.Message);
                return Json(new { error = "Erro interno ao carregar dados do calendário" });
            }
        }

        // POST: Public/PublicClientPanel/CancelAppointment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            try
            {
                _logger.LogInformation($"Tentativa de cancelamento de agendamento - AppointmentId: {id}");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Usuário não autenticado ao tentar cancelar agendamento");
                    return Json(new { success = false, message = "Usuário não autenticado" });
                }

                // Buscar o cliente vinculado ao usuário
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                if (customer == null)
                {
                    _logger.LogError("Cliente não encontrado para UserId: {UserId} ao tentar cancelar agendamento", userId);
                    return Json(new { success = false, message = "Cliente não encontrado" });
                }

                _logger.LogDebug("Buscando agendamento para cancelamento - AppointmentId: {AppointmentId}, CustomerId: {CustomerId}", 
                    id, customer.CustomerId);

                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.AppointmentId == id && a.CustomerId == customer.CustomerId);

                if (appointment == null)
                {
                    _logger.LogWarning("Agendamento não encontrado - AppointmentId: {AppointmentId}, CustomerId: {CustomerId}", 
                        id, customer.CustomerId);
                    return Json(new { success = false, message = "Agendamento não encontrado" });
                }

                if (appointment.Status.ToLower() == "cancelado")
                {
                    _logger.LogWarning("Tentativa de cancelar agendamento já cancelado - AppointmentId: {AppointmentId}, Status: {Status}", 
                        id, appointment.Status);
                    return Json(new { success = false, message = "Agendamento já está cancelado" });
                }

                _logger.LogInformation($"Cancelando agendamento - AppointmentId: {id}, Status anterior: {appointment.Status}");

                appointment.Status = "Cancelado";
                _context.Update(appointment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Agendamento cancelado com sucesso - AppointmentId: {id}");

                return Json(new { success = true, message = "Agendamento cancelado com sucesso!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao cancelar agendamento - AppointmentId: {id}. Mensagem: {ex.Message}");
                return Json(new { success = false, message = "Erro interno ao cancelar agendamento" });
            }
        }

        #endregion

        #region Google Calendar

        // GET: Public/PublicClientPanel/LinkGoogleCalendar
        public IActionResult LinkGoogleCalendar()
        {
            _logger.LogInformation("Solicitação de integração com Google Calendar");
            
            // Implementar integração OAuth2 com Google Calendar
            // Funcionalidade futura para sincronização de calendários
            // Por enquanto, retorna uma mensagem informativa
            TempData["Info"] = "Integração com Google Calendar será implementada em breve.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Public/PublicClientPanel/ExportToGoogleCalendar
        [HttpGet]
        public async Task<IActionResult> ExportToGoogleCalendar()
        {
            try
            {
                _logger.LogDebug("Solicitando exportação para Google Calendar");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Usuário não autenticado ao solicitar exportação para Google Calendar");
                    return Json(new { success = false, message = "Usuário não autenticado" });
                }

                // Buscar o cliente vinculado ao usuário
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                if (customer == null)
                {
                    _logger.LogError("Cliente não encontrado para UserId: {UserId} ao solicitar exportação para Google Calendar", userId);
                    return Json(new { success = false, message = "Cliente não encontrado" });
                }

                _logger.LogDebug("Buscando agendamentos para exportação - CustomerId: {CustomerId}", customer.CustomerId);

                var appointments = await _context.Appointments
                    .Include(a => a.Service)
                    .Include(a => a.Professional)
                    .Where(a => a.CustomerId == customer.CustomerId)
                    .Select(a => new
                    {
                        title = a.Service.Name,
                        start = a.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                        end = a.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                        description = $"Serviço: {a.Service.Name}\nProfissional: {a.Professional.Name}\nPreço: €{a.Service.Price}",
                        location = "Estabelecimento de Beleza"
                    })
                    .ToListAsync();

                _logger.LogInformation($"Exportação para Google Calendar preparada - {appointments.Count} agendamentos para CustomerId: {customer.CustomerId}");

                return Json(new { success = true, appointments = appointments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao preparar exportação para Google Calendar. Mensagem: {Message}", ex.Message);
                return Json(new { success = false, message = "Erro interno ao preparar exportação" });
            }
        }

        #endregion

        #region Serviços

        // GET: Public/PublicClientPanel/GetServices
        [HttpGet]
        public async Task<IActionResult> GetServices(decimal? minPrice, decimal? maxPrice, int? categoryId)
        {
            try
            {
                _logger.LogDebug("Solicitando serviços com filtros - MinPrice: {MinPrice}, MaxPrice: {MaxPrice}, CategoryId: {CategoryId}", 
                    minPrice, maxPrice, categoryId);

                var query = _context.Services
                    .Include(s => s.Category)
                    .Where(s => s.IsActive);

                if (minPrice.HasValue)
                {
                    query = query.Where(s => s.Price >= minPrice.Value);
                    _logger.LogDebug("Filtro aplicado - MinPrice: {MinPrice}", minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(s => s.Price <= maxPrice.Value);
                    _logger.LogDebug("Filtro aplicado - MaxPrice: {MaxPrice}", maxPrice.Value);
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(s => s.CategoryId == categoryId.Value);
                    _logger.LogDebug("Filtro aplicado - CategoryId: {CategoryId}", categoryId.Value);
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

                _logger.LogInformation($"Retornando {services.Count} serviços com filtros aplicados");

                return Json(services);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar serviços. Mensagem: {Message}", ex.Message);
                return Json(new List<object>());
            }
        }

        #endregion

        #region Categorias

        // GET: Public/PublicClientPanel/GetCategories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                _logger.LogDebug("Carregando categorias");
                
                var categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new
                    {
                        id = c.CategoryId.ToString(), // Convertendo para string
                        name = c.Name
                    })
                    .ToListAsync();

                _logger.LogInformation($"Carregadas {categories.Count} categorias");
                return Json(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar categorias. Mensagem: {Message}", ex.Message);
                return Json(new List<object>());
            }
        }

        #endregion

        #region Exportação PDF

        // GET: Public/PublicClientPanel/ExportHistoryPdf
        [HttpGet]
        public async Task<IActionResult> ExportHistoryPdf()
        {
            try
            {
                _logger.LogInformation("Solicitando exportação de histórico em PDF");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Usuário não autenticado ao solicitar exportação PDF");
                    return RedirectToAction("Login", "PublicAccount", new { area = "Public" });
                }

                // Buscar o cliente vinculado ao usuário
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                if (customer == null)
                {
                    _logger.LogError("Cliente não encontrado para UserId: {UserId} ao solicitar exportação PDF", userId);
                    return NotFound();
                }

                _logger.LogDebug("Gerando PDF para CustomerId: {CustomerId}", customer.CustomerId);

                // Carregar todos os agendamentos do cliente
                var appointments = await _context.Appointments
                    .Include(a => a.Service)
                        .ThenInclude(s => s.Category)
                    .Include(a => a.Professional)
                    .Include(a => a.Customer)
                    .Where(a => a.CustomerId == customer.CustomerId)
                    .OrderByDescending(a => a.StartTime)
                    .ToListAsync();

                _logger.LogDebug("Carregados {Count} agendamentos para PDF - CustomerId: {CustomerId}", 
                    appointments.Count, customer.CustomerId);

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
                
                _logger.LogInformation($"PDF gerado com sucesso - CustomerId: {customer.CustomerId}, FileName: {fileName}");
                
                return File(memoryStream.ToArray(), "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar PDF. Mensagem: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Erro ao gerar PDF. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region Estatísticas

        // GET: Public/PublicClientPanel/Stats
        public async Task<IActionResult> Stats()
        {
            try
            {
                _logger.LogInformation("Acessando página de estatísticas");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Usuário não autenticado ao acessar estatísticas");
                    return RedirectToAction("Login", "PublicAccount", new { area = "Public" });
                }

                // Buscar o cliente vinculado ao usuário
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                if (customer == null)
                {
                    _logger.LogError("Cliente não encontrado para UserId: {UserId} ao acessar estatísticas", userId);
                    return NotFound();
                }

                _logger.LogDebug("Carregando página de estatísticas para CustomerId: {CustomerId}", customer.CustomerId);

                ViewBag.CustomerName = customer.Name;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar página de estatísticas. Mensagem: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Erro ao carregar estatísticas. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Public/PublicClientPanel/GetStatsData
        [HttpGet]
        public async Task<IActionResult> GetStatsData()
        {
            try
            {
                _logger.LogDebug("Solicitando dados de estatísticas");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Usuário não autenticado ao solicitar dados de estatísticas");
                    return Json(new { error = "Usuário não autenticado" });
                }

                // Buscar o cliente vinculado ao usuário
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                if (customer == null)
                {
                    _logger.LogError("Cliente não encontrado para UserId: {UserId} ao solicitar dados de estatísticas", userId);
                    return Json(new { error = "Cliente não encontrado" });
                }

                var clientId = customer.CustomerId;
                _logger.LogDebug("Calculando estatísticas para CustomerId: {CustomerId}", clientId);

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

                // Avaliação média das reviews de serviço (tabela existente no SQLite)
                var serviceReviews = await _context.ServiceReviews
                    .Where(r => r.CustomerId == clientId)
                    .ToListAsync();

                var averageRating = serviceReviews.Any() ? serviceReviews.Average(r => r.Rating) : 0.0;

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

                _logger.LogInformation($"Estatísticas calculadas - TotalAppointments: {totalAppointments}, TotalSpent: {totalSpent}, CustomerId: {clientId}");

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular estatísticas. Mensagem: {Message}", ex.Message);
                return Json(new { error = "Erro interno ao calcular estatísticas" });
            }
        }

        #endregion

        #region Produtos

        // GET: Public/PublicClientPanel/GetProducts
        [HttpGet]
        public async Task<IActionResult> GetProducts(decimal? minPrice, decimal? maxPrice, int? categoryId)
        {
            try
            {
                _logger.LogDebug("Solicitando produtos com filtros - MinPrice: {MinPrice}, MaxPrice: {MaxPrice}, CategoryId: {CategoryId}", 
                    minPrice, maxPrice, categoryId);

                var query = _context.Products
                    .Include(p => p.ProductCategory)
                    .Where(p => p.IsActive && p.Stock > 0);

                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.SalePrice >= minPrice.Value);
                    _logger.LogDebug("Filtro aplicado - MinPrice: {MinPrice}", minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.SalePrice <= maxPrice.Value);
                    _logger.LogDebug("Filtro aplicado - MaxPrice: {MaxPrice}", maxPrice.Value);
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.ProductCategoryId == categoryId.Value);
                    _logger.LogDebug("Filtro aplicado - CategoryId: {CategoryId}", categoryId.Value);
                }

                var products = await query
                    .OrderBy(p => p.ProductCategory.Name)
                    .ThenBy(p => p.Name)
                    .Select(p => new
                    {
                        id = p.ProductId,
                        name = p.Name,
                        description = p.Description,
                        price = p.SalePrice,
                        stock = p.Stock,
                        category = p.ProductCategory.Name,
                        imagePath = p.ImageFullPath
                    })
                    .ToListAsync();

                _logger.LogInformation($"Retornando {products.Count} produtos com filtros aplicados");

                return Json(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar produtos. Mensagem: {Message}", ex.Message);
                return Json(new List<object>());
            }
        }

        // GET: Public/PublicClientPanel/GetProductCategories
        [HttpGet]
        public async Task<IActionResult> GetProductCategories()
        {
            try
            {
                _logger.LogDebug("Carregando categorias de produtos");
                
                var categories = await _context.ProductCategories
                    .OrderBy(c => c.Name)
                    .Select(c => new
                    {
                        id = c.ProductCategoryId.ToString(),
                        name = c.Name
                    })
                    .ToListAsync();

                _logger.LogInformation($"Carregadas {categories.Count} categorias de produtos");
                return Json(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar categorias de produtos. Mensagem: {Message}", ex.Message);
                return Json(new List<object>());
            }
        }

        #endregion

        #region Perfil e Preferências

        // GET: Public/PublicClientPanel/GetProfileData
        [HttpGet]
        public async Task<IActionResult> GetProfileData()
        {
            try
            {
                _logger.LogDebug("Solicitando dados do perfil do usuário");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Usuário não autenticado ao solicitar dados do perfil");
                    return Json(new { success = false, message = "Usuário não autenticado" });
                }

                // Buscar o cliente vinculado ao usuário
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                if (customer == null)
                {
                    _logger.LogError("Cliente não encontrado para UserId: {UserId} ao solicitar dados do perfil", userId);
                    return Json(new { success = false, message = "Cliente não encontrado" });
                }

                var profileData = new
                {
                    firstName = customer.User?.FirstName ?? "",
                    lastName = customer.User?.LastName ?? "",
                    email = customer.User?.Email ?? "",
                    phone = customer.Phone ?? "",
                    birthDate = customer.BirthDate?.ToString("yyyy-MM-dd") ?? "",
                    gender = customer.Gender ?? "",
                    address = customer.Address ?? ""
                };

                _logger.LogInformation("Dados do perfil retornados para CustomerId: {CustomerId}", customer.CustomerId);

                return Json(new { success = true, profile = profileData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados do perfil. Mensagem: {Message}", ex.Message);
                return Json(new { success = false, message = "Erro interno ao carregar dados do perfil" });
            }
        }

        // POST: Public/PublicClientPanel/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile()
        {
            try
            {
                _logger.LogInformation("Atualizando perfil do usuário");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Usuário não autenticado ao tentar atualizar perfil");
                    return Json(new { success = false, message = "Usuário não autenticado" });
                }

                // Buscar o cliente vinculado ao usuário
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                if (customer == null)
                {
                    _logger.LogError("Cliente não encontrado para UserId: {UserId} ao tentar atualizar perfil", userId);
                    return Json(new { success = false, message = "Cliente não encontrado" });
                }

                // Atualizar dados do cliente
                customer.Name = $"{Request.Form["firstName"]} {Request.Form["lastName"]}".Trim();
                customer.Phone = Request.Form["phone"];
                customer.Address = Request.Form["address"];
                
                if (DateTime.TryParse(Request.Form["birthDate"], out DateTime birthDate))
                {
                    customer.BirthDate = birthDate;
                }
                
                customer.Gender = Request.Form["gender"];

                // Atualizar dados do usuário
                if (customer.User != null)
                {
                    customer.User.FirstName = Request.Form["firstName"];
                    customer.User.LastName = Request.Form["lastName"];
                    customer.User.Email = Request.Form["email"];
                    customer.User.PhoneNumber = Request.Form["phone"];
                }

                _context.Update(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Perfil atualizado com sucesso para CustomerId: {CustomerId}", customer.CustomerId);

                return Json(new { success = true, message = "Perfil atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar perfil. Mensagem: {Message}", ex.Message);
                return Json(new { success = false, message = "Erro interno ao atualizar perfil" });
            }
        }

        // POST: Public/PublicClientPanel/UpdatePreferences
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePreferences()
        {
            try
            {
                _logger.LogInformation("Atualizando preferências do usuário");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Usuário não autenticado ao tentar atualizar preferências");
                    return Json(new { success = false, message = "Usuário não autenticado" });
                }

                // Buscar o cliente vinculado ao usuário
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                if (customer == null)
                {
                    _logger.LogError("Cliente não encontrado para UserId: {UserId} ao tentar atualizar preferências", userId);
                    return Json(new { success = false, message = "Cliente não encontrado" });
                }

                // Atualizar preferências (você pode criar uma tabela específica para isso)
                // Por enquanto, vamos salvar em campos do cliente
                customer.PreferredTime = Request.Form["preferredTime"];
                customer.PreferredDay = Request.Form["preferredDay"];

                _context.Update(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Preferências atualizadas com sucesso para CustomerId: {CustomerId}", customer.CustomerId);

                return Json(new { success = true, message = "Preferências atualizadas com sucesso!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar preferências. Mensagem: {Message}", ex.Message);
                return Json(new { success = false, message = "Erro interno ao atualizar preferências" });
            }
        }

        #endregion

        #region Perfil e Configurações

        // GET: Public/PublicClientPanel/Profile
        public async Task<IActionResult> Profile()
        {
            try
            {
                _logger.LogInformation("Acessando página de perfil do cliente");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Usuário não autenticado ao acessar perfil");
                    return RedirectToAction("Login", "PublicAccount", new { area = "Public" });
                }

                // Buscar o cliente vinculado ao usuário
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                if (customer == null)
                {
                    _logger.LogError("Cliente não encontrado para UserId: {UserId} ao acessar perfil", userId);
                    return NotFound();
                }

                _logger.LogDebug("Carregando página de perfil para CustomerId: {CustomerId}", customer.CustomerId);

                ViewBag.CustomerName = customer.Name;
                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar página de perfil. Mensagem: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Erro ao carregar perfil. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Public/PublicClientPanel/UpdateAllergies
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAllergies(string allergyHistory)
        {
            try
            {
                _logger.LogInformation("Atualizando histórico de alergias do cliente");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Usuário não autenticado ao tentar atualizar alergias");
                    return Json(new { success = false, message = "Usuário não autenticado" });
                }

                // Buscar o cliente vinculado ao usuário
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                if (customer == null)
                {
                    _logger.LogError("Cliente não encontrado para UserId: {UserId} ao tentar atualizar alergias", userId);
                    return Json(new { success = false, message = "Cliente não encontrado" });
                }

                // Atualizar histórico de alergias
                customer.AllergyHistory = allergyHistory;
                _context.Update(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Histórico de alergias atualizado com sucesso para CustomerId: {CustomerId}", customer.CustomerId);

                return Json(new { success = true, message = "Histórico de alergias atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar histórico de alergias. Mensagem: {Message}", ex.Message);
                return Json(new { success = false, message = "Erro interno ao atualizar histórico de alergias" });
            }
        }

        // GET: Public/PublicClientPanel/GetProfileStats
        [HttpGet]
        public async Task<IActionResult> GetProfileStats()
        {
            try
            {
                _logger.LogDebug("Solicitando estatísticas do perfil");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Usuário não autenticado ao solicitar estatísticas do perfil");
                    return Json(new { success = false, message = "Usuário não autenticado" });
                }

                // Buscar o cliente vinculado ao usuário
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                if (customer == null)
                {
                    _logger.LogError("Cliente não encontrado para UserId: {UserId} ao solicitar estatísticas do perfil", userId);
                    return Json(new { success = false, message = "Cliente não encontrado" });
                }

                // Calcular estatísticas
                var totalAppointments = await _context.Appointments
                    .CountAsync(a => a.CustomerId == customer.CustomerId);

                var stats = new
                {
                    registrationDate = customer.RegistrationDate.ToString("dd/MM/yyyy"),
                    accountStatus = customer.IsActive ? "Ativa" : "Inativa",
                    totalAppointments = totalAppointments,
                    allergyHistory = customer.AllergyHistory ?? ""
                };

                _logger.LogInformation("Estatísticas do perfil retornadas para CustomerId: {CustomerId}", customer.CustomerId);

                return Json(new { success = true, stats = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar estatísticas do perfil. Mensagem: {Message}", ex.Message);
                return Json(new { success = false, message = "Erro interno ao carregar estatísticas do perfil" });
            }
        }

        #endregion

        private static string GetStatusColor(string status)
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
