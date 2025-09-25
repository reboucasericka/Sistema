using Sistema.Data.Entities;
using Sistema.Models;

namespace Sistema.Helpers
{
    public interface IConverterHelper
    {
        Produto ToProduct(ProductViewModel model, string path, bool isNew);
        ProductViewModel ToProductViewModel(Produto produto);
    }
}
