using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;


namespace Sistema.Data
{
    public class SistemaDbContext : IdentityDbContext<Usuario>
    {
        

        public SistemaDbContext(DbContextOptions<SistemaDbContext> options) : base(options) { }

        // Mapear tabelas → cada DbSet representa uma tabela

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Perfil> Perfis { get; set; }
        public DbSet<RegistroAcesso> RegistroAcessos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<HistoricoProcedimento> HistoricoProcedimentos { get; set; }
        public DbSet<AvaliacaoAtendimento> AvaliacoesAtendimento { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<CategoriaServico> CategoriasServicos { get; set; }
        public DbSet<Profissional> Profissionais { get; set; }
        public DbSet<ProfissionalServico> ProfissionalServicos { get; set; }
        public DbSet<Agendamento> Agendamentos { get; set; }
        public DbSet<Horario> Horarios { get; set; }
        public DbSet<Lembrete> Lembretes { get; set; }
        public DbSet<Configuracao> Configuracoes { get; set; }
        public DbSet<EstoqueMovimentacao> EstoqueMovimentacoes { get; set; }
        public DbSet<Pagar> Pagar { get; set; }
        public DbSet<Receber> Receber { get; set; }
        public DbSet<FormaPagamento> FormasPagamento { get; set; }
        public DbSet<Caixa> Caixas { get; set; }
        public DbSet<Faturacao> Faturacoes { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<CategoriaProduto> CategoriasProdutos { get; set; }
        public DbSet<EstoqueEntrada> EstoqueEntradas { get; set; }
        public DbSet<EstoqueSaida> EstoqueSaidas { get; set; }                        
        public DbSet<Estetica> Esteticas { get; set; }
        public DbSet<Plano> Planos { get; set; }
        public DbSet<PlanoAgendamento> PlanoAgendamentos { get; set; }              
        
    }
}
