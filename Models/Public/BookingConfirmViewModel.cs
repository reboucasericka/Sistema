using System.ComponentModel.DataAnnotations;

namespace Sistema.Models.Public
{
    public class BookingConfirmViewModel
    {
        public int ServiceId { get; set; }
        public int ProfessionalId { get; set; }
        public int CustomerId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ProfessionalName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public int ServiceDuration { get; set; }
        public decimal Price { get; set; }
        
        [Display(Name = "Observações")]
        public string? Notes { get; set; }
    }
}
