using Sistema.DTOs.Common;

namespace Sistema.DTOs.API
{
    public class ProfessionalDto : BaseDto
    {
        public int ProfessionalId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public Guid ImageId { get; set; }
        public decimal CommissionPercent { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
