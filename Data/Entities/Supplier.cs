using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Fornecedores")]
    public class Supplier : IEntity
    {
        [Key]
        public int SupplierId { get; set; } //FornecedorId

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } //Nome

        [Required]
        [MaxLength(100)]
        public string? Phone { get; set; } //Telefone

        [MaxLength(100)]
        public string? Email { get; set; } //Email

        [MaxLength(20)]
        public string? Nif { get; set; } //NIF

        [MaxLength(200)]
        public string? Address { get; set; } //Morada

        public int? DeliveryTime { get; set; } //PrazoEntrega (em dias)

        public string? Notes { get; set; } //Notas

        [Column(TypeName = "datetime2")] // Para precisão maior que datetime padrão
        public DateTime RegistrationDate { get; set; } = DateTime.Now; //DataRegisto

        // 🔗 relação 1:N (um fornecedor pode fornecer vários produtos)
        public ICollection<Product> Products { get; set; } //Produtos fornecidos


        // ✅ Implementação da interface IEntity
        public int Id
        {
            get => SupplierId; //retorna o valor da PK
            set => SupplierId = value; //define o valor da PK
        }
    }
}