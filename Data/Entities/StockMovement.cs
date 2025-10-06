using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("StockMovements")]
    public class StockMovement
    {
        [Key]
        public int MovementId { get; set; }

        // 🔗 FK → Product
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }

        [Required, StringLength(10)]
        public string MovementType { get; set; } // entry, output, adjustment, loss

        [Required, StringLength(100)]
        public string Reason { get; set; }

        // 🔗 FK → User
        public string UserId { get; set; }
        public User User { get; set; }

        // 🔗 FK → Supplier (optional)
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime MovementDate { get; set; } = DateTime.Now;

        [Column(TypeName = "date")]
        public DateTime? ExpiryDate { get; set; }

        [StringLength(50)]
        public string? Batch { get; set; }
    }
}