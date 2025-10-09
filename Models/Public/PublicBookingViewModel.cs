using System.ComponentModel.DataAnnotations;

namespace Sistema.Models.Public
{
    public class PublicBookingViewModel
    {
        public int ServiceId { get; set; }

        [Display(Name = "Serviço")]
        public string ServiceName { get; set; }

        [Display(Name = "Preço")]
        public decimal Price { get; set; }

        [Display(Name = "Profissional")]
        public string? ProfessionalName { get; set; }

        [Required(ErrorMessage = "A data é obrigatória")]
        [Display(Name = "Data")]
        public DateTime SelectedDate { get; set; }

        [Required(ErrorMessage = "O horário é obrigatório")]
        [Display(Name = "Horário")]
        public string SelectedTime { get; set; }

        [Display(Name = "Duração")]
        public string? Duration { get; set; }

        [Display(Name = "Descrição")]
        public string? Description { get; set; }

        [Display(Name = "Foto")]
        public string? ImageId { get; set; }

        // Propriedades adicionais para compatibilidade
        public int ProfessionalId { get; set; }
        public List<string> AvailableSlots { get; set; } = new List<string>();
    }
}