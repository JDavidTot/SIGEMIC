using System.ComponentModel.DataAnnotations;

namespace SIGEMIC.ViewModels;

public class OlvidePasswordViewModel
{
    [Required(
        ErrorMessage =
            "Ingrese su nombre de usuario."
    )]
    [Display(Name = "Usuario")]
    public string Usuario { get; set; }
        = string.Empty;
}


public class RestablecerPasswordViewModel
{
    [Required]
    public string Token { get; set; }
        = string.Empty;


    [Required(
        ErrorMessage =
            "Ingrese la nueva contraseña."
    )]
    [StringLength(
        100,
        MinimumLength = 8,
        ErrorMessage =
            "La contraseña debe tener al menos 8 caracteres."
    )]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string NuevaContrasena { get; set; }
        = string.Empty;


    [Required(
        ErrorMessage =
            "Confirme la nueva contraseña."
    )]
    [DataType(DataType.Password)]
    [Compare(
        nameof(NuevaContrasena),
        ErrorMessage =
            "Las contraseñas no coinciden."
    )]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarContrasena { get; set; }
        = string.Empty;
}