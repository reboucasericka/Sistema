using System.ComponentModel.DataAnnotations;

namespace Sistema.Models.Admin
{
    public class AdminCreateAppointmentViewModel
    {
        [Required(ErrorMessage = "O serviço é obrigatório")]
        [Display(Name = "Serviço")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "O profissional é obrigatório")]
        [Display(Name = "Profissional")]
        public int ProfessionalId { get; set; }

        [Required(ErrorMessage = "A data é obrigatória")]
        [Display(Name = "Data")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "O horário é obrigatório")]
        [Display(Name = "Horário")]
        [DataType(DataType.Time)]
        public TimeSpan Time { get; set; }

        [Display(Name = "Observações")]
        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        public string? Notes { get; set; }

        [Display(Name = "Cliente")]
        public int? CustomerId { get; set; }

        [Display(Name = "Nome do Cliente")]
        public string? CustomerName { get; set; }

        [Display(Name = "Telefone do Cliente")]
        public string? CustomerPhone { get; set; }

        [Display(Name = "Email do Cliente")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string? CustomerEmail { get; set; }
    }
}
