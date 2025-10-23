namespace Sistema.DTOs.MAUI
{
    public class ProfessionalModel
    {
        public int ProfessionalId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public decimal CommissionPercent { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Propriedades para UI
        public string DisplayName => $"{Name} - {Specialty}";
        public string CommissionDisplay => $"{CommissionPercent}%";
    }
}
