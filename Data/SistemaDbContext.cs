using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;

namespace Sistema.Data
{
    public class SistemaDbContext : IdentityDbContext<User>
    {
        public SistemaDbContext(DbContextOptions<SistemaDbContext> options) : base(options)
        {
        }

        // Entities
        public DbSet<AccessLog> AccessLogs { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<BeautyCenter> BeautyCenters { get; set; }
        public DbSet<Billing> Billings { get; set; }
        public DbSet<BillingDetails> BillingDetails { get; set; }
        public DbSet<CashMovement> CashMovements { get; set; }
        public DbSet<CashRegister> CashRegisters { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Payable> Payables { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<PlanAppointment> PlanAppointments { get; set; }
        public DbSet<PriceTable> PriceTables { get; set; }
        public DbSet<ProcedureHistory> ProcedureHistories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Professional> Professionals { get; set; }
        public DbSet<ProfessionalSchedule> ProfessionalSchedules { get; set; }
        public DbSet<ProfessionalService> ProfessionalServices { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Receivable> Receivables { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceReview> ServiceReviews { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<StockEntry> StockEntries { get; set; }
        public DbSet<StockExit> StockExits { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PublicProductInfo> PublicProductInfos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurações específicas das entidades podem ser adicionadas aqui
            // Por exemplo, configurações de relacionamentos, índices, etc.
        }
    }
}
