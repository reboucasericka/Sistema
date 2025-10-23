using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Sistema.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SistemaDbContext>
    {
        public SistemaDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SistemaDbContext>();
            
            // Configuração para design-time
            optionsBuilder.UseSqlServer("Server=REBOUCAS\\Sistema;Database=Sistema;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
            
            return new SistemaDbContext(optionsBuilder.Options);
        }
    }
}
