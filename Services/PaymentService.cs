using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;
using Sistema.Helpers;

namespace Sistema.Services
{
    public interface IPaymentService
    {
        Task<bool> MarkPayableAsPaidAsync(int payableId, string userId);
        Task<bool> MarkReceivableAsPaidAsync(int receivableId, string userId);
        Task<CashRegister?> GetOpenCashRegisterAsync();
        Task<bool> CreateCashMovementAsync(CashMovement cashMovement);
    }

    public class PaymentService : IPaymentService
    {
        private readonly SistemaDbContext _context;
        private readonly IPayableRepository _payableRepository;
        private readonly IReceivableRepository _receivableRepository;
        private readonly ICashRegisterHelper _cashRegisterHelper;

        public PaymentService(
            SistemaDbContext context,
            IPayableRepository payableRepository,
            IReceivableRepository receivableRepository,
            ICashRegisterHelper cashRegisterHelper)
        {
            _context = context;
            _payableRepository = payableRepository;
            _receivableRepository = receivableRepository;
            _cashRegisterHelper = cashRegisterHelper;
        }

        public async Task<CashRegister?> GetOpenCashRegisterAsync()
        {
            return await _cashRegisterHelper.GetOpenCashRegisterAsync();
        }

        public async Task<bool> CreateCashMovementAsync(CashMovement cashMovement)
        {
            _context.CashMovements.Add(cashMovement);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> MarkPayableAsPaidAsync(int payableId, string userId)
        {
            var payable = await _payableRepository.GetByIdAsync(payableId);
            if (payable == null) return false;

            var openCashRegister = await GetOpenCashRegisterAsync();
            if (openCashRegister == null) return false;

            // Marcar como pago
            var success = await _payableRepository.MarkAsPaidAsync(payableId, userId);
            if (!success) return false;

            // Criar movimento de caixa (saída)
            var cashMovement = new CashMovement
            {
                CashRegisterId = openCashRegister.CashRegisterId,
                Type = "exit",
                Amount = payable.Value,
                Description = $"Pagamento realizado - {payable.Description}",
                Date = DateTime.Now,
                Reference = $"Payable-{payable.PayableId}",
                RelatedEntityId = payable.PayableId,
                RelatedEntityType = "Payable"
            };

            return await CreateCashMovementAsync(cashMovement);
        }

        public async Task<bool> MarkReceivableAsPaidAsync(int receivableId, string userId)
        {
            var receivable = await _receivableRepository.GetByIdAsync(receivableId);
            if (receivable == null) return false;

            var openCashRegister = await GetOpenCashRegisterAsync();
            if (openCashRegister == null) return false;

            // Marcar como pago
            var success = await _receivableRepository.MarkAsPaidAsync(receivableId, userId);
            if (!success) return false;

            // Criar movimento de caixa (entrada)
            var cashMovement = new CashMovement
            {
                CashRegisterId = openCashRegister.CashRegisterId,
                Type = "entry",
                Amount = receivable.Value,
                Description = $"Recebimento realizado - {receivable.Description}",
                Date = DateTime.Now,
                Reference = $"Receivable-{receivable.ReceivableId}",
                RelatedEntityId = receivable.ReceivableId,
                RelatedEntityType = "Receivable"
            };

            return await CreateCashMovementAsync(cashMovement);
        }
    }
}
