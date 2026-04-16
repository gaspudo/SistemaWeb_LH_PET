using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LH_PET_WEB.Models;

namespace LH_PET_WEB.Models
{
    [Table("tb_cliente")]
    public class Cliente
    {
        [Key]
        [Column("pk_cliente")]
        public int Id { get; set; }

        [Required]
        [Column("fk_usuario")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [MaxLength(255)]
        [Column("nm_cliente")]
        public string Nome { get; set; } = "usuário";

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [MaxLength(14)]
        [Column("cd_cpf")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Telefone é obrigatório.")]
        [MaxLength(20)]
        [Column("cd_telefone")]
        public string Telefone { get; set; } = string.Empty;

        //Relacionamentos 
        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        public ICollection<Pet>? Pets { get; set; } = new List<Pet>();
    }
}