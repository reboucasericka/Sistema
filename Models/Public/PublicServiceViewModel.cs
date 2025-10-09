using System.ComponentModel.DataAnnotations;

namespace Sistema.Models.Public
{
    public class PublicServiceViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Display(Name = "Duration")]
        public string? Duration { get; set; }

        [Display(Name = "Photo")]
        public string? ImageId { get; set; }

        [Display(Name = "Category")]
        public string CategoryName { get; set; }

        [Display(Name = "Category Id")]
        public int CategoryId { get; set; }
    }
}
