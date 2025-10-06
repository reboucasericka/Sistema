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
    }
}
