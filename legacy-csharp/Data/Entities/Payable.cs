using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Payables")]
    public class Payable : IEntity
    {
        [Key]
        public int PayableId { get; set; }

        // Implementação da interface IEntity
       

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Value { get; set; } // Alias para Amount

        [Column(TypeName = "date")]
        public DateTime DueDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? PaymentDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Paid

        [StringLength(50)]
        public string Type { get; set; } = "Expense"; // Expense, Commission, etc.

        [Column(TypeName = "date")]
        public DateTime LaunchDate { get; set; } = DateTime.Now;

        public bool IsPaid { get; set; } = false;

        public int? PersonId { get; set; } // ID da pessoa relacionada

        public int? Installment { get; set; } // Parcela atual
        public int? TotalInstallments { get; set; } // Total de parcelas

        public string? Photo { get; set; } // Foto/documento

        public string? Notes { get; set; } // Notas adicionais

        // Navigation properties for user relationships
        public User LaunchUser => User;
        public User? ClearUser { get; set; } // Usuário que quitou
        public string? ClearUserId { get; set; } // ID do usuário que quitou

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 🔗 FK → Supplier (nullable - para pagamentos a fornecedores)
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        // 🔗 FK → Professional (nullable - para comissões de profissionais)
        public int? ProfessionalId { get; set; }
        public Professional? Professional { get; set; }

        // 🔗 FK → User who created
        public string UserId { get; set; }
        public User User { get; set; }

        // 🔗 FK → PaymentMethod (optional)
        public int? PaymentMethodId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }

        // 🔗 FK → Sale (optional - para comissões vinculadas a vendas)
        public int? SaleId { get; set; }
        public Sale? Sale { get; set; }

        // 🔗 FK → Product (optional - para pagamentos relacionados a produtos)
        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public bool ExportedToExcel { get; set; } = false;
        public bool ExportedToPdf { get; set; } = false;
    }
}