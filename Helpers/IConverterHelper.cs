using Sistema.Data.Entities;
using Sistema.Models;

namespace Sistema.Helpers
{
    public interface IConverterHelper
    {
        Sistema.Data.Entities.Product ToProduct(ProductViewModel model, Guid imageId, bool isNew);
        ProductViewModel ToProductViewModel(Sistema.Data.Entities.Product product);
    }
}
