using Microsoft.Data.SqlClient;
using System;

class Program
{
    static void Main()
    {
        string connectionString = "Server=REBOUCAS\\SISTEMA;Database=Sistema;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True";
        
        Console.WriteLine("🔍 Testando conexão com SQL Server...");
        Console.WriteLine($"📡 String de conexão: {connectionString}");
        Console.WriteLine();
        
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                Console.WriteLine("⏳ Tentando conectar...");
                connection.Open();
                
                Console.WriteLine("✅ Conexão estabelecida com sucesso!");
                Console.WriteLine($"📊 Servidor: {connection.ServerVersion}");
                Console.WriteLine($"🗄️ Base de dados: {connection.Database}");
                Console.WriteLine($"🔗 Estado: {connection.State}");
                
                // Teste simples de query
                using (var command = new SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES", connection))
                {
                    var result = command.ExecuteScalar();
                    Console.WriteLine($"📋 Número de tabelas na base de dados: {result}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Erro na conexão:");
            Console.WriteLine($"   Tipo: {ex.GetType().Name}");
            Console.WriteLine($"   Mensagem: {ex.Message}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Erro interno: {ex.InnerException.Message}");
            }
        }
        
        Console.WriteLine();
        Console.WriteLine("Pressione qualquer tecla para sair...");
        Console.ReadKey();
    }
}