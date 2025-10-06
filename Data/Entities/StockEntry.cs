using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("StockEntries")]
    public class StockEntry
    {
        [Key]
        public int EntryId { get; set; }

        // 🔗 FK → Product
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }

        [Required, StringLength(100)]
        public string Reason { get; set; }

        // 🔗 FK → User (who created)
        public string UserId { get; set; }
        public User User { get; set; }

        // 🔗 FK → Supplier (optional)
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [StringLength(20)]
        public string MovementType { get; set; } = "entry";

        [StringLength(50)]
        public string? Batch { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ExpiryDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? UnitValue { get; set; }

        public bool ExportedToExcel { get; set; } = false;
        public bool ExportedToPdf { get; set; } = false;
    }
}