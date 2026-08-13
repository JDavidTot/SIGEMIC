using System.ComponentModel.DataAnnotations;

namespace SIGEMIC.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ingrese su usuario")]
        [Display(Name = "Usuario")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese su contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;
    }
}