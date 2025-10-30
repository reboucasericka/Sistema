using System.ComponentModel.DataAnnotations;

namespace SistemaAPI.DTOs
{
    public class PublicProductInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ApplicationMethod { get; set; }
        public string? Ingredients { get; set; }
        public string? Brand { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid ImageId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public string ImageFullPath => ImageId == Guid.Empty
            ? "/images/noimage.png"
            : $"/uploads/product-info/{ImageId}.png";
    }
}
