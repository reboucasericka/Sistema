using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Schedule")]
    public class Schedule
    {
        [Key]
        public int ScheduleId { get; set; } // HorarioId

        // 🔗 FK → Professional
        public int ProfessionalId { get; set; } // ProfissionalId
        public Professional Professional { get; set; } // Profissional

        [Column(TypeName = "time")]
        public TimeSpan Time { get; set; } // Hora

        public int? DayOfWeek { get; set; } // DiaSemana: 1=Sunday ... 7=Saturday

        [Column(TypeName = "date")]
        public DateTime? SpecificDate { get; set; } // DataEspecifica

        [StringLength(20)]
        public string Status { get; set; } = "available"; // Status: disponível

        public bool IsExportedToExcel { get; set; } = false; // ExportadoExcel
        public bool IsExportedToPdf { get; set; } = false;   // ExportadoPdf
    }
}