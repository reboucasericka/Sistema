using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("CategoriasServicos")]
    public class CategoriaServico
    {
        [Key]
        public int CategoriaServicoId { get; set; }

        [Required, StringLength(100)]
        public string Nome { get; set; }

        // 🔗 Relação 1:N → Uma categoria pode ter vários serviços
        public ICollection<Servico> Servicos { get; set; }
    }
}