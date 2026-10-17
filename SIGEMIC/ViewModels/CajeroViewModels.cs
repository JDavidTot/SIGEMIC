using System.ComponentModel.DataAnnotations;

namespace SIGEMIC.ViewModels;

public class CrearCajeroViewModel
{
    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100)]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100)]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; } = string.Empty;

    [Required]
    [RegularExpression("ACTIVO|INACTIVO")]
    public string Estado { get; set; } = "ACTIVO";

    public List<PermisoSeleccionViewModel> Permisos { get; set; } = new();
}

public class EditarCajeroViewModel
{
    public uint IdUsuario { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombres { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Apellidos { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
    public string Correo { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string? NuevaContrasena { get; set; }

    [Required]
    [RegularExpression("ACTIVO|INACTIVO")]
    public string Estado { get; set; } = "ACTIVO";
}

public class PermisosCajeroViewModel
{
    public uint IdUsuario { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public List<PermisoSeleccionViewModel> Permisos { get; set; } = new();
}

public class PermisoSeleccionViewModel
{
    public uint IdPermiso { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Modulo { get; set; } = string.Empty;
    public bool Seleccionado { get; set; }
}