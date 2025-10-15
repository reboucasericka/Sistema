using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Customers")]
    public class Customer : IEntity
    {
        [Key]
        public int CustomerId { get; set; }       
        

        [Required, StringLength(100)]
        public string Name { get; set; } // Nome do cliente

        [StringLength(100)]
        public string? Email { get; set; } // Email do cliente

        [StringLength(20)]
        public string? Phone { get; set; } // Telemóvel do cliente

        [StringLength(200)]
        public string? Address { get; set; } // Endereço do cliente

        [Column(TypeName = "date")] // Usar date para armazenar apenas a data
        public DateTime? BirthDate { get; set; } // Data de nascimento do cliente

        [Column(TypeName = "datetime2")] // Usar datetime2 para maior precisão
        public DateTime RegistrationDate { get; set; } = DateTime.Now; // Data de registo do cliente

        public bool IsActive { get; set; } = true; // Indica se o cliente está ativo

        public string? Notes { get; set; } // Notas adicionais sobre o cliente

        [StringLength(255)]
        public string? AllergyHistory { get; set; } // Histórico de alergias

        public Guid ImageId { get; set; } // Foto do cliente (ID da imagem no blob)

        // 🔗 FK → User
        [ForeignKey("User")]
        public string? UserId { get; set; }
        public User? User { get; set; }

        // Navigation properties
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Propriedade calculada para o caminho completo da imagem
        public string ImageFullPath => ImageId == Guid.Empty
            ? $"https://supershoptpsi-ftc4dnb4bcbkgmhw.westeurope-01.azurewebsites.net/images/noimage.png"
            : $"https://supershopcontaarmazename.blob.core.windows.net/customers/{ImageId}.png";
    }
}