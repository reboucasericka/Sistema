using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(500)]
        public string Message { get; set; }   // Texto da notificação

        [MaxLength(50)]
        public string Type { get; set; } = "info"; // info, warning, error, success

        [MaxLength(100)]
        public string Icon { get; set; }      // Ex: "fas fa-calendar"

        [MaxLength(50)]
        public string Time { get; set; }      // Ex: "3 mins", "12/10 14:00"

        [MaxLength(200)]
        public string Link { get; set; }      // Link de navegação

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ReadAt { get; set; }

        public bool IsRead { get; set; } = false; // Se o admin já viu ou não
    }
}

