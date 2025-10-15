using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Sistema.Services
{
    public interface IExcelExportService
    {
        byte[] ExportClientsToExcel(IEnumerable<dynamic> clients);
        byte[] ExportCashRegisterToExcel(IEnumerable<dynamic> cashRegisters);
        byte[] ExportSalesToExcel(IEnumerable<dynamic> sales);
        byte[] ExportAppointmentsToExcel(IEnumerable<dynamic> appointments);
        byte[] ExportFinancialSummaryToExcel(dynamic summary, DateTime startDate, DateTime endDate);
        byte[] ExportSalesReportToExcel(IEnumerable<dynamic> sales, dynamic summary);
        byte[] ExportCashFlowReportToExcel(dynamic summary, IEnumerable<dynamic> receivables, IEnumerable<dynamic> payables, IEnumerable<dynamic> cashMovements);
        byte[] ExportCommissionsReportToExcel(IEnumerable<dynamic> commissions, dynamic summary);
    }

    public class ExcelExportService : IExcelExportService
    {
        public ExcelExportService()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public byte[] ExportClientsToExcel(IEnumerable<dynamic> clients)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Clientes");

            // Cabeçalhos
            worksheet.Cells[1, 1].Value = "Nome";
            worksheet.Cells[1, 2].Value = "Email";
            worksheet.Cells[1, 3].Value = "Telefone";
            worksheet.Cells[1, 4].Value = "Endereço";
            worksheet.Cells[1, 5].Value = "Data de Nascimento";
            worksheet.Cells[1, 6].Value = "Data de Cadastro";
            worksheet.Cells[1, 7].Value = "Status";

            // Estilo do cabeçalho
            using (var range = worksheet.Cells[1, 1, 1, 7])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Dados
            int row = 2;
            foreach (var client in clients)
            {
                worksheet.Cells[row, 1].Value = client.Name;
                worksheet.Cells[row, 2].Value = client.Email;
                worksheet.Cells[row, 3].Value = client.Phone;
                worksheet.Cells[row, 4].Value = client.Address;
                worksheet.Cells[row, 5].Value = client.BirthDate?.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 6].Value = client.RegistrationDate.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 7].Value = client.IsActive ? "Ativo" : "Inativo";
                row++;
            }

            // Ajustar largura das colunas
            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        public byte[] ExportCashRegisterToExcel(IEnumerable<dynamic> cashRegisters)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Caixa");

            // Cabeçalhos
            worksheet.Cells[1, 1].Value = "Data";
            worksheet.Cells[1, 2].Value = "Valor Inicial";
            worksheet.Cells[1, 3].Value = "Valor Final";
            worksheet.Cells[1, 4].Value = "Diferença";
            worksheet.Cells[1, 5].Value = "Status";
            worksheet.Cells[1, 6].Value = "Aberto por";
            worksheet.Cells[1, 7].Value = "Fechado por";

            // Estilo do cabeçalho
            using (var range = worksheet.Cells[1, 1, 1, 7])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Dados
            int row = 2;
            foreach (var cash in cashRegisters)
            {
                var difference = cash.ValorFinal - cash.ValorInicial;
                worksheet.Cells[row, 1].Value = cash.Data.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 2].Value = cash.ValorInicial;
                worksheet.Cells[row, 3].Value = cash.ValorFinal;
                worksheet.Cells[row, 4].Value = difference;
                worksheet.Cells[row, 5].Value = cash.Status;
                worksheet.Cells[row, 6].Value = cash.UsuarioAberturaRef?.FirstName;
                worksheet.Cells[row, 7].Value = cash.UsuarioFechamentoRef?.FirstName;
                row++;
            }

            // Ajustar largura das colunas
            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        public byte[] ExportSalesToExcel(IEnumerable<dynamic> sales)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Vendas");

            // Cabeçalhos
            worksheet.Cells[1, 1].Value = "Data";
            worksheet.Cells[1, 2].Value = "Valor Total";
            worksheet.Cells[1, 3].Value = "Quantidade Serviços";
            worksheet.Cells[1, 4].Value = "Quantidade Produtos";
            worksheet.Cells[1, 5].Value = "Usuário";

            // Estilo do cabeçalho
            using (var range = worksheet.Cells[1, 1, 1, 5])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Dados
            int row = 2;
            foreach (var sale in sales)
            {
                worksheet.Cells[row, 1].Value = sale.Data.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 2].Value = sale.ValorTotal;
                worksheet.Cells[row, 3].Value = sale.QuantidadeServicos;
                worksheet.Cells[row, 4].Value = sale.QuantidadeProdutos;
                worksheet.Cells[row, 5].Value = sale.Usuario?.FirstName;
                row++;
            }

            // Ajustar largura das colunas
            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        public byte[] ExportAppointmentsToExcel(IEnumerable<dynamic> appointments)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Agendamentos");

            // Cabeçalhos
            worksheet.Cells[1, 1].Value = "Data";
            worksheet.Cells[1, 2].Value = "Hora";
            worksheet.Cells[1, 3].Value = "Cliente";
            worksheet.Cells[1, 4].Value = "Profissional";
            worksheet.Cells[1, 5].Value = "Serviço";
            worksheet.Cells[1, 6].Value = "Status";

            // Estilo do cabeçalho
            using (var range = worksheet.Cells[1, 1, 1, 6])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Dados
            int row = 2;
            foreach (var appointment in appointments)
            {
                worksheet.Cells[row, 1].Value = appointment.Date.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 2].Value = appointment.Time.ToString("HH:mm");
                worksheet.Cells[row, 3].Value = appointment.Client?.Name;
                worksheet.Cells[row, 4].Value = appointment.Professional?.Name;
                worksheet.Cells[row, 5].Value = appointment.Service?.Name;
                worksheet.Cells[row, 6].Value = appointment.Status;
                row++;
            }

            // Ajustar largura das colunas
            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        public byte[] ExportFinancialSummaryToExcel(dynamic summary, DateTime startDate, DateTime endDate)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Resumo Financeiro");

            // Título
            worksheet.Cells[1, 1].Value = "Resumo Financeiro";
            worksheet.Cells[1, 1, 1, 2].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;

            // Período
            worksheet.Cells[2, 1].Value = $"Período: {startDate:dd/MM/yyyy} a {endDate:dd/MM/yyyy}";
            worksheet.Cells[2, 1, 2, 2].Merge = true;

            // Dados do resumo
            int row = 4;
            var summaryData = new[]
            {
                new { Label = "Receitas Totais", Value = summary.TotalReceitas },
                new { Label = "Receitas Pagas", Value = summary.ReceitasPagas },
                new { Label = "Receitas Pendentes", Value = summary.ReceitasPendentes },
                new { Label = "Despesas Totais", Value = summary.TotalDespesas },
                new { Label = "Despesas Pagas", Value = summary.DespesasPagas },
                new { Label = "Despesas Pendentes", Value = summary.DespesasPendentes },
                new { Label = "Total Vendas", Value = summary.TotalVendas },
                new { Label = "Quantidade Vendas", Value = summary.QuantidadeVendas },
                new { Label = "Total Entradas", Value = summary.TotalEntradas },
                new { Label = "Total Saídas", Value = summary.TotalSaidas },
                new { Label = "Saldo Líquido", Value = summary.SaldoLiquido },
                new { Label = "Total Comissões", Value = summary.TotalComissoes },
                new { Label = "Comissões Pagas", Value = summary.ComissoesPagas },
                new { Label = "Comissões Pendentes", Value = summary.ComissoesPendentes }
            };

            worksheet.Cells[row, 1].Value = "Descrição";
            worksheet.Cells[row, 2].Value = "Valor (€)";
            worksheet.Cells[row, 1, row, 2].Style.Font.Bold = true;
            worksheet.Cells[row, 1, row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[row, 1, row, 2].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

            row++;
            foreach (var item in summaryData)
            {
                worksheet.Cells[row, 1].Value = item.Label;
                worksheet.Cells[row, 2].Value = item.Value;
                row++;
            }

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        public byte[] ExportSalesReportToExcel(IEnumerable<dynamic> sales, dynamic summary)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Relatório de Vendas");

            // Resumo
            worksheet.Cells[1, 1].Value = "Resumo de Vendas";
            worksheet.Cells[1, 1, 1, 5].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;

            worksheet.Cells[2, 1].Value = "Total de Vendas:";
            worksheet.Cells[2, 2].Value = summary.TotalSales;
            worksheet.Cells[3, 1].Value = "Valor Total:";
            worksheet.Cells[3, 2].Value = summary.TotalAmount;
            worksheet.Cells[4, 1].Value = "Ticket Médio:";
            worksheet.Cells[4, 2].Value = summary.AverageTicket;

            // Dados das vendas
            int row = 6;
            worksheet.Cells[row, 1].Value = "Data";
            worksheet.Cells[row, 2].Value = "Cliente";
            worksheet.Cells[row, 3].Value = "Profissional";
            worksheet.Cells[row, 4].Value = "Valor Total";
            worksheet.Cells[row, 5].Value = "Método de Pagamento";

            worksheet.Cells[row, 1, row, 5].Style.Font.Bold = true;
            worksheet.Cells[row, 1, row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[row, 1, row, 5].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);

            row++;
            foreach (var sale in sales)
            {
                worksheet.Cells[row, 1].Value = sale.SaleDate.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 2].Value = $"{sale.Customer?.User?.FirstName} {sale.Customer?.User?.LastName}";
                worksheet.Cells[row, 3].Value = sale.Professional?.Name;
                worksheet.Cells[row, 4].Value = sale.FinalTotal;
                worksheet.Cells[row, 5].Value = sale.PaymentMethod?.Name;
                row++;
            }

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        public byte[] ExportCashFlowReportToExcel(dynamic summary, IEnumerable<dynamic> receivables, IEnumerable<dynamic> payables, IEnumerable<dynamic> cashMovements)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Fluxo de Caixa");

            // Resumo
            worksheet.Cells[1, 1].Value = "Resumo do Fluxo de Caixa";
            worksheet.Cells[1, 1, 1, 3].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;

            worksheet.Cells[2, 1].Value = "Total Recebimentos:";
            worksheet.Cells[2, 2].Value = summary.TotalReceivables;
            worksheet.Cells[3, 1].Value = "Total Pagamentos:";
            worksheet.Cells[3, 2].Value = summary.TotalPayables;
            worksheet.Cells[4, 1].Value = "Saldo Líquido:";
            worksheet.Cells[4, 2].Value = summary.SaldoLiquido;

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        public byte[] ExportCommissionsReportToExcel(IEnumerable<dynamic> commissions, dynamic summary)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Comissões");

            // Resumo
            worksheet.Cells[1, 1].Value = "Resumo de Comissões";
            worksheet.Cells[1, 1, 1, 3].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;

            worksheet.Cells[2, 1].Value = "Total Comissões:";
            worksheet.Cells[2, 2].Value = summary.TotalCommissions;
            worksheet.Cells[3, 1].Value = "Comissões Pagas:";
            worksheet.Cells[3, 2].Value = summary.TotalPaidCommissions;
            worksheet.Cells[4, 1].Value = "Comissões Pendentes:";
            worksheet.Cells[4, 2].Value = summary.TotalPendingCommissions;

            // Dados das comissões
            int row = 6;
            worksheet.Cells[row, 1].Value = "Data";
            worksheet.Cells[row, 2].Value = "Profissional";
            worksheet.Cells[row, 3].Value = "Valor";
            worksheet.Cells[row, 4].Value = "Status";

            worksheet.Cells[row, 1, row, 4].Style.Font.Bold = true;
            worksheet.Cells[row, 1, row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[row, 1, row, 4].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);

            row++;
            foreach (var commission in commissions)
            {
                worksheet.Cells[row, 1].Value = commission.CreatedAt.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 2].Value = commission.Professional?.Name;
                worksheet.Cells[row, 3].Value = commission.Amount;
                worksheet.Cells[row, 4].Value = commission.IsPaid ? "Pago" : "Pendente";
                row++;
            }

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }
    }
}
