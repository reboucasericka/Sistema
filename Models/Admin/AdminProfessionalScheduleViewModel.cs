using System.ComponentModel.DataAnnotations;
using Sistema.Data.Entities;

namespace Sistema.Models.Admin
{
    public class AdminProfessionalScheduleViewModel
    {
        public int ScheduleId { get; set; }

        [Required(ErrorMessage = "O profissional é obrigatório")]
        [Display(Name = "Profissional")]
        public int ProfessionalId { get; set; }

        [Required(ErrorMessage = "O dia da semana é obrigatório")]
        [Display(Name = "Dia da Semana")]
        public DayOfWeek DayOfWeek { get; set; }

        [Required(ErrorMessage = "A hora de início é obrigatória")]
        [Display(Name = "Hora de Início")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "A hora de fim é obrigatória")]
        [Display(Name = "Hora de Fim")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        // Navigation properties for display
        public Professional? Professional { get; set; }

        // Computed properties
        public string DayName => GetDayName(DayOfWeek);
        public string TimeRange => $"{StartTime.ToString(@"hh\:mm")} - {EndTime.ToString(@"hh\:mm")}";

        private string GetDayName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => "Domingo",
                DayOfWeek.Monday => "Segunda-feira",
                DayOfWeek.Tuesday => "Terça-feira",
                DayOfWeek.Wednesday => "Quarta-feira",
                DayOfWeek.Thursday => "Quinta-feira",
                DayOfWeek.Friday => "Sexta-feira",
                DayOfWeek.Saturday => "Sábado",
                _ => dayOfWeek.ToString()
            };
        }
    }
}
