using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Configuracoes")]
    public class Configuracao
    {
        [Key]
        public int ConfiguracaoId { get; set; }

        [Required, StringLength(100)]
        public string NomeClinica { get; set; }

        [Required, StringLength(100)]
        public string Email { get; set; }

        [StringLength(20)]
        public string? TelefoneFixo { get; set; }

        [StringLength(20)]
        public string? TelemovelWhatsApp { get; set; }

        [StringLength(200)]
        public string? Endereco { get; set; }

        [StringLength(200)]
        public string? Logo { get; set; }

        [StringLength(200)]
        public string? Icone { get; set; }

        [StringLength(200)]
        public string? LogoRelatorio { get; set; }

        [StringLength(20)]
        public string? TipoRelatorio { get; set; } // PDF, Excel

        [StringLength(200)]
        public string? Instagram { get; set; }

        [StringLength(25)]
        public string TipoComissao { get; set; } = "%";

        [Column(TypeName = "decimal(10,2)")]
        public decimal ComissaoPadraoExtensao { get; set; } = 20.00m;

        [Column(TypeName = "decimal(10,2)")]
        public decimal ComissaoPadraoDesign { get; set; } = 15.00m;

        [StringLength(200)]
        public string PastaImagens { get; set; } = "uploads/";

        [StringLength(50)]
        public string HorarioFuncionamento { get; set; } = "09:00-18:00";

        public int DuracaoPadraoServico { get; set; } = 60;
    }
}