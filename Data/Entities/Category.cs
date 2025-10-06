using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Category")]
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        public ICollection<Service> Service { get; set; } = new List<Service>();
    }
}
