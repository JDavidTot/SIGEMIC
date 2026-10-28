using System.ComponentModel.DataAnnotations;

namespace SIGEMIC.Models
{
    public class CrearClienteViewModel
    {
        [Required(
            ErrorMessage =
                "Los nombres son obligatorios."
        )]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; }
            = string.Empty;

        [Required(
            ErrorMessage =
                "Los apellidos son obligatorios."
        )]
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; }
            = string.Empty;

        // =====================================================
        // CORREO
        // =====================================================

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

        // =====================================================
        // ACCESO
        // =====================================================

        [Required(
            ErrorMessage =
                "El usuario es obligatorio."
        )]
        [Display(Name = "Usuario")]
        public string Username { get; set; }
            = string.Empty;

        [Required(
            ErrorMessage =
                "La contraseña es obligatoria."
        )]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage =
                "La contraseña debe tener al menos 8 caracteres."
        )]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }
            = string.Empty;

        // =====================================================
        // INFORMACIÓN PERSONAL
        // =====================================================

        [Required(
            ErrorMessage =
                "El DPI es obligatorio."
        )]
        [Display(Name = "DPI")]
        public string Dpi { get; set; }
            = string.Empty;

        [Display(Name = "Fecha de nacimiento")]
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }

        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        // =====================================================
        // INFORMACIÓN LABORAL
        // =====================================================

        [Display(Name = "Profesión")]
        public string? Profesion { get; set; }

        [Display(Name = "Lugar de trabajo")]
        public string? LugarTrabajo { get; set; }

        [Display(Name = "Ingreso mensual")]
        public decimal? IngresoMensual { get; set; }
    }
}