using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Lembretes")]
    public class Lembrete
    {
        [Key]
        public int LembreteId { get; set; }

        // 🔗 FK → Agendamento
        public int AgendamentoId { get; set; }
        public Appointment Agendamento { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DataEnvio { get; set; }

        [Required, StringLength(20)]
        public string MeioEnvio { get; set; } // email, sms, whatsapp

        [StringLength(20)]
        public string Status { get; set; } = "pendente";

        public bool ExportadoExcel { get; set; } = false;
        public bool ExportadoPdf { get; set; } = false;
    }
}