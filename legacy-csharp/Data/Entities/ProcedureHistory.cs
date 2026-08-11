using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("ProcedureHistories")]
    public class ProcedureHistory
    {
        [Key]
        public int ProcedureId { get; set; }

        // 🔗 FK → Cliente
        public int CustomerId { get; set; }
        public Customer Cliente { get; set; }

        // 🔗 FK → Servico
        public int ServiceId { get; set; }
        public Service Servico { get; set; }

        // 🔗 FK → Profissional
        public int ProfessionalId { get; set; }
        public Professional Profissional { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DataProcedimento { get; set; } = DateTime.Now;

        [StringLength(255)]
        public string? MaterialUsado { get; set; } // Ex.: Fios 12mm, Curvatura D

        public string? ObservacoesTecnicas { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Preco { get; set; }
    }
}