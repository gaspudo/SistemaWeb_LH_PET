using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace SistemaWeb_LH_PET.Models
{
    [Table("tb_pet")]
    public class Pet
    {
        [Key]
        [Column("pk_pet")]
        public int Id {get;set;}

        [Required]
        [Column("fk_cliente")]
        public int ClienteId {get;set;}
        public Cliente? Cliente {get;set;}

        [Required]
        [MaxLength(100)]
        [Column("nm_pet")]
        public string Nome {get;set;} = string.Empty;

        [Required]
        [Column("ds_especie")]
        public string Especie {get;set;} = string.Empty;

        [Required]
        [Column("ds_raca")]
        public string Raca {get;set;} = string.Empty;

        [Required]
        [Column("dt_nascimento")]
        public DateTime DataNascimento {get;set;}

        [NotMapped]
        public int IdadeCalculada 
        {
            get
            {
                var hoje = DateTime.Today;
                var idade = hoje.Year - DataNascimento.Year;
                if (DataNascimento.Date > hoje.AddYears(-idade)) idade--;
                return idade;
            }
        }
    }
}