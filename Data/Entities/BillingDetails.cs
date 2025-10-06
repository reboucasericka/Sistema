using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("BillingDetails")]
    public class BillingDetails
    {
        [Key]
        public int BillingDetailId { get; set; }

        // 🔗 FK → Billing
        public int BillingId { get; set; }
        public Billing Billing { get; set; }

        [Required, StringLength(20)]
        public string ItemType { get; set; } // Product or Service

        // 🔗 FK → Product (optional)
        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        // 🔗 FK → Service (optional)
        public int? ServiceId { get; set; }
        public Service? Service { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitValue { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalValue { get; set; }
    }
}