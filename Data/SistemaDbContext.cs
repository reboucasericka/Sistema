using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using System.Reflection.Metadata;


namespace Sistema.Data
{
    public class SistemaDbContext : IdentityDbContext<User, IdentityRole, string>
    {
        public SistemaDbContext(DbContextOptions<SistemaDbContext> options) : base(options) { }


        // Mapear tabelas → cada DbSet representa uma tabela


        public DbSet<Profile> Profiles { get; set; } //Perfil
        public DbSet<AccessLog> AccessLogs { get; set; } //RegistroAcesso
        public DbSet<Client> Clients { get; set; } //Cliente
        public DbSet<Product> Products { get; set; } //Produto
        public DbSet<User> Users { get; set; } //Usuário
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Supplier> Suppliers{ get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<ServiceCategories> ServiceCategories { get; set; }




        public DbSet<HistoricoProcedimento> HistoricoProcedimentos { get; set; }
        public DbSet<AvaliacaoAtendimento> AvaliacoesAtendimento { get; set; }
        public DbSet<Service> Services { get; set; }
       
        public DbSet<Professional> Professionals { get; set; }
        public DbSet<ProfissionalServico> ProfissionalServicos { get; set; }
       
        public DbSet<Horario> Horarios { get; set; }
        public DbSet<Lembrete> Lembretes { get; set; }
        public DbSet<Configuracao> Configuracoes { get; set; }
        public DbSet<EstoqueMovimentacao> EstoqueMovimentacoes { get; set; }
        public DbSet<Pagar> Pagar { get; set; }
        public DbSet<Receber> Receber { get; set; }
        public DbSet<FormaPagamento> FormasPagamento { get; set; }
        public DbSet<Caixa> Caixas { get; set; }
        public DbSet<Faturacao> Faturacoes { get; set; }
        
       
        
        public DbSet<EstoqueEntrada> EstoqueEntradas { get; set; }
        public DbSet<EstoqueSaida> EstoqueSaidas { get; set; }                        
        public DbSet<Estetica> Esteticas { get; set; }
        public DbSet<Plano> Planos { get; set; }
        public DbSet<PlanoAgendamento> PlanoAgendamentos { get; set; }              
        
    }
}
