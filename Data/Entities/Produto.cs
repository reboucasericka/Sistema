using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Produtos")]
    public class Produto : IEntity
    {
        [Key]
        public int ProdutoId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nome { get; set; }

        [MaxLength(255)]
        public string? Descricao { get; set; }

        // Implementação da interface IEntity
        public int Id
        {
            get => ProdutoId;
            set => ProdutoId = value;
        }

        // 🔗 FK → CategoriasProdutos
        [ForeignKey("CategoriaProduto")]
        public int CategoriaProdutoId { get; set; }
        public CategoriaProduto CategoriaProduto { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal ValorCompra { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal ValorVenda { get; set; }

        public int Estoque { get; set; } = 0;

        [MaxLength(200)]
        [Display(Name = "Foto do Produto")]
        public string? Foto { get; set; }  //ImageProductFile

        public int NivelEstoqueMinimo { get; set; } = 0;

        // 🔗 FK → Fornecedores (opcional)
        [ForeignKey("Fornecedor")]
        public int? FornecedorId { get; set; }
        public Fornecedor? Fornecedor { get; set; }

        public Usuario Usuario { get; set; }

        public string ImagemFullPath
        {
            get
            {
                if (string.IsNullOrEmpty(Foto))
                {
                    return null;
                }
                return $"https://localhost:7183/images/noimage.png";
                return $"https://localhost:7183/{Foto.Substring(1)}";
            }
        }
    }
}
