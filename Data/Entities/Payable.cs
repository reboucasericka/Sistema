using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Payables")]
    public class Payable
    {
        [Key]
        public int PayableId { get; set; }

        [StringLength(100)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Type { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Value { get; set; }

        [Column(TypeName = "date")]
        public DateTime LaunchDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime DueDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? PaymentDate { get; set; }

        // 🔗 FK → User who launched
        public int UserId { get; set; }
        public User LaunchUser { get; set; }

        // 🔗 FK → User who cleared (optional)
        public int? ClearUserId { get; set; }
        public User? ClearUser { get; set; }

        [StringLength(200)]
        public string? Photo { get; set; }

        public int? PersonId { get; set; } // generic field (customer/supplier)

        public bool IsPaid { get; set; } = false;

        // 🔗 FK → Product (optional)
        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public int? Quantity { get; set; }

        // 🔗 FK → PaymentMethod (optional)
        public int? PaymentMethodId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }

        public int Installment { get; set; } = 1;
        public int TotalInstallments { get; set; } = 1;

        public bool ExportedToExcel { get; set; } = false;
        public bool ExportedToPdf { get; set; } = false;
    }
}