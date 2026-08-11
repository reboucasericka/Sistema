using System.ComponentModel.DataAnnotations;

namespace Sistema.Models.Public
{
    public class CreateAppointmentRequest
    {
        [Required(ErrorMessage = "O profissional é obrigatório.")]
        public int ProfessionalId { get; set; }

        [Required(ErrorMessage = "O serviço é obrigatório.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "A data é obrigatória.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "O horário é obrigatório.")]
        public TimeSpan Time { get; set; }

        [Required(ErrorMessage = "A duração é obrigatória.")]
        [Range(15, 480, ErrorMessage = "A duração deve estar entre 15 e 480 minutos.")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
        public decimal Price { get; set; }

        [StringLength(500, ErrorMessage = "As notas não podem exceder 500 caracteres.")]
        public string? Notes { get; set; }
    }
}
