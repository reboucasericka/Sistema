using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("ServiceCategories")]
    public class ServiceCategories
    {
        [Key]
        public int ServiceCategoriesId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        // 🔗 Relação 1:N → Uma categoria pode ter vários serviços
        public ICollection<Service> Services { get; set; }
    }
}