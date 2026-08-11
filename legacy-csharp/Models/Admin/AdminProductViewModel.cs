using Sistema.Data.Entities;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Sistema.Models.Admin
{
    public class AdminProductViewModel 
    {
        public int ProductId { get; set; }

        [Required, MaxLength(150)]
        [Display(Name = "Nome do Produto")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        [Display(Name = "Descrição")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Por favor, selecione uma categoria")]
        [Range(1, int.MaxValue, ErrorMessage = "Por favor, selecione uma categoria válida")]
        [Display(Name = "Categoria")]
        public int ProductCategoryId { get; set; }

        [Display(Name = "Fornecedor")]
        public int? SupplierId { get; set; }

        [Required, Display(Name = "Preço de Compra")]
        [DataType(DataType.Currency)]
        public decimal PurchasePrice { get; set; }

        [Required, Display(Name = "Preço de Venda")]
        [DataType(DataType.Currency)]
        public decimal SalePrice { get; set; }

        [Display(Name = "Estoque Atual")]
        public int Stock { get; set; }

        [Display(Name = "Estoque Mínimo")]
        public int MinimumStockLevel { get; set; }

        [Display(Name = "Ativo")]
        public bool IsActive { get; set; }

        // GUID da imagem
        public Guid? ImageId { get; set; }

        [Display(Name = "Foto do Produto")]
        public IFormFile? ImageFile { get; set; }

        public string ImageFullPath => ImageId == Guid.Empty || ImageId == null
            ? $"https://supershoptpsi-ftc4dnb4bcbkgmhw.westeurope-01.azurewebsites.net/images/noimage.png"
            : $"https://supershopcontaarmazename.blob.core.windows.net/products/{ImageId}.png";
    }
}
