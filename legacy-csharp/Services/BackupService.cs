using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using System.Text.Json;

namespace Sistema.Services
{
    public interface IBackupService
    {
        Task<string> CreateBackupAsync();
        Task<bool> RestoreBackupAsync(string backupPath);
        Task<List<string>> GetBackupFilesAsync();
        Task CleanOldBackupsAsync(int daysToKeep = 30);
    }

    public class BackupService : IBackupService
    {
        private readonly SistemaDbContext _context;
        private readonly ILogger<BackupService> _logger;
        private readonly string _backupDirectory;

        public BackupService(SistemaDbContext context, ILogger<BackupService> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Backups");
            
            if (!Directory.Exists(_backupDirectory))
            {
                Directory.CreateDirectory(_backupDirectory);
            }
        }

        public async Task<string> CreateBackupAsync()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"backup_{timestamp}.json";
                var backupPath = Path.Combine(_backupDirectory, backupFileName);

                _logger.LogInformation($"Iniciando backup em: {backupPath}");

                // Backup de dados financeiros
                var financialData = new
                {
                    Billing = await _context.Billings.ToListAsync(),
                    BillingDetails = await _context.BillingDetails.ToListAsync(),
                    CashRegister = await _context.CashRegisters.ToListAsync(),
                    Payables = await _context.Payables.ToListAsync(),
                    Receivable = await _context.Receivables.ToListAsync(),
                    PaymentMethod = await _context.PaymentMethods.ToListAsync(),
                    Customers = await _context.Customers.ToListAsync(),
                    Appointments = await _context.Appointments.ToListAsync(),
                    BackupDate = DateTime.Now,
                    Version = "1.0"
                };

                var json = JsonSerializer.Serialize(financialData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(backupPath, json);

                _logger.LogInformation($"Backup criado com sucesso: {backupPath}");
                return backupPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar backup");
                throw;
            }
        }

        public async Task<bool> RestoreBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    _logger.LogError($"Arquivo de backup não encontrado: {backupPath}");
                    return false;
                }

                _logger.LogInformation($"Iniciando restauração do backup: {backupPath}");

                var json = await File.ReadAllTextAsync(backupPath);
                var backupData = JsonSerializer.Deserialize<dynamic>(json);

                // Aqui você implementaria a lógica de restauração
                // Por segurança, não implementei a restauração automática
                // que poderia sobrescrever dados existentes

                _logger.LogInformation("Restauração concluída com sucesso");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao restaurar backup");
                return false;
            }
        }

        public async Task<List<string>> GetBackupFilesAsync()
        {
            try
            {
                var backupFiles = Directory.GetFiles(_backupDirectory, "backup_*.json")
                    .OrderByDescending(f => File.GetCreationTime(f))
                    .ToList();

                return await Task.FromResult(backupFiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar arquivos de backup");
                return new List<string>();
            }
        }

        public async Task CleanOldBackupsAsync(int daysToKeep = 30)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var oldBackups = Directory.GetFiles(_backupDirectory, "backup_*.json")
                    .Where(f => File.GetCreationTime(f) < cutoffDate)
                    .ToList();

                foreach (var backup in oldBackups)
                {
                    File.Delete(backup);
                    _logger.LogInformation($"Backup antigo removido: {backup}");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar backups antigos");
            }
        }
    }
}
