using System.ComponentModel.DataAnnotations;
using Sistema.Data.Entities;

namespace Sistema.Models.Admin
{
    public class AdminProfessionalViewModel
    {
        public int ProfessionalId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "User is required")]
        [Display(Name = "User")]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "User Email")]
        public string? UserEmail { get; set; }

        [Display(Name = "Foto do Profissional")]
        public string? ImageId { get; set; }

        [Required(ErrorMessage = "Specialty is required")]
        [StringLength(100, ErrorMessage = "Specialty cannot exceed 100 characters")]
        [Display(Name = "Specialty")]
        public string Specialty { get; set; } = string.Empty;

        [Display(Name = "Default Commission")]
        [Range(0, 100, ErrorMessage = "Commission must be between 0 and 100")]
        public decimal DefaultCommission { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties for display
        public List<ProfessionalService>? ProfessionalServices { get; set; }
        public List<Appointment>? Appointments { get; set; }
        public List<Schedule>? Schedules { get; set; }

        // Computed properties
        public int TotalServices => ProfessionalServices?.Count ?? 0;
        public int TotalAppointments => Appointments?.Count ?? 0;
        public int ActiveAppointments => Appointments?.Count(a => a.Date >= DateTime.Today && a.Status != "Cancelado") ?? 0;
        public int CompletedAppointments => Appointments?.Count(a => a.Status == "Concluído") ?? 0;
    }
}
