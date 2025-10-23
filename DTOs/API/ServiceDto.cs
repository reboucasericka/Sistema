using Sistema.DTOs.Common;

namespace Sistema.DTOs.API
{
    public class ServiceDto : BaseDto
    {
        public int ServiceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationInMinutes { get; set; }
        public bool IsActive { get; set; } = true;
        public decimal Commission { get; set; } = 0;
        public int ReturnDays { get; set; } = 0;
    }
}
