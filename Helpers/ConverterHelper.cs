using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Sistema.Data.Entities;
using Sistema.Models.Admin;
using Sistema.Models.Public;

namespace Sistema.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        // Product mappings
        public Sistema.Data.Entities.Product ToProduct(ProductViewModel model, string photoPath, bool isNew)
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
                PhotoPath = photoPath,
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
                PhotoPath = product.PhotoPath
            };
        }

        // Customer mappings
        public Customer ToCustomer(CustomerCreateViewModel model)
        {
            return new Customer
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                BirthDate = model.BirthDate,
                RegistrationDate = DateTime.Now,
                IsActive = model.IsActive,
                Notes = model.Notes,
                AllergyHistory = model.AllergyHistory,
                PhotoPath = model.PhotoPath
            };
        }

        public Customer ToCustomer(CustomerEditViewModel model)
        {
            return new Customer
            {
                CustomerId = model.CustomerId,
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                BirthDate = model.BirthDate,
                RegistrationDate = model.RegistrationDate,
                IsActive = model.IsActive,
                Notes = model.Notes,
                AllergyHistory = model.AllergyHistory,
                PhotoPath = model.PhotoPath
            };
        }

        public CustomerViewModel ToCustomerViewModel(Customer customer)
        {
            return new CustomerViewModel
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                BirthDate = customer.BirthDate,
                RegistrationDate = customer.RegistrationDate,
                IsActive = customer.IsActive,
                Notes = customer.Notes,
                AllergyHistory = customer.AllergyHistory,
                PhotoPath = customer.PhotoPath,
                Appointments = customer.Appointments?.ToList()
            };
        }

        public CustomerCreateViewModel ToCustomerCreateViewModel(Customer customer)
        {
            return new CustomerCreateViewModel
            {
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                BirthDate = customer.BirthDate,
                IsActive = customer.IsActive,
                Notes = customer.Notes,
                AllergyHistory = customer.AllergyHistory,
                PhotoPath = customer.PhotoPath
            };
        }

        public CustomerEditViewModel ToCustomerEditViewModel(Customer customer)
        {
            return new CustomerEditViewModel
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                BirthDate = customer.BirthDate,
                RegistrationDate = customer.RegistrationDate,
                IsActive = customer.IsActive,
                Notes = customer.Notes,
                AllergyHistory = customer.AllergyHistory,
                PhotoPath = customer.PhotoPath
            };
        }

        // Profile mappings
        public CustomerProfileViewModel ToCustomerProfileViewModel(Customer customer)
        {
            var appointments = customer.Appointments?.ToList() ?? new List<Appointment>();
            
            return new CustomerProfileViewModel
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                BirthDate = customer.BirthDate,
                RegistrationDate = customer.RegistrationDate,
                Notes = customer.Notes,
                AllergyHistory = customer.AllergyHistory,
                PhotoPath = customer.PhotoPath,
                Appointments = appointments,
                UpcomingAppointments = appointments.Where(a => a.Date >= DateTime.Today && a.Status != "Cancelado").ToList(),
                PastAppointments = appointments.Where(a => a.Date < DateTime.Today).ToList()
            };
        }

        public CustomerProfileEditViewModel ToCustomerProfileEditViewModel(Customer customer)
        {
            return new CustomerProfileEditViewModel
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                BirthDate = customer.BirthDate,
                Notes = customer.Notes,
                AllergyHistory = customer.AllergyHistory,
                PhotoPath = customer.PhotoPath
            };
        }

        public Customer ToCustomer(CustomerProfileEditViewModel model)
        {
            return new Customer
            {
                CustomerId = model.CustomerId,
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                BirthDate = model.BirthDate,
                Notes = model.Notes,
                AllergyHistory = model.AllergyHistory,
                PhotoPath = model.PhotoPath
            };
        }

        // Professional mappings
        public Professional ToProfessional(ProfessionalCreateViewModel model)
        {
            return new Professional
            {
                Name = model.Name,
                UserId = model.UserId,
                PhotoPath = model.PhotoPath,
                Specialty = model.Specialty,
                DefaultCommission = model.DefaultCommission,
                IsActive = model.IsActive
            };
        }

        public Professional ToProfessional(ProfessionalEditViewModel model)
        {
            return new Professional
            {
                ProfessionalId = model.ProfessionalId,
                Name = model.Name,
                UserId = model.UserId,
                PhotoPath = model.PhotoPath,
                Specialty = model.Specialty,
                DefaultCommission = model.DefaultCommission,
                IsActive = model.IsActive
            };
        }

        public ProfessionalViewModel ToProfessionalViewModel(Professional professional)
        {
            return new ProfessionalViewModel
            {
                ProfessionalId = professional.ProfessionalId,
                Name = professional.Name,
                UserId = professional.UserId,
                UserEmail = professional.User?.Email,
                PhotoPath = professional.PhotoPath,
                Specialty = professional.Specialty,
                DefaultCommission = professional.DefaultCommission,
                IsActive = professional.IsActive,
                ProfessionalServices = professional.ProfessionalServices?.ToList(),
                Appointments = professional.Appointments?.ToList(),
                Schedules = professional.Schedules?.ToList()
            };
        }

        public ProfessionalCreateViewModel ToProfessionalCreateViewModel(Professional professional)
        {
            return new ProfessionalCreateViewModel
            {
                Name = professional.Name,
                UserId = professional.UserId,
                PhotoPath = professional.PhotoPath,
                Specialty = professional.Specialty,
                DefaultCommission = professional.DefaultCommission,
                IsActive = professional.IsActive
            };
        }

        public ProfessionalEditViewModel ToProfessionalEditViewModel(Professional professional)
        {
            return new ProfessionalEditViewModel
            {
                ProfessionalId = professional.ProfessionalId,
                Name = professional.Name,
                UserId = professional.UserId,
                PhotoPath = professional.PhotoPath,
                Specialty = professional.Specialty,
                DefaultCommission = professional.DefaultCommission,
                IsActive = professional.IsActive
            };
        }

        // Professional Profile mappings
        public ProfessionalProfileViewModel ToProfessionalProfileViewModel(Professional professional)
        {
            return new ProfessionalProfileViewModel
            {
                ProfessionalId = professional.ProfessionalId,
                Name = professional.Name,
                UserId = professional.UserId,
                PhotoPath = professional.PhotoPath,
                Specialty = professional.Specialty,
                DefaultCommission = professional.DefaultCommission,
                ProfessionalServices = professional.ProfessionalServices?.ToList() ?? new List<ProfessionalService>(),
                Appointments = professional.Appointments?.ToList() ?? new List<Appointment>(),
                Schedules = professional.Schedules?.ToList() ?? new List<Schedule>()
            };
        }

        public ProfessionalProfileEditViewModel ToProfessionalProfileEditViewModel(Professional professional)
        {
            return new ProfessionalProfileEditViewModel
            {
                ProfessionalId = professional.ProfessionalId,
                Name = professional.Name,
                PhotoPath = professional.PhotoPath,
                Specialty = professional.Specialty,
                DefaultCommission = professional.DefaultCommission
            };
        }

        public Professional ToProfessional(ProfessionalProfileEditViewModel model)
        {
            return new Professional
            {
                ProfessionalId = model.ProfessionalId,
                Name = model.Name,
                PhotoPath = model.PhotoPath,
                Specialty = model.Specialty,
                DefaultCommission = model.DefaultCommission
            };
        }
    }
}
