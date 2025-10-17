using System;

namespace Sistema.Models
{
    public class GoogleCalendarEventDto
    {
        public int AppointmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ProfessionalEmail { get; set; }
        public string? CustomerEmail { get; set; }
    }
}
