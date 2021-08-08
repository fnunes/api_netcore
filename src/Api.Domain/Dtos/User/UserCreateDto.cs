using System.ComponentModel.DataAnnotations;

namespace Api.Domain.Dtos.User
{
    public class UserCreateDto
    {
        [Required(ErrorMessage = "Nome é um campo obrigatório")]
        [StringLength(60, ErrorMessage = "O máximo de caracterers para nome é {1}")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email é um campo obrigatório")]
        [EmailAddress(ErrorMessage = "O email informado não possoui um formato válido")]
        [StringLength(100, ErrorMessage = "O máximo de caracterers para email é {1}")]
        public string Email { get; set; }
    }
}
