using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace Sistema.Data.Entities
{
    [Table("PlanAppointments")]
    public class PlanAppointment
    {
        [Key]
        public int PlanAppointmentId { get; set; }

        // 🔗 FK → Plan
        public int PlanId { get; set; }
        public Plan Plan { get; set; } // Plano

        // 🔗 FK → Appointment
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; } // Agendamento

        public bool IsSessionUsed { get; set; } = false; // SessaoUsada
    }
}