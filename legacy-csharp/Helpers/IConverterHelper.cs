using Sistema.Data.Entities;
using Sistema.Models.Admin;
using Sistema.Models.Public;

namespace Sistema.Helpers
{
    public interface IConverterHelper
    {
        // Converte de ViewModel → Entidade
        Product ToProduct(AdminProductViewModel model, Guid imageId, bool isNew, string? userId = null);
        // Converte de Entidade → ViewModel
        AdminProductViewModel ToProductViewModel(Product product);

        // Converte de Entidade → ViewModel
        

        // Customer mappings
        Customer ToCustomer(AdminCustomerCreateViewModel model);
        Customer ToCustomer(AdminCustomerEditViewModel model);
        AdminCustomerViewModel ToCustomerViewModel(Customer customer);
        AdminCustomerCreateViewModel ToCustomerCreateViewModel(Customer customer);
        AdminCustomerEditViewModel ToCustomerEditViewModel(Customer customer);
        
        // Profile mappings
        PublicCustomerProfileViewModel ToCustomerProfileViewModel(Customer customer);
        PublicCustomerProfileEditViewModel ToCustomerProfileEditViewModel(Customer customer);
        Customer ToCustomer(PublicCustomerProfileEditViewModel model);
        
        // Professional mappings
        Professional ToProfessional(AdminProfessionalCreateViewModel model);
        Professional ToProfessional(AdminProfessionalEditViewModel model);
        AdminProfessionalViewModel ToProfessionalViewModel(Professional professional);
        AdminProfessionalCreateViewModel ToProfessionalCreateViewModel(Professional professional);
        AdminProfessionalEditViewModel ToProfessionalEditViewModel(Professional professional);
        
        // Professional Profile mappings
        PublicProfessionalProfileViewModel ToProfessionalProfileViewModel(Professional professional);
        ProfessionalProfileEditViewModel ToProfessionalProfileEditViewModel(Professional professional);
        Professional ToProfessional(ProfessionalProfileEditViewModel model);
    }
}
