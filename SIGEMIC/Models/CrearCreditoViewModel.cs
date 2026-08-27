using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SIGEMIC.Models
{
    public class CrearCreditoViewModel
    {
        [Required]
        [Display(Name = "Cliente")]
        public uint IdCliente { get; set; }

        [Required]
        [Display(Name = "Plan de crédito")]
        public uint IdPlan { get; set; }

        [Required]
        [Range(1, 1000000)]
        [Display(Name = "Monto solicitado")]
        public decimal MontoOriginal { get; set; }

        [Required]
        [Display(Name = "Plazo")]
        public uint PlazoMeses { get; set; }

        public List<SelectListItem> Clientes { get; set; }
            = new List<SelectListItem>();

        public List<SelectListItem> Planes { get; set; }
            = new List<SelectListItem>();
    }
}