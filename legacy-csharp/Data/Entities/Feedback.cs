using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    public class Feedback : IEntity
    {
        public int FeedbackId { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; }

        [Required]
        public int ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual Customer Client { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "A avaliação deve estar entre 1 e 5 estrelas")]
        public int Rating { get; set; }

        [MaxLength(300, ErrorMessage = "O comentário não pode exceder 300 caracteres")]
        public string? Comment { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        // Propriedades de navegação adicionais
        public virtual Service? Service => Appointment?.Service;
        public virtual Professional? Professional => Appointment?.Professional;
    }
}
