using System.ComponentModel.DataAnnotations;

namespace SistemaAPI.DTOs
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int ClientId { get; set; }
        public int? AppointmentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded
        public string? Notes { get; set; }
        public string? TransactionId { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Propriedades de navegação simplificadas
        public string? ClientName { get; set; }
        public string? AppointmentDescription { get; set; }
    }
}
