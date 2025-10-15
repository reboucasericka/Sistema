using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Sales")]
    public class Sale : IEntity
    {
        [Key]
        public int SaleId { get; set; }

       

        [Column(TypeName = "datetime2")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime SaleDate { get; set; } = DateTime.Now;

        // 🔗 FK → Customer
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        // 🔗 FK → Professional
        public int ProfessionalId { get; set; }
        public Professional Professional { get; set; }

        // 🔗 FK → PaymentMethod
        public int PaymentMethodId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Discount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal FinalTotal { get; set; }

        // 🔗 FK → User who created
        public string UserId { get; set; }
        public User User { get; set; }

        // Navigation Properties
        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
        public ICollection<Receivable> Receivables { get; set; } = new List<Receivable>();
        public ICollection<Payable> Payables { get; set; } = new List<Payable>();
    }
}
