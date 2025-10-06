using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("BeautyCenters")]
    public class BeautyCenter
    {
        [Key]
        public int BeautyCenterId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, StringLength(200)]
        public string Address { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        public bool IsActive { get; set; } = true;

        public bool ExportedToExcel { get; set; } = false;
        public bool ExportedToPdf { get; set; } = false;
    }
}