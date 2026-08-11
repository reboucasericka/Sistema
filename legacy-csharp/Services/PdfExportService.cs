using System.Text;
using System.Text.Json;

namespace Sistema.Services
{
    public interface IPdfExportService
    {
        byte[] ExportFinancialSummaryToPdf(dynamic summary, DateTime startDate, DateTime endDate);
        byte[] ExportSalesReportToPdf(IEnumerable<dynamic> sales, dynamic summary);
        byte[] ExportCashFlowReportToPdf(dynamic summary, IEnumerable<dynamic> receivables, IEnumerable<dynamic> payables, IEnumerable<dynamic> cashMovements);
        byte[] ExportCommissionsReportToPdf(IEnumerable<dynamic> commissions, dynamic summary);
    }

    public class PdfExportService : IPdfExportService
    {
        public byte[] ExportFinancialSummaryToPdf(dynamic summary, DateTime startDate, DateTime endDate)
        {
            var html = GenerateFinancialSummaryHtml(summary, startDate, endDate);
            return GeneratePdfFromHtml(html);
        }

        public byte[] ExportSalesReportToPdf(IEnumerable<dynamic> sales, dynamic summary)
        {
            var html = GenerateSalesReportHtml(sales, summary);
            return GeneratePdfFromHtml(html);
        }

        public byte[] ExportCashFlowReportToPdf(dynamic summary, IEnumerable<dynamic> receivables, IEnumerable<dynamic> payables, IEnumerable<dynamic> cashMovements)
        {
            var html = GenerateCashFlowReportHtml(summary, receivables, payables, cashMovements);
            return GeneratePdfFromHtml(html);
        }

        public byte[] ExportCommissionsReportToPdf(IEnumerable<dynamic> commissions, dynamic summary)
        {
            var html = GenerateCommissionsReportHtml(commissions, summary);
            return GeneratePdfFromHtml(html);
        }

        private string GenerateFinancialSummaryHtml(dynamic summary, DateTime startDate, DateTime endDate)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Resumo Financeiro</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .section {{ margin-bottom: 25px; }}
        .section h3 {{ color: #333; border-bottom: 2px solid #007bff; padding-bottom: 5px; }}
        .summary-grid {{ display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }}
        .summary-item {{ background: #f8f9fa; padding: 15px; border-radius: 8px; border-left: 4px solid #007bff; }}
        .summary-item h4 {{ margin: 0 0 10px 0; color: #007bff; }}
        .summary-item .value {{ font-size: 18px; font-weight: bold; color: #333; }}
        .period {{ text-align: center; color: #666; margin-bottom: 20px; }}
        .footer {{ margin-top: 30px; text-align: center; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>Resumo Financeiro</h1>
        <div class='period'>Período: {startDate:dd/MM/yyyy} a {endDate:dd/MM/yyyy}</div>
    </div>

    <div class='summary-grid'>
        <div class='summary-item'>
            <h4>Receitas Totais</h4>
            <div class='value'>€ {summary.TotalReceitas:F2}</div>
        </div>
        <div class='summary-item'>
            <h4>Despesas Totais</h4>
            <div class='value'>€ {summary.TotalDespesas:F2}</div>
        </div>
        <div class='summary-item'>
            <h4>Receitas Pagas</h4>
            <div class='value'>€ {summary.ReceitasPagas:F2}</div>
        </div>
        <div class='summary-item'>
            <h4>Despesas Pagas</h4>
            <div class='value'>€ {summary.DespesasPagas:F2}</div>
        </div>
        <div class='summary-item'>
            <h4>Receitas Pendentes</h4>
            <div class='value'>€ {summary.ReceitasPendentes:F2}</div>
        </div>
        <div class='summary-item'>
            <h4>Despesas Pendentes</h4>
            <div class='value'>€ {summary.DespesasPendentes:F2}</div>
        </div>
        <div class='summary-item'>
            <h4>Total Vendas</h4>
            <div class='value'>€ {summary.TotalVendas:F2}</div>
        </div>
        <div class='summary-item'>
            <h4>Quantidade Vendas</h4>
            <div class='value'>{summary.QuantidadeVendas}</div>
        </div>
        <div class='summary-item'>
            <h4>Total Entradas</h4>
            <div class='value'>€ {summary.TotalEntradas:F2}</div>
        </div>
        <div class='summary-item'>
            <h4>Total Saídas</h4>
            <div class='value'>€ {summary.TotalSaidas:F2}</div>
        </div>
        <div class='summary-item'>
            <h4>Saldo Líquido</h4>
            <div class='value'>€ {summary.SaldoLiquido:F2}</div>
        </div>
        <div class='summary-item'>
            <h4>Total Comissões</h4>
            <div class='value'>€ {summary.TotalComissoes:F2}</div>
        </div>
    </div>

    <div class='footer'>
        <p>Relatório gerado em {DateTime.Now:dd/MM/yyyy HH:mm}</p>
    </div>
</body>
</html>";
        }

        private string GenerateSalesReportHtml(IEnumerable<dynamic> sales, dynamic summary)
        {
            var salesHtml = string.Join("", sales.Take(50).Select(sale => $@"
                <tr>
                    <td>{sale.SaleDate:dd/MM/yyyy}</td>
                    <td>{sale.Customer?.User?.FirstName} {sale.Customer?.User?.LastName}</td>
                    <td>{sale.Professional?.Name}</td>
                    <td>€ {sale.FinalTotal:F2}</td>
                    <td>{sale.PaymentMethod?.Name}</td>
                </tr>"));

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Relatório de Vendas</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .summary {{ background: #f8f9fa; padding: 20px; border-radius: 8px; margin-bottom: 20px; }}
        .summary-grid {{ display: grid; grid-template-columns: repeat(4, 1fr); gap: 15px; }}
        .summary-item {{ text-align: center; }}
        .summary-item h4 {{ margin: 0 0 5px 0; color: #007bff; }}
        .summary-item .value {{ font-size: 18px; font-weight: bold; color: #333; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #f2f2f2; }}
        .footer {{ margin-top: 30px; text-align: center; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>Relatório de Vendas</h1>
    </div>

    <div class='summary'>
        <div class='summary-grid'>
            <div class='summary-item'>
                <h4>Total de Vendas</h4>
                <div class='value'>{summary.TotalSales}</div>
            </div>
            <div class='summary-item'>
                <h4>Valor Total</h4>
                <div class='value'>€ {summary.TotalAmount:F2}</div>
            </div>
            <div class='summary-item'>
                <h4>Total de Itens</h4>
                <div class='value'>{summary.TotalItems}</div>
            </div>
            <div class='summary-item'>
                <h4>Ticket Médio</h4>
                <div class='value'>€ {summary.AverageTicket:F2}</div>
            </div>
        </div>
    </div>

    <table>
        <thead>
            <tr>
                <th>Data</th>
                <th>Cliente</th>
                <th>Profissional</th>
                <th>Valor</th>
                <th>Método de Pagamento</th>
            </tr>
        </thead>
        <tbody>
            {salesHtml}
        </tbody>
    </table>

    <div class='footer'>
        <p>Relatório gerado em {DateTime.Now:dd/MM/yyyy HH:mm}</p>
    </div>
</body>
</html>";
        }

        private string GenerateCashFlowReportHtml(dynamic summary, IEnumerable<dynamic> receivables, IEnumerable<dynamic> payables, IEnumerable<dynamic> cashMovements)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Relatório de Fluxo de Caixa</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .summary {{ background: #f8f9fa; padding: 20px; border-radius: 8px; margin-bottom: 20px; }}
        .summary-grid {{ display: grid; grid-template-columns: repeat(3, 1fr); gap: 15px; }}
        .summary-item {{ text-align: center; }}
        .summary-item h4 {{ margin: 0 0 5px 0; color: #007bff; }}
        .summary-item .value {{ font-size: 18px; font-weight: bold; color: #333; }}
        .footer {{ margin-top: 30px; text-align: center; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>Relatório de Fluxo de Caixa</h1>
    </div>

    <div class='summary'>
        <div class='summary-grid'>
            <div class='summary-item'>
                <h4>Total Recebimentos</h4>
                <div class='value'>€ {summary.TotalReceivables:F2}</div>
            </div>
            <div class='summary-item'>
                <h4>Total Pagamentos</h4>
                <div class='value'>€ {summary.TotalPayables:F2}</div>
            </div>
            <div class='summary-item'>
                <h4>Saldo Líquido</h4>
                <div class='value'>€ {summary.SaldoLiquido:F2}</div>
            </div>
        </div>
    </div>

    <div class='footer'>
        <p>Relatório gerado em {DateTime.Now:dd/MM/yyyy HH:mm}</p>
    </div>
</body>
</html>";
        }

        private string GenerateCommissionsReportHtml(IEnumerable<dynamic> commissions, dynamic summary)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Relatório de Comissões</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .summary {{ background: #f8f9fa; padding: 20px; border-radius: 8px; margin-bottom: 20px; }}
        .summary-grid {{ display: grid; grid-template-columns: repeat(3, 1fr); gap: 15px; }}
        .summary-item {{ text-align: center; }}
        .summary-item h4 {{ margin: 0 0 5px 0; color: #007bff; }}
        .summary-item .value {{ font-size: 18px; font-weight: bold; color: #333; }}
        .footer {{ margin-top: 30px; text-align: center; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>Relatório de Comissões</h1>
    </div>

    <div class='summary'>
        <div class='summary-grid'>
            <div class='summary-item'>
                <h4>Total Comissões</h4>
                <div class='value'>€ {summary.TotalCommissions:F2}</div>
            </div>
            <div class='summary-item'>
                <h4>Comissões Pagas</h4>
                <div class='value'>€ {summary.TotalPaidCommissions:F2}</div>
            </div>
            <div class='summary-item'>
                <h4>Comissões Pendentes</h4>
                <div class='value'>€ {summary.TotalPendingCommissions:F2}</div>
            </div>
        </div>
    </div>

    <div class='footer'>
        <p>Relatório gerado em {DateTime.Now:dd/MM/yyyy HH:mm}</p>
    </div>
</body>
</html>";
        }

        private byte[] GeneratePdfFromHtml(string html)
        {
            // Para uma implementação real, você usaria uma biblioteca como PuppeteerSharp, wkhtmltopdf, ou similar
            // Por enquanto, vamos retornar o HTML como bytes para simular
            return Encoding.UTF8.GetBytes(html);
        }
    }
}
