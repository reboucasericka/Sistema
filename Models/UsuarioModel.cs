using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Models
{
    [Table("usuarios")] // tabela no SQL Server
    public class UsuarioModel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("nome")]
        [StringLength(50)]
        public string Nome { get; set; }

        [Required]
        [Column("email")]
        [StringLength(50)]
        public string Email { get; set; }

        [Column("nif")]
        [StringLength(9)]
        public string NIF { get; set; }

        [Required]
        [Column("senha")]
        [StringLength(100)] 
        public string Senha { get; set; }

        [Required]
        [Column("senha_crip")]
        [StringLength(100)]
        public string SenhaCrip { get; set; }

        [Required]
        [Column("nivel")]
        [StringLength(35)]
        public string Nivel { get; set; }

        [Required]
        [Column("data")]
        public DateTime Data { get; set; }

        [Column("ativo")]
        [StringLength(5)]
        public string Ativo { get; set; } // Vai receber "Sim" ou "Não"

        [Column("telemovel")]
        [StringLength(20)]
        public string Telemovel { get; set; }

        [Column("endereco")]
        [StringLength(100)]
        public string Endereco { get; set; }

        [Column("foto")]
        [StringLength(100)]
        public string Foto { get; set; }
    }
}