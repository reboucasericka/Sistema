using System.ComponentModel.DataAnnotations;

namespace SistemaAPI.DTOs
{
    public class AppointmentDto
    {
        public int AppointmentId { get; set; }
        public int ClientId { get; set; }
        public int ServiceId { get; set; }
        public int ProfessionalId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Notes { get; set; }
        public decimal? TotalPrice { get; set; }
        public bool IsActive { get; set; } = true;
        public bool ReminderSent { get; set; } = false;
        public bool ExportedToExcel { get; set; } = false;
        public bool ExportedToPdf { get; set; } = false;
        public DateTime? UpdatedAt { get; set; }
        public string? GoogleEventId { get; set; }
        
        // Propriedades de navegação simplificadas
        public string? ClientName { get; set; }
        public string? ClientEmail { get; set; }
        public string? ServiceName { get; set; }
        public string? ServiceCategory { get; set; }
        public string? ProfessionalName { get; set; }
        public string? ProfessionalSpecialty { get; set; }
        
        // Propriedades calculadas para compatibilidade
        public DateTime StartDateTime => AppointmentDate.Add(StartTime);
        public DateTime EndDateTime => AppointmentDate.Add(EndTime);
    }
}
