using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Fornecedores")]
    public class Fornecedor : IEntity
    {
        [Key]
        public int FornecedorId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Telemovel { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Nif { get; set; }

        [MaxLength(200)]
        public string? Endereco { get; set; }

        public int? PrazoEntrega { get; set; }

        public string? Observacoes { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        // 🔗 relação 1:N (um fornecedor pode fornecer vários produtos)
        public ICollection<Produto> Produtos { get; set; }
        

        // ✅ Implementação da interface IEntity
        public int Id
        {
            get => FornecedorId;
            set => FornecedorId = value;
        }
    }
}