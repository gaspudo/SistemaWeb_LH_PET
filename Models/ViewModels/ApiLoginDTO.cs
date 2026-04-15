using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using LH_PET_WEB.Validations;

namespace LH_PET_WEB.Models.ViewModels
{
    public class ApiLoginDTO
    {
        [Required(ErrorMessage = "O E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O E-mail deve ser um endereço de e-mail válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        public string Senha { get; set; } = string.Empty;
    }

    public class ApiRegistroDTO 
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MinLength(3, ErrorMessage = "O nome deve conter pelo menos 3 caracteres.")]
        public string Nome { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [Cpf(ErrorMessage = "O CPF informado é inválido.")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [RegularExpression(@"^\(?\d{2}\)?[\s-]?\d{4,5}-?\d{4}$", ErrorMessage = "O telefone está no formato errado.")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "O E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O E-mail deve ser um endereço de e-mail válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W ]).{8,}$", ErrorMessage = "A senha é muito fraca.")]
        public string Senha { get; set; } = string.Empty;
    }

    public class ApiPetDTO 
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string Especie { get; set; } = string.Empty;

        [Required]
        public string Raca { get; set; } = string.Empty;

        [Required]
        public DateTime DataNascimento { get; set; }
    }

    public class ApiAgendamentoDTO 
    {
        [Required]
        public int PetId { get; set; }

        [Required]
        public DateTime DataHora { get; set; }

        [Required]
        public string Tipo { get; set; } = string.Empty;
    }

    public class ApiPerfilUpdateDTO
    {
        [Required]
        [MinLength(3, ErrorMessage = "O nome deve conter pelo menos 3 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\(?\d{2}\)?[\s-]?\d{4,5}-?\d{4}$", ErrorMessage = "O telefone está no formato errado.")]
        public string Telefone { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "O E-mail deve ser um endereço de e-mail válido.")]
        public string Email { get; set; } = string.Empty;
    }
}