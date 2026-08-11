using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Plans")]
    public class Plan
    {
        [Key]
        public int PlanId { get; set; } // PlanoId

        [Required, StringLength(100)]
        public string Name { get; set; } // Nome

        public string? Description { get; set; } // Descricao

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; } // Valor

        public int SessionQuantity { get; set; } // QuantidadeSessoes

        public int ValidityDays { get; set; } // ValidadeDias

        public bool IsActive { get; set; } = true; // Ativo

        public bool IsExportedToExcel { get; set; } = false; // ExportadoExcel
        public bool IsExportedToPdf { get; set; } = false;   // ExportadoPdf

        // 🔗 FK → Client (optional)
        public int? CustomerId { get; set; }
        public Customer? Client { get; set; } // Cliente

        // 1:N Relationship
        public ICollection<PlanAppointment> PlanAppointments { get; set; } // PlanoAgendamentos
    }
}