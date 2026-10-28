using System.ComponentModel.DataAnnotations;

namespace SIGEMIC.ViewModels;

public class PerfilAdministradorViewModel
{
    public uint IdUsuario { get; set; }

    public string Nombres { get; set; }
        = string.Empty;

    public string Apellidos { get; set; }
        = string.Empty;

    public string Username { get; set; }
        = string.Empty;

    public string Rol { get; set; }
        = string.Empty;

    public string Estado { get; set; }
        = string.Empty;

    [Required(
        ErrorMessage =
            "El correo electrónico es obligatorio."
    )]
    [EmailAddress(
        ErrorMessage =
            "Ingrese un correo electrónico válido."
    )]
    [StringLength(150)]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; }
        = string.Empty;
}