using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Helpers;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Recepcionista")]
    public class AdminCashRegisterController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IUserHelper _userHelper;

        public AdminCashRegisterController(SistemaDbContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        /// <summary>
        /// Exibe a lista de caixas com histórico completo
        /// </summary>
        /// <returns>View com lista de caixas ordenados por data</returns>
        public async Task<IActionResult> Index()
        {
            // Carregar caixas com todos os relacionamentos necessários
            var cashRegisters = await _context.CashRegisters
                .Include(cr => cr.UserAbertura)
                .Include(cr => cr.UserFechamento)
                .OrderByDescending(cr => cr.Date)
                .ToListAsync();

            return View(cashRegisters);
        }

        // GET: Admin/CashRegister/Open
        public IActionResult Open()
        {
            return View();
        }

        // POST: Admin/CashRegister/Open
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Open(decimal initialValue)
        {
            // Verificar se já existe caixa aberto
            var openCashRegister = await _context.CashRegisters
                .Where(cr => !cr.IsClosed)
                .FirstOrDefaultAsync();

            if (openCashRegister != null)
            {
                TempData["ErrorMessage"] = "Já existe um caixa aberto. Feche o caixa atual antes de abrir um novo.";
                return RedirectToAction(nameof(Index));
            }

            var cashRegister = new CashRegister
            {
                Date = DateTime.Now,
                InitialValue = initialValue,
                FinalValue = initialValue,
                UserIdAbertura = _userHelper.GetUserId(User),
                IsClosed = false,
                Status = "Open"
            };

            _context.Add(cashRegister);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Caixa aberto com sucesso!";
            return RedirectToAction(nameof(Summary), new { id = cashRegister.CashRegisterId });
        }

        // GET: Admin/CashRegister/Close/5
        public async Task<IActionResult> Close(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cashRegister = await _context.CashRegisters
                .Include(cr => cr.UserAbertura)
                .Include(cr => cr.CashMovements)
                .FirstOrDefaultAsync(cr => cr.CashRegisterId == id);

            if (cashRegister == null)
            {
                return NotFound();
            }

            if (cashRegister.IsClosed)
            {
                TempData["ErrorMessage"] = "Este caixa já está fechado.";
                return RedirectToAction(nameof(Index));
            }

            // Calcular totais
            var totalEntradas = cashRegister.CashMovements
                .Where(cm => cm.Type == "Entrada")
                .Sum(cm => cm.Amount);

            var totalSaidas = cashRegister.CashMovements
                .Where(cm => cm.Type == "Saída")
                .Sum(cm => cm.Amount);

            var saldoEsperado = cashRegister.InitialValue + totalEntradas - totalSaidas;

            ViewBag.TotalEntradas = totalEntradas;
            ViewBag.TotalSaidas = totalSaidas;
            ViewBag.SaldoEsperado = saldoEsperado;

            return View(cashRegister);
        }

        // POST: Admin/CashRegister/Close/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id, decimal finalValue, string? notes)
        {
            var cashRegister = await _context.CashRegisters
                .Include(cr => cr.CashMovements)
                .FirstOrDefaultAsync(cr => cr.CashRegisterId == id);

            if (cashRegister == null)
            {
                return NotFound();
            }

            if (cashRegister.IsClosed)
            {
                TempData["ErrorMessage"] = "Este caixa já está fechado.";
                return RedirectToAction(nameof(Index));
            }

            // Calcular saldo esperado
            var totalEntradas = cashRegister.CashMovements
                .Where(cm => cm.Type == "Entrada")
                .Sum(cm => cm.Amount);

            var totalSaidas = cashRegister.CashMovements
                .Where(cm => cm.Type == "Saída")
                .Sum(cm => cm.Amount);

            var saldoEsperado = cashRegister.InitialValue + totalEntradas - totalSaidas;

            cashRegister.FinalValue = finalValue;
            cashRegister.UserIdFechamento = _userHelper.GetUserId(User);
            cashRegister.IsClosed = true;
            cashRegister.Status = "Closed";
            cashRegister.Notes = notes;

            _context.Update(cashRegister);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Caixa fechado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/CashRegister/Summary/5
        public async Task<IActionResult> Summary(int? id)
        {
            if (id == null)
            {
                // Buscar caixa aberto atual
                var openCashRegister = await _context.CashRegisters
                    .Include(cr => cr.UserAbertura)
                    .Include(cr => cr.CashMovements)
                    .Where(cr => !cr.IsClosed)
                    .OrderByDescending(cr => cr.Date)
                    .FirstOrDefaultAsync();

                if (openCashRegister == null)
                {
                    TempData["ErrorMessage"] = "Não há caixa aberto.";
                    return RedirectToAction(nameof(Index));
                }

                return await Summary(openCashRegister.CashRegisterId);
            }

            var cashRegister = await _context.CashRegisters
                .Include(cr => cr.UserAbertura)
                .Include(cr => cr.UserFechamento)
                .Include(cr => cr.CashMovements)
                .FirstOrDefaultAsync(cr => cr.CashRegisterId == id);

            if (cashRegister == null)
            {
                return NotFound();
            }

            // Calcular totais
            var totalEntradas = cashRegister.CashMovements
                .Where(cm => cm.Type == "Entrada")
                .Sum(cm => cm.Amount);

            var totalSaidas = cashRegister.CashMovements
                .Where(cm => cm.Type == "Saída")
                .Sum(cm => cm.Amount);

            var saldoEsperado = cashRegister.InitialValue + totalEntradas - totalSaidas;
            var diferenca = cashRegister.FinalValue - saldoEsperado;

            ViewBag.TotalEntradas = totalEntradas;
            ViewBag.TotalSaidas = totalSaidas;
            ViewBag.SaldoEsperado = saldoEsperado;
            ViewBag.Diferenca = diferenca;

            return View(cashRegister);
        }

        // GET: Admin/CashRegister/GetOpenRegister
        public async Task<IActionResult> GetOpenRegister()
        {
            var openCashRegister = await _context.CashRegisters
                .Include(cr => cr.UserAbertura)
                .Include(cr => cr.CashMovements)
                .Where(cr => !cr.IsClosed)
                .OrderByDescending(cr => cr.Date)
                .FirstOrDefaultAsync();

            if (openCashRegister == null)
            {
                return Json(new { exists = false });
            }

            var totalEntradas = openCashRegister.CashMovements
                .Where(cm => cm.Type == "Entrada")
                .Sum(cm => cm.Amount);

            var totalSaidas = openCashRegister.CashMovements
                .Where(cm => cm.Type == "Saída")
                .Sum(cm => cm.Amount);

            var saldoAtual = openCashRegister.InitialValue + totalEntradas - totalSaidas;

            return Json(new
            {
                exists = true,
                cashRegisterId = openCashRegister.CashRegisterId,
                date = openCashRegister.Date,
                initialValue = openCashRegister.InitialValue,
                totalEntradas,
                totalSaidas,
                saldoAtual,
                openingUser = openCashRegister.UserAbertura?.FirstName + " " + openCashRegister.UserAbertura?.LastName
            });
        }

        // POST: Admin/CashRegister/FinalizeSale
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeSale([FromBody] SaleViewModel saleData)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Buscar caixa aberto
                var openCashRegister = await _context.CashRegisters
                    .Where(cr => !cr.IsClosed)
                    .OrderByDescending(cr => cr.Date)
                    .FirstOrDefaultAsync();

                if (openCashRegister == null)
                {
                    return Json(new { success = false, message = "Não há caixa aberto." });
                }

                // 2. Criar a venda
                var sale = new Sale
                {
                    CustomerId = saleData.CustomerId,
                    ProfessionalId = saleData.ProfessionalId,
                    PaymentMethodId = saleData.PaymentMethodId,
                    TotalAmount = saleData.TotalAmount,
                    FinalTotal = saleData.FinalTotal,
                    UserId = _userHelper.GetUserId(User),
                    SaleDate = DateTime.Now
                };

                _context.Add(sale);
                await _context.SaveChangesAsync();

                // 3. Criar itens da venda
                foreach (var item in saleData.Items)
                {
                    var saleItem = new SaleItem
                    {
                        SaleId = sale.SaleId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    };
                    _context.Add(saleItem);
                }

                // 4. Criar Receivable
                var receivable = new Receivable
                {
                    Description = $"Venda #{sale.SaleId}",
                    Amount = sale.FinalTotal,
                    CustomerId = sale.CustomerId,
                    ProfessionalId = sale.ProfessionalId,
                    SaleId = sale.SaleId,
                    PaymentMethodId = sale.PaymentMethodId,
                    UserId = _userHelper.GetUserId(User),
                    Status = "Paid", // Venda à vista
                    IsPaid = true,
                    PaymentDate = DateTime.Now
                };

                _context.Add(receivable);

                // 5. Criar movimento de entrada
                var entradaMovement = new CashMovement
                {
                    CashRegisterId = openCashRegister.CashRegisterId,
                    Type = "Entrada",
                    Amount = sale.FinalTotal,
                    Description = $"Venda #{sale.SaleId}",
                    Date = DateTime.Now,
                    ReferenceId = receivable.ReceivableId,
                    ReferenceType = "Receivable"
                };

                _context.Add(entradaMovement);

                // 6. Calcular comissão do profissional
                var professional = await _context.Professionals.FindAsync(sale.ProfessionalId);
                if (professional != null && professional.CommissionPercentage > 0)
                {
                    var commission = sale.FinalTotal * (professional.CommissionPercentage / 100);

                    // 7. Criar Payable (comissão)
                    var payable = new Payable
                    {
                        Description = $"Comissão - Venda #{sale.SaleId}",
                        Amount = commission,
                        DueDate = DateTime.Now.AddDays(7), // Comissão paga em 7 dias
                        Type = "Commission",
                        ProfessionalId = sale.ProfessionalId,
                        SaleId = sale.SaleId,
                        UserId = _userHelper.GetUserId(User),
                        Status = "Pending"
                    };

                    _context.Add(payable);

                    // 8. Criar movimento de saída (comissão)
                    var saidaMovement = new CashMovement
                    {
                        CashRegisterId = openCashRegister.CashRegisterId,
                        Type = "Saída",
                        Amount = commission,
                        Description = $"Comissão - Venda #{sale.SaleId}",
                        Date = DateTime.Now,
                        ReferenceId = payable.PayableId,
                        ReferenceType = "Payable"
                    };

                    _context.Add(saidaMovement);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Venda finalizada com sucesso!", saleId = sale.SaleId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Erro ao finalizar venda: " + ex.Message });
            }
        }

        // =====================================================================
        // MÓDULO PDV (PONTO DE VENDA)
        // =====================================================================

        /// <summary>
        /// Exibe a interface do PDV (Ponto de Venda)
        /// </summary>
        /// <returns>View do PDV</returns>
        [Authorize(Roles = "Admin")]
        public IActionResult PDV()
        {
            return View();
        }

        /// <summary>
        /// Busca produto por código de barras para o PDV
        /// </summary>
        /// <param name="barcode">Código de barras do produto</param>
        /// <returns>Dados do produto em JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetProductByBarcode(string barcode)
        {
            if (string.IsNullOrEmpty(barcode))
                return BadRequest("Código de barras não informado.");

            var product = await _context.Products
                .Where(p => p.ProductId.ToString() == barcode && p.Stock > 0)
                .Select(p => new { 
                    Id = p.ProductId, 
                    Name = p.Name, 
                    Price = p.SalePrice, 
                    StockQuantity = p.Stock,
                    Barcode = p.ProductId.ToString()
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound("Produto não encontrado ou sem estoque.");

            return Json(product);
        }

        /// <summary>
        /// Finaliza uma venda do PDV
        /// </summary>
        /// <param name="sale">Dados da venda</param>
        /// <returns>Resultado da operação</returns>
        [HttpPost]
        public async Task<IActionResult> FinalizeSale([FromBody] PDVSaleViewModel sale)
        {
            if (sale == null || !sale.Items.Any())
                return BadRequest("Nenhum item informado.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Verificar se há caixa aberto
                var openCashRegister = await _context.CashRegisters
                    .Where(cr => !cr.IsClosed)
                    .OrderByDescending(cr => cr.Date)
                    .FirstOrDefaultAsync();

                if (openCashRegister == null)
                {
                    return Json(new { success = false, message = "Não há caixa aberto. Abra um caixa antes de realizar vendas." });
                }

                // 2. Validar estoque dos produtos
                foreach (var item in sale.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        return Json(new { success = false, message = $"Produto ID {item.ProductId} não encontrado." });
                    }

                    if (product.Stock < item.Quantity)
                    {
                        return Json(new { success = false, message = $"Estoque insuficiente para o produto {product.Name}. Disponível: {product.Stock}" });
                    }
                }

                // 3. Criar a venda
                var saleEntity = new Sale
                {
                    CustomerId = sale.CustomerId ?? 1, // Cliente padrão se não especificado
                    ProfessionalId = sale.ProfessionalId ?? 1, // Profissional padrão se não especificado
                    PaymentMethodId = sale.PaymentMethodId,
                    TotalAmount = sale.Total,
                    FinalTotal = sale.Total,
                    UserId = _userHelper.GetUserId(User),
                    SaleDate = DateTime.Now
                };

                _context.Add(saleEntity);
                await _context.SaveChangesAsync();

                // 4. Criar itens da venda e atualizar estoque
                foreach (var item in sale.Items)
                {
                    var saleItem = new SaleItem
                    {
                        SaleId = saleEntity.SaleId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    };
                    _context.Add(saleItem);

                    // Atualizar estoque
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock -= item.Quantity;
                        _context.Update(product);
                    }
                }

                // 5. Criar Receivable
                var receivable = new Receivable
                {
                    Description = $"Venda PDV #{saleEntity.SaleId}",
                    Amount = saleEntity.FinalTotal,
                    CustomerId = sale.CustomerId ?? 1,
                    ProfessionalId = sale.ProfessionalId ?? 1,
                    SaleId = saleEntity.SaleId,
                    PaymentMethodId = sale.PaymentMethodId,
                    UserId = _userHelper.GetUserId(User),
                    Status = "Paid",
                    IsPaid = true,
                    PaymentDate = DateTime.Now
                };

                _context.Add(receivable);

                // 6. Criar movimento de entrada no caixa
                var cashMovement = new CashMovement
                {
                    CashRegisterId = openCashRegister.CashRegisterId,
                    Type = "Entrada",
                    Amount = saleEntity.FinalTotal,
                    Description = $"Venda PDV #{saleEntity.SaleId}",
                    Date = DateTime.Now,
                    ReferenceId = receivable.ReceivableId,
                    ReferenceType = "Receivable"
                };

                _context.Add(cashMovement);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = "Venda finalizada com sucesso!", 
                    saleId = saleEntity.SaleId,
                    total = saleEntity.FinalTotal
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Erro ao finalizar venda: " + ex.Message });
            }
        }

        private bool CashRegisterExists(int id)
        {
            return _context.CashRegisters.Any(e => e.CashRegisterId == id);
        }
    }

    // ViewModel para finalização de vendas
    public class SaleViewModel
    {
        public int CustomerId { get; set; }
        public int ProfessionalId { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal FinalTotal { get; set; }
        public List<SaleItemViewModel> Items { get; set; } = new();
    }

    public class SaleItemViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    // ViewModel específico para PDV
    public class PDVSaleViewModel
    {
        public decimal Total { get; set; }
        public int? CustomerId { get; set; }
        public int? ProfessionalId { get; set; }
        public int PaymentMethodId { get; set; }
        public List<PDVSaleItemViewModel> Items { get; set; } = new();
    }

    public class PDVSaleItemViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}