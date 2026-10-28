using System.ComponentModel.DataAnnotations;

namespace SIGEMIC.ViewModels;

// =====================================================
// CREAR USUARIO
// ADMINISTRADOR O CAJERO
// =====================================================

public class CrearUsuarioViewModel
{
    [Required(ErrorMessage = "Ingrese los nombres.")]
    [StringLength(100)]
    [Display(Name = "Nombres")]
    public string Nombres { get; set; } = string.Empty;


    [Required(ErrorMessage = "Ingrese los apellidos.")]
    [StringLength(100)]
    [Display(Name = "Apellidos")]
    public string Apellidos { get; set; } = string.Empty;


    [Required(ErrorMessage = "Ingrese el nombre de usuario.")]
    [StringLength(50)]
    [Display(Name = "Usuario")]
    public string Username { get; set; } = string.Empty;


    [Required(ErrorMessage = "Ingrese el correo electrónico.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    [StringLength(150)]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;


    [Required(ErrorMessage = "Ingrese una contraseña.")]
    [StringLength(
        100,
        MinimumLength = 8,
        ErrorMessage = "La contraseña debe tener al menos 8 caracteres."
    )]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = string.Empty;


    [Required(ErrorMessage = "Seleccione el rol.")]
    [Display(Name = "Rol")]
    public string Rol { get; set; } = string.Empty;


    [Required]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = "ACTIVO";


    public List<PermisoSeleccionViewModel> Permisos { get; set; }
        = new();
}


// =====================================================
// EDITAR USUARIO
// =====================================================

public class EditarUsuarioViewModel
{
    public uint IdUsuario { get; set; }


    [Required(ErrorMessage = "Ingrese los nombres.")]
    [StringLength(100)]
    [Display(Name = "Nombres")]
    public string Nombres { get; set; } = string.Empty;


    [Required(ErrorMessage = "Ingrese los apellidos.")]
    [StringLength(100)]
    [Display(Name = "Apellidos")]
    public string Apellidos { get; set; } = string.Empty;


    [Required(ErrorMessage = "Ingrese el nombre de usuario.")]
    [StringLength(50)]
    [Display(Name = "Usuario")]
    public string Username { get; set; } = string.Empty;


    [Required(ErrorMessage = "Ingrese el correo electrónico.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    [StringLength(150)]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;


    [Display(Name = "Nueva contraseña")]
    [DataType(DataType.Password)]
    public string? NuevaContrasena { get; set; }


    [Required]
    [Display(Name = "Rol")]
    public string Rol { get; set; } = string.Empty;


    [Required]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = "ACTIVO";
}


// =====================================================
// PERMISOS DE CAJERO
// =====================================================

public class PermisosCajeroViewModel
{
    public uint IdUsuario { get; set; }

    public string NombreCompleto { get; set; }
        = string.Empty;

    public string Username { get; set; }
        = string.Empty;

    public List<PermisoSeleccionViewModel> Permisos { get; set; }
        = new();
}


// =====================================================
// PERMISO SELECCIONABLE
// =====================================================

public class PermisoSeleccionViewModel
{
    public uint IdPermiso { get; set; }

    public string Codigo { get; set; }
        = string.Empty;

    public string Nombre { get; set; }
        = string.Empty;

    public string? Descripcion { get; set; }

    public string Modulo { get; set; }
        = string.Empty;

    public bool Seleccionado { get; set; }
}