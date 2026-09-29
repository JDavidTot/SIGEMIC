using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SIGEMIC.Models
{
    public class SolicitarPagoViewModel
    {

        [Required(ErrorMessage = "Seleccione el tipo de operación.")]
        [Display(Name = "Tipo de operación")]
        public string TipoOperacion { get; set; } = "PAGO_CUOTA";


        [Required(ErrorMessage = "Seleccione un crédito.")]
        [Display(Name = "Crédito")]
        public ulong IdCredito { get; set; }



        [Display(Name = "Cuota")]
        public ulong? IdCuota { get; set; }


        [Required(ErrorMessage = "Seleccione el banco.")]
        [Display(Name = "Banco")]
        public uint IdBanco { get; set; }


        [Required(ErrorMessage = "Ingrese el monto depositado.")]
        [Range(
            typeof(decimal),
            "0.01",
            "999999999.99",
            ErrorMessage = "El monto debe ser mayor que cero."
        )]
        [Display(Name = "Monto depositado")]
        public decimal Monto { get; set; }


        [Required(ErrorMessage = "Ingrese el número de boleta.")]
        [StringLength(
            100,
            ErrorMessage = "El número de boleta es demasiado largo."
        )]
        [Display(Name = "Número de boleta")]
        public string NumeroBoleta { get; set; } = string.Empty;


        [Required(ErrorMessage = "Debe adjuntar el comprobante.")]
        [Display(Name = "Comprobante")]
        public IFormFile? Comprobante { get; set; }



        public List<SelectListItem> Creditos { get; set; }
            = new();

        public List<SelectListItem> Cuotas { get; set; }
            = new();

        public List<Banco> Bancos { get; set; }
            = new();
    }
}