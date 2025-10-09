using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Sistema.Models.Admin
{
    public class AdminCustomerEditViewModel
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome não pode exceder 100 caracteres")]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100, ErrorMessage = "Email não pode exceder 100 caracteres")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(20, ErrorMessage = "Telefone não pode exceder 20 caracteres")]
        [Display(Name = "Telefone")]
        public string? Phone { get; set; }

        [StringLength(200, ErrorMessage = "Endereço não pode exceder 200 caracteres")]
        [Display(Name = "Endereço")]
        public string? Address { get; set; }

        [Display(Name = "Data de Nascimento")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Data de Registro")]
        public DateTime RegistrationDate { get; set; }

        [Display(Name = "Cliente Ativo")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Notas")]
        public string? Notes { get; set; }

        [StringLength(255, ErrorMessage = "Histórico de alergias não pode exceder 255 caracteres")]
        [Display(Name = "Histórico de Alergias")]
        public string? AllergyHistory { get; set; }

        [Display(Name = "Foto do Cliente")]
        public IFormFile? file { get; set; }

        public string? ImageId { get; set; }
    }
}
