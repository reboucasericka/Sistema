using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("PlanoAgendamentos")]
    public class PlanoAgendamento
    {
        [Key]
        public int PlanoAgendamentoId { get; set; }

        // 🔗 FK → Plano
        public int PlanoId { get; set; }
        public Plano Plano { get; set; }

        // 🔗 FK → Agendamento
        public int AgendamentoId { get; set; }
        public Appointment Agendamento { get; set; }

        public bool SessaoUsada { get; set; } = false;
    }
}