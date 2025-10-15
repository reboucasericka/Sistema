using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Receivables")]
    public class Receivable : IEntity
    {
        [Key]
        public int ReceivableId { get; set; }

        

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Value { get; set; } // Alias para Amount

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Paid

        public bool IsPaid { get; set; } = false;

        [Column(TypeName = "date")]
        public DateTime? PaymentDate { get; set; }

        public int? PersonId { get; set; } // ID da pessoa relacionada

        // Navigation properties for user relationships
        public User LaunchUser => User;
        public User? ClearUser { get; set; } // Usuário que quitou
        public string? ClearUserId { get; set; } // ID do usuário que quitou

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "date")]
        public DateTime LaunchDate { get; set; } = DateTime.Now;

        // 🔗 FK → Customer
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        // 🔗 FK → Professional
        public int ProfessionalId { get; set; }
        public Professional Professional { get; set; }

        // 🔗 FK → Service (nullable - para recebimentos de serviços)
        public int? ServiceId { get; set; }
        public Service? Service { get; set; }

        // 🔗 FK → Sale (nullable - para recebimentos de vendas)
        public int? SaleId { get; set; }
        public Sale? Sale { get; set; }

        // 🔗 FK → Product (optional - para recebimentos relacionados a produtos)
        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        // 🔗 FK → PaymentMethod
        public int PaymentMethodId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        // 🔗 FK → User who created
        public string UserId { get; set; }
        public User User { get; set; }

        public bool ExportedToExcel { get; set; } = false;
        public bool ExportedToPdf { get; set; } = false;
    }
}