using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace LH_PET_WEB.Models
{
    [Table("tb_agendamento")]
    public class Agendamento
    {
        [Key]
        [Column("pk_agendamento")]
        public int Id { get; set; }

        [Required]
        [Column("fk_pet")]
        public int PetId { get; set; }

        [Required(ErrorMessage = "O campo 'Data e Hora' são obrigatório.")]
        [Column("dt_data_hora")]
        public DateTime DataHora { get; set; }

        [Required(ErrorMessage = "O campo 'Serviço' é obrigatório.")]
        [MaxLength(50)]
        [Column("ds_tipo")]
        public string Tipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo 'Status' é obrigatório.")]
        [MaxLength(50)]
        [Column("ds_status")]
        public string Status { get; set; } = "Pendente";

        // Relacionamentos
        [ForeignKey("PetId")]
        public Pet? Pet { get; set; }

        public Atendimento? Atendimento { get; set; }
    }
}