using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.Json;

class Program
{
    static void Main()
    {
        try
        {
            Console.WriteLine("🔍 Testando conexão com SQL Server...");
            Console.WriteLine();

            // Ler a connection string do appsettings.json
            string connectionString = GetConnectionString();
            
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("❌ Erro: Não foi possível obter a connection string do appsettings.json");
                return;
            }

            // Testar a conexão
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                // Extrair informações da connection string
                var builder = new SqlConnectionStringBuilder(connectionString);
                string serverName = builder.DataSource;
                string databaseName = builder.InitialCatalog;

                // Contar tabelas
                int tableCount = GetTableCount(connection);

                // Exibir resultados
                Console.WriteLine("✅ Conexão bem-sucedida com o SQL Server!");
                Console.WriteLine($"Servidor: {serverName}");
                Console.WriteLine($"Base de dados: {databaseName}");
                Console.WriteLine($"Tabelas encontradas: {tableCount}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro na conexão: {ex.Message}");
        }
    }

    static string GetConnectionString()
    {
        try
        {
            string json = File.ReadAllText("../appsettings.json");
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                if (doc.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings))
                {
                    if (connectionStrings.TryGetProperty("DefaultConnection", out var defaultConnection))
                    {
                        return defaultConnection.GetString();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao ler appsettings.json: {ex.Message}");
        }
        return null;
    }

    static int GetTableCount(SqlConnection connection)
    {
        try
        {
            using (var command = new SqlCommand(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", 
                connection))
            {
                return (int)command.ExecuteScalar();
            }
        }
        catch
        {
            return 0;
        }
    }
}
