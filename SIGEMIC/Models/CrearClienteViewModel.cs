using System.ComponentModel.DataAnnotations;

namespace SIGEMIC.Models
{
    public class CrearClienteViewModel
    {
        [Required]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Usuario")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "DPI")]
        public string Dpi { get; set; } = string.Empty;

        [Display(Name = "Fecha de nacimiento")]
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }

        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Display(Name = "Profesión")]
        public string? Profesion { get; set; }

        [Display(Name = "Lugar de trabajo")]
        public string? LugarTrabajo { get; set; }

        [Display(Name = "Ingreso mensual")]
        public decimal? IngresoMensual { get; set; }
    }
}