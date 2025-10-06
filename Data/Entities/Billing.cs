using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Billings")]
    public class Billing
    {
        [Key]
        public int BillingId { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalValue { get; set; }

        public int ServicesQuantity { get; set; } = 0;

        public int ProductsQuantity { get; set; } = 0;

        // 🔗 FK → User
        public int UserId { get; set; }
        public User User { get; set; }

        public bool ExportedToExcel { get; set; } = false;
        public bool ExportedToPdf { get; set; } = false;
    }
}
