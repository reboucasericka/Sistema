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
        public DbSet<Service> Service { get; set; }
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

        }
    }
}
