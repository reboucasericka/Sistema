using Microsoft.Graph;
using Microsoft.Graph.Models;
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
        public int ClientId { get; set; } // FK
        public Client Client { get; set; } // Navegação

        // 🔗 FK → Service
        public int ServiceId { get; set; } // FK
        public Service Service { get; set; } // Navegação

        // 🔗 FK → Professional
        public int ProfessionalId { get; set; } // FK
        public Professional Professional { get; set; } // Navegação

        [Column(TypeName = "date")] // Usar date para armazenar apenas a data
        public DateTime Data { get; set; } // Data do agendamento

        [Column(TypeName = "time")] // Usar time para armazenar apenas o horário
        public TimeSpan Horario { get; set; } // Horário do agendamento

        [StringLength(20)]
        public string Status { get; set; } = "scheduled"; // Status do agendamento (e.g., scheduled, completed, canceled)

        public string? Notes { get; set; } // Notas adicionais sobre o agendamento

        public bool ReminderSent { get; set; } = false; // Indica se o lembrete foi enviado
        public bool ExportedToExcel { get; set; } = false; // Indica se o agendamento foi exportado para Excel
        public bool ExportedToPdf { get; set; } = false; // Indica se o agendamento foi exportado para PDF

        // 🔗 Relação 1:N → um agendamento pode ter vários lembretes
        public ICollection<Reminder> Reminders { get; set; } // Navegação
    }
}