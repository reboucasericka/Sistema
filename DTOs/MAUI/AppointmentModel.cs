namespace Sistema.DTOs.MAUI
{
    public class AppointmentModel
    {
        public int AppointmentId { get; set; }
        public int CustomerId { get; set; }
        public int ProfessionalId { get; set; }
        public int ServiceId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "Pending";
        
        // Dados relacionados para exibição
        public string CustomerName { get; set; } = string.Empty;
        public string ProfessionalName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public decimal ServicePrice { get; set; }
        
        // Propriedades para UI
        public string FormattedTime => StartTime.ToString("HH:mm");
        public string FormattedDate => StartTime.ToString("dd/MM/yyyy");
        public string Duration => $"{(EndTime - StartTime).TotalMinutes} min";
    }
}
