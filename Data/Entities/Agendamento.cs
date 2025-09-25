using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Agendamentos")]
    public class Agendamento
    {
        [Key]
        public int AgendamentoId { get; set; }

        // 🔗 FK → Cliente
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        // 🔗 FK → Servico
        public int ServicoId { get; set; }
        public Servico Servico { get; set; }

        // 🔗 FK → Profissional
        public int ProfissionalId { get; set; }
        public Profissional Profissional { get; set; }

        [Column(TypeName = "date")]
        public DateTime Data { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan Horario { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "agendado";

        public string? Observacoes { get; set; }

        public bool LembreteEnviado { get; set; } = false;
        public bool ExportadoExcel { get; set; } = false;
        public bool ExportadoPdf { get; set; } = false;

        // 🔗 Relação 1:N → um agendamento pode ter vários lembretes
        public ICollection<Lembrete> Lembretes { get; set; }
    }
}