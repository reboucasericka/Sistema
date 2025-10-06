using Sistema.Data.Entities;
using Sistema.Models.Admin;
using Sistema.Models.Public;

namespace Sistema.Helpers
{
    public interface IConverterHelper
    {
        // Product mappings
        Sistema.Data.Entities.Product ToProduct(ProductViewModel model, string photoPath, bool isNew);
        ProductViewModel ToProductViewModel(Sistema.Data.Entities.Product product);
        
        // Customer mappings
        Customer ToCustomer(CustomerCreateViewModel model);
        Customer ToCustomer(CustomerEditViewModel model);
        CustomerViewModel ToCustomerViewModel(Customer customer);
        CustomerCreateViewModel ToCustomerCreateViewModel(Customer customer);
        CustomerEditViewModel ToCustomerEditViewModel(Customer customer);
        
        // Profile mappings
        CustomerProfileViewModel ToCustomerProfileViewModel(Customer customer);
        CustomerProfileEditViewModel ToCustomerProfileEditViewModel(Customer customer);
        Customer ToCustomer(CustomerProfileEditViewModel model);
        
        // Professional mappings
        Professional ToProfessional(ProfessionalCreateViewModel model);
        Professional ToProfessional(ProfessionalEditViewModel model);
        ProfessionalViewModel ToProfessionalViewModel(Professional professional);
        ProfessionalCreateViewModel ToProfessionalCreateViewModel(Professional professional);
        ProfessionalEditViewModel ToProfessionalEditViewModel(Professional professional);
        
        // Professional Profile mappings
        ProfessionalProfileViewModel ToProfessionalProfileViewModel(Professional professional);
        ProfessionalProfileEditViewModel ToProfessionalProfileEditViewModel(Professional professional);
        Professional ToProfessional(ProfessionalProfileEditViewModel model);
    }
}
