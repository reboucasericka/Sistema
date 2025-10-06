using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Appointments")]
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; } // PK

        // 🔗 FK → Cliente
        public int CustomerId { get; set; } // FK
        public Customer Customer { get; set; } // Navegação

        // 🔗 FK → Service
        public int ServiceId { get; set; } // FK
        public Service Service { get; set; } // Navegação

        // 🔗 FK → Professional
        public int ProfessionalId { get; set; } // FK
        public Professional Professional { get; set; } // Navegação

        [Column(TypeName = "datetime")]
        public DateTime StartTime { get; set; } // Data e hora de início

        [Column(TypeName = "datetime")]
        public DateTime EndTime { get; set; } // Data e hora de fim

        // Propriedades para compatibilidade com as views
        [NotMapped]
        public DateTime Date => StartTime.Date;

        [NotMapped]
        public TimeSpan Time => StartTime.TimeOfDay;

        [NotMapped]
        public Customer Client => Customer;

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Status do agendamento (Pending, Confirmed, Completed, Canceled)

        public string? Notes { get; set; } // Notas adicionais sobre o agendamento

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TotalPrice { get; set; } // Preço total do agendamento

        public bool IsActive { get; set; } = true; // Indica se o agendamento está ativo

        public bool ReminderSent { get; set; } = false; // Indica se o lembrete foi enviado
        public bool ExportedToExcel { get; set; } = false; // Indica se o agendamento foi exportado para Excel
        public bool ExportedToPdf { get; set; } = false; // Indica se o agendamento foi exportado para PDF

        // 🔗 Relação 1:N → um agendamento pode ter vários lembretes
        public ICollection<Reminder> Reminders { get; set; } // Navegação
    }
}