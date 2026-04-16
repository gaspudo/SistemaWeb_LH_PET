using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LH_PET_WEB.Models
{
    [Table("tb_produto")]
    public class Produto
    {
        [Key]
        [Column("pk_produto")]
        public int Id { get; set; }

        [Required(ErrorMessage ="O nome do produto é obrigatório.")]
        [MaxLength(255)]
        [Column("nm_produto")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage ="O preço do produto é obrigatório.")]
        [Column("vl_preco")]
        public decimal Preco { get; set; }

        [Required]
        [Column("vl_estoque")]
        public int Estoque { get; set; } = 0;

        [Column("ds_imagem_url")]
        public string? ImagemUrl { get; set; }
    }
}