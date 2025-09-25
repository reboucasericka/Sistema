using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Horarios")]
    public class Horario
    {
        [Key]
        public int HorarioId { get; set; }

        // 🔗 FK → Profissional
        public int ProfissionalId { get; set; }
        public Profissional Profissional { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan Hora { get; set; }

        public int? DiaSemana { get; set; } // 1=Domingo ... 7=Sábado

        [Column(TypeName = "date")]
        public DateTime? DataEspecifica { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "disponível";

        public bool ExportadoExcel { get; set; } = false;
        public bool ExportadoPdf { get; set; } = false;
    }
}