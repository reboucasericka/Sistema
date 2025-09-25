using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("FormasPagamento")]
    public class FormaPagamento
    {
        [Key]
        public int FormaPagamentoId { get; set; }

        [Required, StringLength(50)]
        public string Descricao { get; set; }

        public bool Ativo { get; set; } = true;

        // 🔗 Relações
        public ICollection<Pagar> Pagar { get; set; }
        public ICollection<Receber> Receber { get; set; }
    }
}