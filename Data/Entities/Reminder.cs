using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Reminder")]
    public class Reminder
    {
        [Key]
        public int ReminderId { get; set; }

        // 🔗 FK → Appointment
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime SendDate { get; set; } // DataEnvio

        [Required, StringLength(20)]
        public string SendMethod { get; set; } // MeioEnvio: email, sms, whatsapp

        [StringLength(20)]
        public string Status { get; set; } = "pending"; // Status: pendente

        public bool IsExportedToExcel { get; set; } = false; // ExportadoExcel
        public bool IsExportedToPdf { get; set; } = false;   // ExportadoPdf
    }
}