using Microsoft.AspNetCore.Identity;
using Sistema.Data.Entities;

namespace Sistema.Data
{
    public class SeedDb
    {
        private readonly SistemaDbContext _context;
        //rivate readonly UserManager<Usuario> _userManager;
        //private readonly RoleManager<IdentityRole> _roleManager;
        private Random _random;

        public SeedDb(SistemaDbContext context)
        {
            _context = context;
            _random = new Random();
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();

            await CheckPerfisAsync();
            await CheckUsuariosAsync();
            await CheckCategoriasServicosAsync();
            await CheckCategoriasProdutosAsync();
            await CheckFornecedoresAsync();
            await CheckEsteticaAsync();
        }

        private async Task CheckPerfisAsync()
        {
            if (!_context.Perfis.Any())
            {
                _context.Perfis.AddRange(
                    new Perfil { Nome = "Administrador" },
                    new Perfil { Nome = "Profissional" },
                    new Perfil { Nome = "Recepcionista" }
                );
                await _context.SaveChangesAsync();
            }
        }

        private async Task CheckUsuariosAsync()
        {
            if (!_context.Usuarios.Any())
            {
                var adminPerfil = _context.Perfis.FirstOrDefault(p => p.Nome == "Administrador");

                _context.Usuarios.Add(new Usuario
                {
                    Nome = "Admin",
                    Email = "admin@sistema.com",
                    SenhaHash = "123456", // ⚠️ idealmente guardar hash real
                    PerfilId = adminPerfil.PerfilId,
                    Ativo = true,
                    DataCadastro = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }
        }

        private async Task CheckCategoriasServicosAsync()
        {
            if (!_context.CategoriasServicos.Any())
            {
                _context.CategoriasServicos.AddRange(
                    new CategoriaServico { Nome = "Extensão de Cílios Fio a Fio" },
                    new CategoriaServico { Nome = "Volume Russo" },
                    new CategoriaServico { Nome = "Design de Sobrancelhas" }
                );
                await _context.SaveChangesAsync();
            }
        }

        private async Task CheckCategoriasProdutosAsync()
        {
            if (!_context.CategoriasProdutos.Any())
            {
                _context.CategoriasProdutos.AddRange(
                    new CategoriaProduto { Nome = "Colas e Adesivos" },
                    new CategoriaProduto { Nome = "Fios para Extensão" },
                    new CategoriaProduto { Nome = "Removedores" }
                );
                await _context.SaveChangesAsync();
            }
        }

        private async Task CheckFornecedoresAsync()
        {
            if (!_context.Fornecedores.Any())
            {
                _context.Fornecedores.Add(new Fornecedor
                {
                    Nome = "Fornecedor Padrão",
                    Email = "contato@fornecedor.com",
                    Telemovel = "912345678",
                    DataCadastro = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
        }

        private async Task CheckEsteticaAsync()
        {
            if (!_context.Esteticas.Any())
            {
                _context.Esteticas.Add(new Estetica
                {
                    Nome = "Clínica Padrão",
                    Endereco = "Rua das Flores, 100 - Coimbra",
                    Telemovel = "987654321",
                    Email = "contato@clinicapadrao.com",
                    Ativo = true
                });
                await _context.SaveChangesAsync();
            }
        }
    }
}