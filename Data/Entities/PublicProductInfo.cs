using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("PublicProductInfos")]
    public class PublicProductInfo : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? ApplicationMethod { get; set; }

        [MaxLength(1000)]
        public string? Ingredients { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        public bool IsActive { get; set; } = true;

        [Display(Name = "Foto do Produto")]
        public Guid? ImageId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string ImageFullPath => ImageId == Guid.Empty || ImageId == null
            ? "/images/noimage.png"
            : $"/uploads/product-info/{ImageId}.png";
    }
}
