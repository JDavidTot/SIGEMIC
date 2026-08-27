using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SIGEMIC.Models
{
    public class AbonoExtraordinarioViewModel
    {
        public ulong IdCredito { get; set; }

        public string CodigoCredito { get; set; } = "";

        public string Cliente { get; set; } = "";

        public decimal SaldoCapital { get; set; }

        public decimal CuotaActual { get; set; }

        public uint PlazoOriginal { get; set; }

        [Required(ErrorMessage = "Ingrese el monto del abono.")]
        [Range(
            typeof(decimal),
            "0.01", "999999999.99",
            ErrorMessage = "El monto debe ser mayor que cero."
        )]
        [Display(Name = "Monto del abono")]
        public decimal MontoAbono { get; set; }

        [Required(ErrorMessage = "Seleccione una forma de pago.")]
        [Display(Name = "Forma de pago")]
        public uint IdFormaPago { get; set; }

        [Display(Name = "Observación")]
        [StringLength(255)]
        public string? Observacion { get; set; }

        public List<SelectListItem> FormasPago { get; set; }
            = new List<SelectListItem>();
    }
}