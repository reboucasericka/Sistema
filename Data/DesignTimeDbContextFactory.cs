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
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SistemaDB;Trusted_Connection=true;MultipleActiveResultSets=true");
            
            return new SistemaDbContext(optionsBuilder.Options);
        }
    }
}
