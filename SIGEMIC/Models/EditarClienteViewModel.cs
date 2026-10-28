using System.ComponentModel.DataAnnotations;

namespace SIGEMIC.Models
{
    public class EditarClienteViewModel
    {
        public uint IdCliente { get; set; }

        [Required(
            ErrorMessage =
                "Ingrese los nombres."
        )]
        [StringLength(100)]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; }
            = string.Empty;

        [Required(
            ErrorMessage =
                "Ingrese los apellidos."
        )]
        [StringLength(100)]
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; }
            = string.Empty;

        // =====================================================
        // CORREO
        // =====================================================

        [Required(
            ErrorMessage =
                "Ingrese el correo electrónico."
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
        // USUARIO
        // =====================================================

        [Required(
            ErrorMessage =
                "Ingrese el nombre de usuario."
        )]
        [StringLength(50)]
        [Display(Name = "Usuario")]
        public string Username { get; set; }
            = string.Empty;

        // =====================================================
        // INFORMACIÓN PERSONAL
        // =====================================================

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [StringLength(255)]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [StringLength(30)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        // =====================================================
        // INFORMACIÓN LABORAL
        // =====================================================

        [StringLength(100)]
        [Display(Name = "Profesión")]
        public string? Profesion { get; set; }

        [StringLength(150)]
        [Display(Name = "Lugar de trabajo")]
        public string? LugarTrabajo { get; set; }

        [Range(
            typeof(decimal),
            "0",
            "999999999.99",
            ErrorMessage =
                "El ingreso mensual no puede ser negativo."
        )]
        [Display(Name = "Ingreso mensual")]
        public decimal? IngresoMensual { get; set; }

        public string Estado { get; set; }
            = string.Empty;
    }
}