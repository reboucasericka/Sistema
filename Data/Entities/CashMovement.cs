using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("CashMovements")]
    public class CashMovement : IEntity
    {
        [Key]
        public int CashMovementId { get; set; }
        
        

        [Required]
        public int CashRegisterId { get; set; }

        [Required, StringLength(20)]
        public string Type { get; set; } // "Entrada" or "Saída"

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required, StringLength(200)]
        public string Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        // Reference to Payable or Receivable
        public int? ReferenceId { get; set; }
        public string? ReferenceType { get; set; } // "Payable" or "Receivable"

        [StringLength(200)]
        public string? Reference { get; set; } // Referência do movimento

        [StringLength(50)]
        public string? RelatedEntityType { get; set; } // Tipo da entidade relacionada

        public int? RelatedEntityId { get; set; } // ID da entidade relacionada

        // Navigation Properties
        [ForeignKey("CashRegisterId")]
        public virtual CashRegister CashRegister { get; set; } = null!;
    }
}
