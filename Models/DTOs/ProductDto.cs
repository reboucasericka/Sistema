using System.ComponentModel.DataAnnotations;

namespace SistemaAPI.DTOs
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? CostPrice { get; set; }
        public int Stock { get; set; } = 0;
        public int StockQuantity { get; set; } = 0;
        public int? MinStock { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Brand { get; set; }
        public string? SKU { get; set; }
        public Guid ImageId { get; set; }
        public int ProductCategoryId { get; set; }
        public int? SupplierId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Propriedades de navegação simplificadas
        public string? CategoryName { get; set; }
        public string? Category { get; set; }
        public string? SupplierName { get; set; }
        
        public string ImageFullPath => ImageId == Guid.Empty
            ? "/images/noimage.png"
            : $"/uploads/products/{ImageId}.png";
    }
}
