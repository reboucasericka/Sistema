using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Sistema.Models.Admin
{
    public class AdminProfessionalEditViewModel
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
        public IFormFile? file { get; set; }

        public string? ImageId { get; set; }

        [Required(ErrorMessage = "Specialty is required")]
        [StringLength(100, ErrorMessage = "Specialty cannot exceed 100 characters")]
        [Display(Name = "Specialty")]
        public string Specialty { get; set; } = string.Empty;

        [Required(ErrorMessage = "Default Commission is required")]
        [Range(0, 100, ErrorMessage = "Commission must be between 0 and 100")]
        [Display(Name = "Default Commission (%)")]
        public decimal DefaultCommission { get; set; }

        [Display(Name = "Active Professional")]
        public bool IsActive { get; set; } = true;
    }
}
