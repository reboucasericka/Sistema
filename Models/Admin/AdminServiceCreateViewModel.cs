using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Models.Admin
{
    public class AdminServiceCreateViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name must not exceed 100 characters")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [StringLength(250, ErrorMessage = "Description must not exceed 250 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [StringLength(100, ErrorMessage = "Duration must not exceed 100 characters")]
        [Display(Name = "Duration")]
        public string? Duration { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Photo")]
        public IFormFile? file { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Categories")]
        public IEnumerable<SelectListItem>? Categories { get; set; }
    }
}
