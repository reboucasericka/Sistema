using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    public class PriceTable : IEntity
    {
        [Key]
        [Column("PriceId")]   // nome real da coluna no banco
        public int PriceId { get; set; }  

        public string? Category { get; set; }
        public string? ServiceName { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
    }

}
