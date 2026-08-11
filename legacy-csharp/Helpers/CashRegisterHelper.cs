using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;

namespace Sistema.Helpers
{
    public class CashRegisterHelper : ICashRegisterHelper
    {
        private readonly SistemaDbContext _context;

        public CashRegisterHelper(SistemaDbContext context)
        {
            _context = context;
        }

        public async Task<CashRegister?> GetOpenCashRegisterAsync()
        {
            return await _context.CashRegisters
                .Where(c => c.Status == "aberto")
                .OrderByDescending(c => c.Date)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsCashRegisterOpenAsync()
        {
            var openCashRegister = await GetOpenCashRegisterAsync();
            return openCashRegister != null;
        }

        public async Task<string> GetCashRegisterStatusMessageAsync()
        {
            var isOpen = await IsCashRegisterOpenAsync();
            return isOpen 
                ? "Caixa aberto" 
                : "Nenhum caixa aberto encontrado. Por favor, abra um caixa primeiro.";
        }
    }
}
