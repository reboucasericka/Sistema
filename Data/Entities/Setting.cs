using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Settings")]
    public class Setting
    {
        [Key]
        public int SettingId { get; set; }

        [Required, StringLength(100)]
        public string ClinicName { get; set; }

        [Required, StringLength(100)]
        public string Email { get; set; }

        [StringLength(20)]
        public string? LandlinePhone { get; set; }

        [StringLength(20)]
        public string? WhatsAppPhone { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(200)]
        public string? Logo { get; set; }

        [StringLength(200)]
        public string? Icon { get; set; }

        [StringLength(200)]
        public string? ReportLogo { get; set; }

        [StringLength(20)]
        public string? ReportType { get; set; } // PDF, Excel

        [StringLength(200)]
        public string? Instagram { get; set; }

        [StringLength(25)]
        public string CommissionType { get; set; } = "%";

        [Column(TypeName = "decimal(10,2)")]
        public decimal DefaultExtensionCommission { get; set; } = 20.00m;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DefaultDesignCommission { get; set; } = 15.00m;

        [StringLength(200)]
        public string ImagesFolder { get; set; } = "uploads/";

        [StringLength(50)]
        public string BusinessHours { get; set; } = "09:00-18:00";

        public int DefaultServiceDuration { get; set; } = 60;
    }
}