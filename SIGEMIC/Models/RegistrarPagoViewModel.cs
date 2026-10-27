using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SIGEMIC.Models
{
    public class RegistrarPagoViewModel
    {
        public ulong IdCuota { get; set; }

        public ulong IdCredito { get; set; }

        public string CodigoCredito { get; set; } = "";

        public string Cliente { get; set; } = "";

        public uint NumeroCuota { get; set; }

        public DateTime FechaVencimiento { get; set; }

        public decimal CapitalPendiente { get; set; }

        public decimal InteresPendiente { get; set; }

        public decimal MoraPendiente { get; set; }

        public decimal TotalPagar { get; set; }

        // =====================================================
        // MONTO
        // =====================================================

        [Required(
            ErrorMessage = "Ingrese el monto a pagar."
        )]
        [Range(
            typeof(decimal),
            "0.01",
            "999999999.99",
            ErrorMessage = "El monto debe ser mayor que cero."
        )]
        [Display(Name = "Monto recibido")]
        public decimal MontoPago { get; set; }

        // =====================================================
        // FORMA DE PAGO
        // =====================================================

        [Required(
            ErrorMessage = "Seleccione una forma de pago."
        )]
        [Display(Name = "Forma de pago")]
        public uint IdFormaPago { get; set; }

        // =====================================================
        // DATOS PARA TRANSFERENCIA / DEPÓSITO
        // Se validarán condicionalmente en el controlador.
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