using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("ProfessionalSchedules")]
    public class ProfessionalSchedule
    {
        [Key]
        public int ScheduleId { get; set; }

        [Required]
        [ForeignKey("Professional")]
        public int ProfessionalId { get; set; }
        public Professional Professional { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; } // Segunda, Terça, etc.

        [Required]
        public TimeSpan StartTime { get; set; } // Ex.: 09:00

        [Required]
        public TimeSpan EndTime { get; set; }   // Ex.: 18:00
    }
}