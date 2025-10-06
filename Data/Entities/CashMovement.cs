using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("CashMovements")]
    public class CashMovement : IEntity
    {
        [Key]
        public int Id { get; set; }
        
        public int MovementId => Id; // Alias for compatibility

        [Required]
        public int CashRegisterId { get; set; }

        [Required, StringLength(20)]
        public string Type { get; set; } // "entry" or "exit"

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required, StringLength(200)]
        public string Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [StringLength(100)]
        public string? Reference { get; set; } // Reference to related entity (Receivable, Pay, etc.)

        public int? RelatedEntityId { get; set; } // ID of related entity
        public string? RelatedEntityType { get; set; } // Type of related entity

        // Navigation Properties
        [ForeignKey("CashRegisterId")]
        public virtual CashRegister CashRegister { get; set; } = null!;
    }
}
