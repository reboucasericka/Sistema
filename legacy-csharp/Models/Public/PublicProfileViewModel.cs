using System.ComponentModel.DataAnnotations;
using Sistema.Data.Entities;
using Microsoft.AspNetCore.Http;

namespace Sistema.Models.Public
{
    public class PublicCustomerProfileViewModel
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone format")]
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [StringLength(255, ErrorMessage = "Allergy history cannot exceed 255 characters")]
        [Display(Name = "Allergy History")]
        public string? AllergyHistory { get; set; }

        [Display(Name = "Foto do Cliente")]
        public string? ImageId { get; set; }

        // Navigation properties
        public List<Appointment>? Appointments { get; set; }
        public List<Appointment>? UpcomingAppointments { get; set; }
        public List<Appointment>? PastAppointments { get; set; }
        
        // Statistics
        public int TotalAppointments => Appointments?.Count ?? 0;
        public int UpcomingCount => UpcomingAppointments?.Count ?? 0;
        public int PastCount => PastAppointments?.Count ?? 0;
    }

    public class PublicCustomerProfileEditViewModel
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone format")]
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        public DateTime? BirthDate { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [StringLength(255, ErrorMessage = "Allergy history cannot exceed 255 characters")]
        [Display(Name = "Allergy History")]
        public string? AllergyHistory { get; set; }

        [Display(Name = "Foto do Cliente")]
        public IFormFile? file { get; set; }

        public string? ImageId { get; set; }
    }

    public class PublicCustomerDashboardViewModel
    {
        public PublicCustomerProfileViewModel Profile { get; set; } = new();
        public List<Appointment> RecentAppointments { get; set; } = new();
        public List<Appointment> UpcomingAppointments { get; set; } = new();
        public List<Service> AvailableServices { get; set; } = new();
        public List<Professional> AvailableProfessionals { get; set; } = new();
        
        // Statistics
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int PendingAppointments { get; set; }
    }
}
