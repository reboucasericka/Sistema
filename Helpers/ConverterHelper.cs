using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Sistema.Data.Entities;
using Sistema.Models;

namespace Sistema.Helpers
{
    public class ConverterHelper : IConverterHelper
    {

        public Sistema.Data.Entities.Product ToProduct(ProductViewModel model, Guid imageId, bool isNew)
        {
            return new Sistema.Data.Entities.Product
            {
                ProductId = isNew ? 0 : model.ProductId,
                Name = model.Name,
                Description = model.Description,
                ProductCategoryId = model.ProductCategoryId,
                PurchasePrice = model.PurchasePrice,
                SalePrice = model.SalePrice,
                Stock = model.Stock,
                ImageId = imageId,
                MinimumStockLevel = model.MinimumStockLevel,
                SupplierId = model.SupplierId
            };
        }

        public ProductViewModel ToProductViewModel(Sistema.Data.Entities.Product product)
        {
            return new ProductViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                ProductCategoryId = product.ProductCategoryId,
                PurchasePrice = product.PurchasePrice,
                SalePrice = product.SalePrice,
                Stock = product.Stock,
                MinimumStockLevel = product.MinimumStockLevel,
                SupplierId = product.SupplierId,
                ImageId = product.ImageId
            };
        }
    }
}
