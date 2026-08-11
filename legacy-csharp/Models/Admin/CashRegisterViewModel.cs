using Sistema.Data.Entities;

namespace Sistema.Models.Admin
{
    public class CashRegisterViewModel
    {
        public decimal CurrentBalance { get; set; }
        public bool IsOpen { get; set; }
        public List<CashMovement> RecentMovements { get; set; } = new List<CashMovement>();
        public List<Product> Products { get; set; } = new List<Product>();
        
        // Dados para os cards financeiros
        public decimal TotalEntradasHoje { get; set; }
        public decimal TotalSaidasHoje { get; set; }
        public decimal SaldoAtual { get; set; }
    }

    public class CashMovementViewModel
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
