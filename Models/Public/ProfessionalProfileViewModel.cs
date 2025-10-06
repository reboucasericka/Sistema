using System.ComponentModel.DataAnnotations;
using Sistema.Data.Entities;

namespace Sistema.Models.Public
{
    public class ProfessionalProfileViewModel
    {
        public int ProfessionalId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "User is required")]
        [Display(Name = "User")]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Foto do Profissional")]
        public string? PhotoPath { get; set; }

        [Required(ErrorMessage = "Specialty is required")]
        [StringLength(100, ErrorMessage = "Specialty cannot exceed 100 characters")]
        [Display(Name = "Specialty")]
        public string Specialty { get; set; } = string.Empty;

        [Display(Name = "Default Commission")]
        [Range(0, 100, ErrorMessage = "Commission must be between 0 and 100")]
        public decimal DefaultCommission { get; set; }

        // Navigation properties
        public List<ProfessionalService> ProfessionalServices { get; set; } = new List<ProfessionalService>();
        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
        public List<Schedule> Schedules { get; set; } = new List<Schedule>();

        // Computed properties
        public int TotalServices => ProfessionalServices?.Count ?? 0;
        public int TotalAppointments => Appointments?.Count ?? 0;
        public int ActiveAppointments => Appointments?.Count(a => a.Date >= DateTime.Today && a.Status != "Cancelado") ?? 0;
        public int CompletedAppointments => Appointments?.Count(a => a.Status == "Concluído") ?? 0;
        public int TodayAppointments => Appointments?.Count(a => a.StartTime.Date == DateTime.Today) ?? 0;
        public int UpcomingAppointments => Appointments?.Count(a => a.Date > DateTime.Today && a.Status != "Cancelado") ?? 0;
    }

    public class ProfessionalProfileEditViewModel
    {
        public int ProfessionalId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Foto do Profissional")]
        public string? PhotoPath { get; set; }

        [Required(ErrorMessage = "Specialty is required")]
        [StringLength(100, ErrorMessage = "Specialty cannot exceed 100 characters")]
        [Display(Name = "Specialty")]
        public string Specialty { get; set; } = string.Empty;

        [Display(Name = "Default Commission")]
        [Range(0, 100, ErrorMessage = "Commission must be between 0 and 100")]
        public decimal DefaultCommission { get; set; }
    }

    public class ProfessionalDashboardViewModel
    {
        public ProfessionalProfileViewModel Profile { get; set; } = new();
        public List<Appointment> TodayAppointments { get; set; } = new();
        public List<Appointment> UpcomingAppointments { get; set; } = new();
        public List<Appointment> RecentAppointments { get; set; } = new();
        public List<Service> AvailableServices { get; set; } = new();
        public List<Schedule> WeeklySchedule { get; set; } = new();
        
        // Statistics
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal MonthlyEarnings { get; set; }
    }
}
