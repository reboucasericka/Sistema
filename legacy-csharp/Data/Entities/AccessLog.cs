using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("AccessLog")]
    public class AccessLog
    {
        [Key]
        public int AccessLogId { get; set; }

        // 🔗 FK → User
        public string UserId { get; set; }
        public User User { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateTime { get; set; } = DateTime.Now; // DataHora

        [Required, StringLength(100)]
        public string Action { get; set; }

        [StringLength(50)]
        public string? Role { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime Timestamp { get; set; } = DateTime.Now; // Alias para DateTime

        [StringLength(500)]
        public string? Details { get; set; } // Detalhes adicionais da ação
    }
}