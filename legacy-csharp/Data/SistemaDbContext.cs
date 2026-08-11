using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;


namespace Sistema.Data
{
    public class SistemaDbContext : IdentityDbContext<User, IdentityRole, string>
    {
        public SistemaDbContext(DbContextOptions<SistemaDbContext> options) : base(options) { }


        // Map tables → each DbSet represents a table

        public DbSet<Profile> Profiles { get; set; }
        public DbSet<AccessLog> AccessLogs { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        // Users is already available through IdentityDbContext
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Reminder> Reminders { get; set; }

        public DbSet<Professional> Professionals { get; set; }
        public DbSet<ProfessionalService> ProfessionalServices { get; set; }

        public DbSet<Plan> Plans { get; set; }

        public DbSet<PriceTable> PriceTables { get; set; }

        public DbSet<PlanAppointment> PlanAppointments { get; set; }

        public DbSet<Schedule> Schedules { get; set; }

        public DbSet<ProfessionalSchedule> ProfessionalSchedules { get; set; }

        public DbSet<ProcedureHistory> ProcedureHistories { get; set; }

        public DbSet<ServiceReview> ServiceReviews { get; set; }

        public DbSet<Feedback> Feedbacks { get; set; }

        public DbSet<Setting> Settings { get; set; }

        public DbSet<StockMovement> StockMovements { get; set; }

        public DbSet<Payable> Payables { get; set; }

        public DbSet<Receivable> Receivables { get; set; }

        public DbSet<PaymentMethod> PaymentMethods { get; set; }

        public DbSet<CashRegister> CashRegisters { get; set; }
        public DbSet<CashMovement> CashMovements { get; set; }

        public DbSet<Billing> Billings { get; set; }
        public DbSet<BillingDetails> BillingDetails { get; set; }

        public DbSet<StockEntry> StockEntries { get; set; }

        public DbSet<StockExit> StockExits { get; set; }

        public DbSet<BeautyCenter> BeautyCenters { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Professional -> User relationship
            builder.Entity<Professional>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure AccessLog -> User relationship
            builder.Entity<AccessLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure StockMovement -> User relationship
            builder.Entity<StockMovement>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure StockExit -> User relationship
            builder.Entity<StockExit>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure StockEntry -> User relationship
            builder.Entity<StockEntry>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Service -> Category relationship
            builder.Entity<Service>()
                .HasOne(s => s.Category)
                .WithMany(c => c.Service)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Product -> ProductCategory relationship
            builder.Entity<Product>()
                .HasOne(p => p.ProductCategory)
                .WithMany(pc => pc.Products)
                .HasForeignKey(p => p.ProductCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Product -> Supplier relationship
            builder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Product -> User relationship
            builder.Entity<Product>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure ProfessionalSchedule -> Professional relationship
            builder.Entity<ProfessionalSchedule>()
                .HasOne(ps => ps.Professional)
                .WithMany()
                .HasForeignKey(ps => ps.ProfessionalId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ProfessionalSchedule -> User relationship
            builder.Entity<ProfessionalSchedule>()
                .HasOne(ps => ps.User)
                .WithMany()
                .HasForeignKey(ps => ps.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Appointment -> Customer relationship
            builder.Entity<Appointment>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Appointments)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Appointment -> Professional relationship
            builder.Entity<Appointment>()
                .HasOne(a => a.Professional)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.ProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Appointment -> Service relationship
            builder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany()
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Customer -> User relationship
            builder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Payable relationships
            builder.Entity<Payable>()
                .HasOne(p => p.Supplier)
                .WithMany()
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payable>()
                .HasOne(p => p.Professional)
                .WithMany()
                .HasForeignKey(p => p.ProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payable>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payable>()
                .HasOne(p => p.PaymentMethod)
                .WithMany(pm => pm.Payables)
                .HasForeignKey(p => p.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Payable>()
                .HasOne(p => p.Sale)
                .WithMany(s => s.Payables)
                .HasForeignKey(p => p.SaleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Receivable relationships
            builder.Entity<Receivable>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Receivable>()
                .HasOne(r => r.Professional)
                .WithMany()
                .HasForeignKey(r => r.ProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Receivable>()
                .HasOne(r => r.Service)
                .WithMany()
                .HasForeignKey(r => r.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Receivable>()
                .HasOne(r => r.Sale)
                .WithMany(s => s.Receivables)
                .HasForeignKey(r => r.SaleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Receivable>()
                .HasOne(r => r.PaymentMethod)
                .WithMany(pm => pm.Receivables)
                .HasForeignKey(r => r.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Receivable>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Sale relationships
            builder.Entity<Sale>()
                .HasOne(s => s.Customer)
                .WithMany()
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Sale>()
                .HasOne(s => s.Professional)
                .WithMany()
                .HasForeignKey(s => s.ProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Sale>()
                .HasOne(s => s.PaymentMethod)
                .WithMany(pm => pm.Sales)
                .HasForeignKey(s => s.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Sale>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure SaleItem relationships
            builder.Entity<SaleItem>()
                .HasOne(si => si.Sale)
                .WithMany(s => s.Items)
                .HasForeignKey(si => si.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SaleItem>()
                .HasOne(si => si.Product)
                .WithMany()
                .HasForeignKey(si => si.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure CashRegister relationships
            builder.Entity<CashRegister>()
                .HasOne(cr => cr.UserAbertura)
                .WithMany()
                .HasForeignKey(cr => cr.UserIdAbertura)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CashRegister>()
                .HasOne(cr => cr.UserFechamento)
                .WithMany()
                .HasForeignKey(cr => cr.UserIdFechamento)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure CashMovement relationships
            builder.Entity<CashMovement>()
                .HasOne(cm => cm.CashRegister)
                .WithMany(cr => cr.CashMovements)
                .HasForeignKey(cm => cm.CashRegisterId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
