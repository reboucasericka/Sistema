using Sistema.Data.Entities;

namespace Sistema.Helpers
{
    public interface ICashRegisterHelper
    {
        Task<CashRegister?> GetOpenCashRegisterAsync();
        Task<bool> IsCashRegisterOpenAsync();
        Task<string> GetCashRegisterStatusMessageAsync();
    }
}
