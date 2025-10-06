using Microsoft.AspNetCore.Mvc.Rendering;
using Sistema.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Models.Admin
{
    public class ServiceViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [StringLength(250, ErrorMessage = "A descrição deve ter no máximo 250 caracteres")]
        [Display(Name = "Descrição")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        [Display(Name = "Preço")]
        public decimal Price { get; set; }

        [StringLength(100, ErrorMessage = "A duração deve ter no máximo 100 caracteres")]
        [Display(Name = "Duração")]
        public string? Duration { get; set; }

        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Foto")]
        public string? PhotoPath { get; set; }

        [Display(Name = "Foto")]
        public IFormFile? PhotoFile { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória")]
        [Display(Name = "Categoria")]
        public int CategoryId { get; set; }

        [Display(Name = "Categorias")]
        public IEnumerable<SelectListItem>? Categories { get; set; }

        // Campos legados para compatibilidade
        public int ServiceId => Id;
        public int ServiceCategoryId => CategoryId;
        public bool Active => IsActive;
    }
}