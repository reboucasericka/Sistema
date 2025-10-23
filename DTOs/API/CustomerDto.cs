using Sistema.DTOs.Common;

namespace Sistema.DTOs.API
{
    public class CustomerDto : BaseDto
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string? Gender { get; set; }
        public string? PreferredTime { get; set; }
        public string? PreferredDay { get; set; }
        public string? AllergyHistory { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
