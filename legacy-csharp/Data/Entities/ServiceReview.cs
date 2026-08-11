using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("ServiceReviews")] // Changed from "AvaliacoesAtendimento" to "ServiceReviews"
    public class ServiceReview
    {
        [Key]
        public int ReviewId { get; set; } // Changed from AvaliacaoId to ReviewId

        // 🔗 FK → Client
        public int CustomerId { get; set; } // Changed from ClienteId to CustomerId
        public Customer Client { get; set; } // Changed from Cliente to Client

        // 🔗 FK → Service
        public int ServiceId { get; set; } // Changed from ServicoId to ServiceId
        public Service Service { get; set; } // Changed from Servico to Service

        [Column(TypeName = "datetime2")]
        public DateTime ReviewDate { get; set; } = DateTime.Now; // Changed from DataAvaliacao to ReviewDate

        [Range(1, 5)]        
        public int Rating { get; set; } // Nota (1 a 5 estrelas)

        [MaxLength(500)]
        public string? Comments { get; set; } // Comentário opcional

    }
}