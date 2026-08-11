using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Sistema.Models.Admin
{
    public class AdminProfessionalCreateViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Usuário Existente")]
        public string? ExistingUserId { get; set; }

        [Display(Name = "Email para Novo Usuário")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string? Email { get; set; }

        [Display(Name = "Senha para Novo Usuário")]
        [StringLength(100, ErrorMessage = "A senha deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
        public string? Password { get; set; }

        [Display(Name = "Foto do Profissional")]
        public IFormFile? PhotoFile { get; set; }

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
