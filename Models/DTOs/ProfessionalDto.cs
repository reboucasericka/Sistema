using System.ComponentModel.DataAnnotations;

namespace SistemaAPI.DTOs
{
    public class ProfessionalDto
    {
        public int ProfessionalId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Specialty { get; set; }
        public string? Specialization { get; set; }
        public decimal? CommissionPercent { get; set; }
        public string? Address { get; set; }
        public DateTime? BirthDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
        public Guid ImageId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Propriedades de navegação simplificadas
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        
        public string ImageFullPath => ImageId == Guid.Empty
            ? "/images/noimage.png"
            : $"/uploads/professionals/{ImageId}.png";
    }
}
