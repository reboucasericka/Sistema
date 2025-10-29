using System.ComponentModel.DataAnnotations;

namespace SistemaAPI.DTOs
{
    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Duration { get; set; }
        public bool IsActive { get; set; } = true;
        public int DurationInMinutes { get; set; } = 60;
        public decimal Commission { get; set; } = 0;
        public int ReturnDays { get; set; } = 0;
        public Guid ImageId { get; set; }
        public int CategoryId { get; set; }
        
        // Propriedades de navegação simplificadas
        public string? CategoryName { get; set; }
        public string ImageFullPath => ImageId == Guid.Empty
            ? "/images/noimage.png"
            : $"/uploads/services/{ImageId}.png";
            
        // Propriedades de compatibilidade
        public int DurationMinutes { get; set; } = 60;
        public string? Category { get; set; }
    }
}
