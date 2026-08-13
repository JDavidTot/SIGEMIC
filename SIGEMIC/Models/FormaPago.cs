using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGEMIC.Models
{
    [Table("formas_pago")]
    public class FormaPago
    {
        [Key]
        [Column("id_forma_pago")]
        public uint IdFormaPago { get; set; }

        [Required]
        [StringLength(50)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(255)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Required]
        [Column("estado")]
        public string Estado { get; set; } = "ACTIVO";

        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}