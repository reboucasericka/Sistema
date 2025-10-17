using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Models.Admin;
using Sistema.Services;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminCashRegisterController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly ICommunicationService _communicationService;

        public AdminCashRegisterController(SistemaDbContext context, ICommunicationService communicationService)
        {
            _context = context;
            _communicationService = communicationService;
        }

        // Página principal do Caixa
        public async Task<IActionResult> Index()
        {
            var hoje = DateTime.Today;
            var viewModel = new CashRegisterViewModel
            {
                CurrentBalance = await GetCurrentBalance(),
                IsOpen = await IsCashRegisterOpen(),
                RecentMovements = await GetRecentMovements(),
                Products = await GetProductsForSale(),
                TotalEntradasHoje = await GetTotalEntradasHoje(hoje),
                TotalSaidasHoje = await GetTotalSaidasHoje(hoje),
                SaldoAtual = await GetCurrentBalance()
            };
            return View(viewModel);
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

            _context.CashRegisters.Add(cashRegister);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Caixa aberto com sucesso!" });
        }

        // Fechar caixa
        [HttpPost]
        public async Task<IActionResult> CloseCashRegister()
        {
            var cashRegister = await _context.CashRegisters
                .Where(cr => !cr.IsClosed && cr.Status == "Open")
                .FirstOrDefaultAsync();

            if (cashRegister == null)
                return BadRequest("Nenhum caixa aberto encontrado.");

            // Calcular totais do dia
            var hoje = DateTime.Today;
            var entradas = await GetTotalEntradasHoje(hoje);
            var saidas = await GetTotalSaidasHoje(hoje);
            var saldo = entradas - saidas;

            cashRegister.IsClosed = true;
            cashRegister.Status = "Closed";
            cashRegister.UserIdFechamento = User.Identity?.Name ?? "System";

            _context.Update(cashRegister);
            await _context.SaveChangesAsync();

            // Enviar notificações SMS e WhatsApp
            // TODO: Implementar SendCashRegisterCloseNotificationAsync no ICommunicationService
            // await _communicationService.SendCashRegisterCloseNotificationAsync(entradas, saidas, saldo);

            return Ok(new { success = true, message = "Caixa fechado com sucesso!" });
        }

        // Buscar produto por código de barras
        [HttpGet]
        public async Task<IActionResult> GetProductByBarcode(string barcode)
        {
            if (string.IsNullOrEmpty(barcode))
                return BadRequest("Código inválido");

            var product = await _context.Products
                .Where(p => p.ProductId.ToString() == barcode || p.Name.Contains(barcode))
                .Select(p => new { Id = p.ProductId, Name = p.Name, Price = p.SalePrice, Stock = p.Stock })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound();

            return Json(product);
        }

        // API para obter clientes
        [HttpGet]
        public async Task<IActionResult> GetClients()
        {
            var clients = await _context.Customers
                .Where(c => c.IsActive)
                .Select(c => new { CustomerId = c.CustomerId, Name = c.Name })
                .ToListAsync();
            return Json(clients);
        }

        // API para obter profissionais
        [HttpGet]
        public async Task<IActionResult> GetProfessionals()
        {
            var professionals = await _context.Professionals
                .Where(p => p.IsActive)
                .Select(p => new { 
                    ProfessionalId = p.ProfessionalId, 
                    Name = p.Name,
                    CommissionPercentage = p.CommissionPercentage
                })
                .ToListAsync();
            return Json(professionals);
        }

        // API para obter métodos de pagamento
        [HttpGet]
        public async Task<IActionResult> GetPaymentMethods()
        {
            var methods = await _context.PaymentMethods
                .Where(pm => pm.IsActive)
                .Select(pm => new { PaymentMethodId = pm.PaymentMethodId, Name = pm.Name })
                .ToListAsync();
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

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Registrar movimentação de caixa
                var cashRegister = await _context.CashRegisters
                    .Where(cr => !cr.IsClosed && cr.Status == "Open")
                    .FirstOrDefaultAsync();

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
                    _context.CashMovements.Add(cashMovement);
                }

                // Atualizar estoque dos produtos
                foreach (var item in data.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        if (product.Stock < item.Quantity)
                            return BadRequest($"Estoque insuficiente para o produto {product.Name}");

                        product.Stock -= item.Quantity;
                        _context.Update(product);
                    }
                }

                // Atualizar saldo do caixa
                if (cashRegister != null)
                {
                    cashRegister.FinalValue += data.Total;
                    _context.Update(cashRegister);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

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
                await transaction.RollbackAsync();
                return BadRequest($"Erro ao processar venda: {ex.Message}");
            }
        }

        // Registrar movimentação manual
        [HttpPost]
        public async Task<IActionResult> AddCashMovement([FromBody] CashMovementViewModel movement)
        {
            if (!await IsCashRegisterOpen())
                return BadRequest("Caixa não está aberto.");

            var cashRegister = await _context.CashRegisters
                .Where(cr => !cr.IsClosed && cr.Status == "Open")
                .FirstOrDefaultAsync();

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

            _context.CashMovements.Add(cashMovement);

            // Atualizar saldo do caixa
            if (movement.Type == "Entrada")
                cashRegister.FinalValue += movement.Amount;
            else
                cashRegister.FinalValue -= movement.Amount;

            _context.Update(cashRegister);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Movimentação registrada com sucesso!" });
        }

        // Métodos auxiliares
        private async Task<decimal> GetCurrentBalance()
        {
            var cashRegister = await _context.CashRegisters
                .Where(cr => !cr.IsClosed && cr.Status == "Open")
                .FirstOrDefaultAsync();

            return cashRegister?.FinalValue ?? 0;
        }

        private async Task<bool> IsCashRegisterOpen()
        {
            return await _context.CashRegisters
                .AnyAsync(cr => !cr.IsClosed && cr.Status == "Open");
        }

        private async Task<List<CashMovement>> GetRecentMovements()
        {
            return await _context.CashMovements
                .Include(cm => cm.CashRegister)
                .OrderByDescending(cm => cm.Date)
                .Take(10)
                .ToListAsync();
        }

        private async Task<List<Product>> GetProductsForSale()
        {
            return await _context.Products
                .Where(p => p.Stock > 0)
                .Include(p => p.ProductCategory)
                .ToListAsync();
        }

        private async Task<decimal> GetTotalEntradasHoje(DateTime hoje)
        {
            return await _context.CashMovements
                .Where(c => c.Date.Date == hoje && c.Type == "Entrada")
                .SumAsync(c => (decimal?)c.Amount) ?? 0;
        }

        private async Task<decimal> GetTotalSaidasHoje(DateTime hoje)
        {
            return await _context.CashMovements
                .Where(c => c.Date.Date == hoje && c.Type == "Saída")
                .SumAsync(c => (decimal?)c.Amount) ?? 0;
        }

        // API para dados do gráfico de fluxo de caixa
        [HttpGet]
        public IActionResult GetCashFlowData()
        {
            var hoje = DateTime.Today;
            var ultimosDias = Enumerable.Range(0, 7)
                .Select(i => hoje.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            var data = ultimosDias.Select(dia => new {
                Data = dia.ToString("dd/MM"),
                Entradas = _context.CashMovements
                    .Where(c => c.Date.Date == dia && c.Type == "Entrada")
                    .Sum(c => (decimal?)c.Amount) ?? 0,
                Saidas = _context.CashMovements
                    .Where(c => c.Date.Date == dia && c.Type == "Saída")
                    .Sum(c => (decimal?)c.Amount) ?? 0
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
    }
}