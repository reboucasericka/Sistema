using Sistema.DTOs.Common;

namespace Sistema.DTOs.API
{
    public class AppointmentDto : BaseDto
    {
        public int AppointmentId { get; set; }
        public int CustomerId { get; set; }
        public int ProfessionalId { get; set; }
        public int ServiceId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "Pending";
        
        // Dados relacionados (sem navegação EF)
        public string CustomerName { get; set; } = string.Empty;
        public string ProfessionalName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public decimal ServicePrice { get; set; }
    }
}
