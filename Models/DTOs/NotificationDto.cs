using System.ComponentModel.DataAnnotations;

namespace SistemaAPI.DTOs
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "Info"; // Info, Warning, Error, Success
        public bool IsRead { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ReadAt { get; set; }
        public string? UserId { get; set; }
        public string? Link { get; set; }
        public string? Icon { get; set; }
    }
}
