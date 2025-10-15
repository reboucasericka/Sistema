using System;
using Sistema.Data.Entities;
using Sistema.Models.Admin;
using Sistema.Models.Public;

namespace Sistema.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        // Product mappings
        // ViewModel → Entidade
        public Product ToProduct(AdminProductViewModel model, Guid imageId, bool isNew, string? userId = null)
        {
            return new Product
            {
                ProductId = isNew ? 0 : model.ProductId,
                Name = model.Name ?? string.Empty,
                Description = model.Description,
                ProductCategoryId = model.ProductCategoryId,
                PurchasePrice = model.PurchasePrice,
                SalePrice = model.SalePrice,
                Stock = model.Stock,
                ImageId = imageId == Guid.Empty ? null : imageId,
                MinimumStockLevel = model.MinimumStockLevel,
                SupplierId = model.SupplierId > 0 ? model.SupplierId : null, // Garantir que seja null se <= 0
                UserId = userId, // Adicionar UserId
                IsActive = model.IsActive
            };
        }
        
        // Entidade → ViewModel
        public AdminProductViewModel ToProductViewModel(Product product)
        {
            return new AdminProductViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name ?? string.Empty,
                Description = product.Description,
                ProductCategoryId = product.ProductCategoryId,
                PurchasePrice = product.PurchasePrice,
                SalePrice = product.SalePrice,
                Stock = product.Stock,
                MinimumStockLevel = product.MinimumStockLevel,
                SupplierId = product.SupplierId,
                ImageId = product.ImageId,
                IsActive = product.IsActive
            };
        }

        // Customer mappings
        public Customer ToCustomer(AdminCustomerCreateViewModel model)
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
                ImageId = Guid.Parse(model.ImageId ?? Guid.Empty.ToString())
            };
        }

        public Customer ToCustomer(AdminCustomerEditViewModel model)
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
                ImageId = Guid.Parse(model.ImageId ?? Guid.Empty.ToString())
            };
        }

        public AdminCustomerViewModel ToCustomerViewModel(Customer customer)
        {
            return new AdminCustomerViewModel
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
                ImageId = customer.ImageId,
                Appointments = customer.Appointments?.ToList()
            };
        }

        public AdminCustomerCreateViewModel ToCustomerCreateViewModel(Customer customer)
        {
            return new AdminCustomerCreateViewModel
            {
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                BirthDate = customer.BirthDate,
                IsActive = customer.IsActive,
                Notes = customer.Notes,
                AllergyHistory = customer.AllergyHistory,
                ImageId = customer.ImageId.ToString()
            };
        }

        public AdminCustomerEditViewModel ToCustomerEditViewModel(Customer customer)
        {
            return new AdminCustomerEditViewModel
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
                ImageId = customer.ImageId.ToString()
            };
        }

        // Profile mappings
        public PublicCustomerProfileViewModel ToCustomerProfileViewModel(Customer customer)
        {
            var appointments = customer.Appointments?.ToList() ?? new List<Appointment>();
            
            return new PublicCustomerProfileViewModel
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
                ImageId = customer.ImageId.ToString(),
                Appointments = appointments,
                UpcomingAppointments = appointments.Where(a => a.Date >= DateTime.Today && a.Status != "Cancelado").ToList(),
                PastAppointments = appointments.Where(a => a.Date < DateTime.Today).ToList()
            };
        }

        public PublicCustomerProfileEditViewModel ToCustomerProfileEditViewModel(Customer customer)
        {
            return new PublicCustomerProfileEditViewModel
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                BirthDate = customer.BirthDate,
                Notes = customer.Notes,
                AllergyHistory = customer.AllergyHistory,
                ImageId = customer.ImageId.ToString()
            };
        }

        public Customer ToCustomer(PublicCustomerProfileEditViewModel model)
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
                ImageId = Guid.Parse(model.ImageId ?? Guid.Empty.ToString())
            };
        }

        // Professional mappings
        public Professional ToProfessional(AdminProfessionalCreateViewModel model)
        {
            return new Professional
            {
                Name = model.Name,
                UserId = model.ExistingUserId ?? string.Empty,
                ImageId = Guid.Parse(model.ImageId ?? Guid.Empty.ToString()),
                Specialty = model.Specialty,
                DefaultCommission = model.DefaultCommission,
                IsActive = model.IsActive
            };
        }

        public Professional ToProfessional(AdminProfessionalEditViewModel model)
        {
            return new Professional
            {
                ProfessionalId = model.ProfessionalId,
                Name = model.Name,
                UserId = model.UserId,
                ImageId = Guid.Parse(model.ImageId ?? Guid.Empty.ToString()),
                Specialty = model.Specialty,
                DefaultCommission = model.DefaultCommission,
                IsActive = model.IsActive
            };
        }

        public AdminProfessionalViewModel ToProfessionalViewModel(Professional professional)
        {
            return new AdminProfessionalViewModel
            {
                ProfessionalId = professional.ProfessionalId,
                Name = professional.Name,
                UserId = professional.UserId,
                UserEmail = professional.User?.Email,
                ImageId = professional.ImageId.ToString().ToString(),
                Specialty = professional.Specialty,
                DefaultCommission = professional.DefaultCommission,
                IsActive = professional.IsActive,
                ProfessionalServices = professional.ProfessionalServices?.ToList(),
                Appointments = professional.Appointments?.ToList(),
                Schedules = professional.Schedules?.ToList()
            };
        }

        public AdminProfessionalCreateViewModel ToProfessionalCreateViewModel(Professional professional)
        {
            return new AdminProfessionalCreateViewModel
            {
                Name = professional.Name,
                ExistingUserId = professional.UserId,
                ImageId = professional.ImageId.ToString(),
                Specialty = professional.Specialty,
                DefaultCommission = professional.DefaultCommission,
                IsActive = professional.IsActive
            };
        }

        public AdminProfessionalEditViewModel ToProfessionalEditViewModel(Professional professional)
        {
            return new AdminProfessionalEditViewModel
            {
                ProfessionalId = professional.ProfessionalId,
                Name = professional.Name,
                UserId = professional.UserId,
                ImageId = professional.ImageId.ToString().ToString(),
                Specialty = professional.Specialty,
                DefaultCommission = professional.DefaultCommission,
                IsActive = professional.IsActive
            };
        }

        // Professional Profile mappings
        public PublicProfessionalProfileViewModel ToProfessionalProfileViewModel(Professional professional)
        {
            return new PublicProfessionalProfileViewModel
            {
                ProfessionalId = professional.ProfessionalId,
                Name = professional.Name,
                UserId = professional.UserId,
                ImageId = professional.ImageId.ToString().ToString(),
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
                ImageId = professional.ImageId.ToString().ToString(),
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
                ImageId = Guid.Parse(model.ImageId ?? Guid.Empty.ToString()),
                Specialty = model.Specialty,
                DefaultCommission = model.DefaultCommission
            };
        }


        
    }
}
