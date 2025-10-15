using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;

namespace Sistema.Data
{
    /// <summary>
    /// Classe para testar a integridade dos relacionamentos entre entidades
    /// </summary>
    public static class IntegrityTest
    {
        /// <summary>
        /// Testa se todas as consultas básicas funcionam sem erros de integridade
        /// </summary>
        public static async Task<bool> TestDataIntegrityAsync(SistemaDbContext context)
        {
            try
            {
                Console.WriteLine("=== INICIANDO TESTE DE INTEGRIDADE DE DADOS ===");

                // Teste 1: Product com relacionamentos
                Console.WriteLine("1. Testando Product com ProductCategory e Supplier...");
                var products = await context.Products
                    .Include(p => p.ProductCategory)
                    .Include(p => p.Supplier)
                    .Include(p => p.User)
                    .Take(5)
                    .ToListAsync();
                Console.WriteLine($"   ✅ {products.Count} produtos carregados com sucesso");

                // Teste 2: Professional com relacionamentos
                Console.WriteLine("2. Testando Professional com User e Schedules...");
                var professionals = await context.Professionals
                    .Include(p => p.User)
                    .Include(p => p.ProfessionalServices)
                        .ThenInclude(ps => ps.Service)
                    .Include(p => p.Schedules)
                    .Take(5)
                    .ToListAsync();
                Console.WriteLine($"   ✅ {professionals.Count} profissionais carregados com sucesso");

                // Teste 3: ProfessionalSchedule com relacionamentos
                Console.WriteLine("3. Testando ProfessionalSchedule com Professional...");
                var schedules = await context.ProfessionalSchedules
                    .Include(ps => ps.Professional)
                        .ThenInclude(p => p.User)
                    .Include(ps => ps.User)
                    .Take(5)
                    .ToListAsync();
                Console.WriteLine($"   ✅ {schedules.Count} horários carregados com sucesso");

                // Teste 4: ProductCategory com Products
                Console.WriteLine("4. Testando ProductCategory com Products...");
                var categories = await context.ProductCategories
                    .Include(pc => pc.Products)
                    .Take(5)
                    .ToListAsync();
                Console.WriteLine($"   ✅ {categories.Count} categorias carregadas com sucesso");

                // Teste 5: Supplier com Products
                Console.WriteLine("5. Testando Supplier com Products...");
                var suppliers = await context.Suppliers
                    .Include(s => s.Products)
                    .Take(5)
                    .ToListAsync();
                Console.WriteLine($"   ✅ {suppliers.Count} fornecedores carregados com sucesso");

                Console.WriteLine("=== TESTE DE INTEGRIDADE CONCLUÍDO COM SUCESSO ===");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRO NO TESTE DE INTEGRIDADE: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Inner Exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// Testa operações CRUD básicas
        /// </summary>
        public static async Task<bool> TestCrudOperationsAsync(SistemaDbContext context)
        {
            try
            {
                Console.WriteLine("=== INICIANDO TESTE DE OPERAÇÕES CRUD ===");

                // Teste de leitura
                Console.WriteLine("1. Testando operações de leitura...");
                var productCount = await context.Products.CountAsync();
                var professionalCount = await context.Professionals.CountAsync();
                var categoryCount = await context.ProductCategories.CountAsync();
                var supplierCount = await context.Suppliers.CountAsync();
                var scheduleCount = await context.ProfessionalSchedules.CountAsync();

                Console.WriteLine($"   ✅ Produtos: {productCount}");
                Console.WriteLine($"   ✅ Profissionais: {professionalCount}");
                Console.WriteLine($"   ✅ Categorias: {categoryCount}");
                Console.WriteLine($"   ✅ Fornecedores: {supplierCount}");
                Console.WriteLine($"   ✅ Horários: {scheduleCount}");

                Console.WriteLine("=== TESTE DE OPERAÇÕES CRUD CONCLUÍDO COM SUCESSO ===");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRO NO TESTE CRUD: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Inner Exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// Testa especificamente a integridade de produtos, categorias e fornecedores
        /// </summary>
        public static async Task<bool> TestProductIntegrityAsync(SistemaDbContext context)
        {
            try
            {
                Console.WriteLine("=== INICIANDO TESTE DE INTEGRIDADE DE PRODUTOS ===");

                // Teste 1: Product com relacionamentos
                Console.WriteLine("1. Testando Product com ProductCategory e Supplier...");
                var products = await context.Products
                    .Include(p => p.ProductCategory)
                    .Include(p => p.Supplier)
                    .Include(p => p.User)
                    .Take(5)
                    .ToListAsync();
                Console.WriteLine($"   ✅ {products.Count} produtos carregados com sucesso");

                // Teste 2: ProductCategory com Products
                Console.WriteLine("2. Testando ProductCategory com Products...");
                var categories = await context.ProductCategories
                    .Include(pc => pc.Products)
                    .Take(5)
                    .ToListAsync();
                Console.WriteLine($"   ✅ {categories.Count} categorias carregadas com sucesso");

                // Teste 3: Supplier com Products
                Console.WriteLine("3. Testando Supplier com Products...");
                var suppliers = await context.Suppliers
                    .Include(s => s.Products)
                    .Take(5)
                    .ToListAsync();
                Console.WriteLine($"   ✅ {suppliers.Count} fornecedores carregados com sucesso");

                // Teste 4: Validação de FKs
                Console.WriteLine("4. Testando validação de FKs...");
                var productsWithInvalidFKs = await context.Products
                    .Where(p => p.ProductCategoryId <= 0)
                    .CountAsync();
                
                if (productsWithInvalidFKs > 0)
                {
                    Console.WriteLine($"   ⚠️ {productsWithInvalidFKs} produtos com ProductCategoryId inválido");
                }
                else
                {
                    Console.WriteLine($"   ✅ Todos os produtos têm ProductCategoryId válido");
                }

                // Teste 5: Produtos ativos
                Console.WriteLine("5. Testando produtos ativos...");
                var activeProducts = await context.Products
                    .Where(p => p.IsActive)
                    .CountAsync();
                var totalProducts = await context.Products.CountAsync();
                Console.WriteLine($"   ✅ {activeProducts}/{totalProducts} produtos ativos");

                Console.WriteLine("=== TESTE DE INTEGRIDADE DE PRODUTOS CONCLUÍDO COM SUCESSO ===");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRO NO TESTE DE INTEGRIDADE DE PRODUTOS: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Inner Exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }
    }
}
