using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Planos")]
    public class Plano
    {
        [Key]
        public int PlanoId { get; set; }

        [Required, StringLength(100)]
        public string Nome { get; set; }

        public string? Descricao { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Valor { get; set; }

        public int QuantidadeSessoes { get; set; }

        public int ValidadeDias { get; set; }

        public bool Ativo { get; set; } = true;

        public bool ExportadoExcel { get; set; } = false;
        public bool ExportadoPdf { get; set; } = false;

        // 🔗 FK → Cliente (opcional)
        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        // Relação 1:N
        public ICollection<PlanoAgendamento> PlanoAgendamentos { get; set; }
    }
}
