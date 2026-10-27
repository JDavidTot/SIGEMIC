using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
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

        // =====================================================
        // MONTO
        // =====================================================

        [Required(
            ErrorMessage = "Ingrese el monto del abono."
        )]
        [Range(
            typeof(decimal),
            "0.01",
            "999999999.99",
            ErrorMessage = "El monto debe ser mayor que cero."
        )]
        [Display(Name = "Monto del abono")]
        public decimal MontoAbono { get; set; }

        // =====================================================
        // FORMA DE PAGO
        // =====================================================

        [Required(
            ErrorMessage = "Seleccione una forma de pago."
        )]
        [Display(Name = "Forma de pago")]
        public uint IdFormaPago { get; set; }

        // =====================================================
        // DATOS BANCARIOS
        // =====================================================

        [Display(Name = "Banco")]
        public uint? IdBanco { get; set; }

        [StringLength(
            100,
            ErrorMessage =
                "El número de boleta no puede superar los 100 caracteres."
        )]
        [Display(Name = "Número de boleta")]
        public string? NumeroBoleta { get; set; }

        [Display(Name = "Comprobante")]
        public IFormFile? Comprobante { get; set; }

        // =====================================================
        // OBSERVACIÓN
        // =====================================================

        [Display(Name = "Observación")]
        [StringLength(255)]
        public string? Observacion { get; set; }

        // =====================================================
        // LISTAS
        // =====================================================

        public List<SelectListItem> FormasPago { get; set; }
            = new();

        public List<Banco> Bancos { get; set; }
            = new();
    }
}