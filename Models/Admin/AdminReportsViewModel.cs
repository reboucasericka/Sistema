using System.Collections.Generic;

namespace Sistema.Models.Admin
{
    public class AdminReportsViewModel
    {
        public decimal TotalEntradas { get; set; }
        public decimal TotalSaidas { get; set; }
        public decimal SaldoAtual { get; set; }
        public int ClientesNovos { get; set; }
        public int ClientesTotais { get; set; }
        public List<ReportItem> ServicosMaisVendidos { get; set; } = new();
    }

    public class ReportItem
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
