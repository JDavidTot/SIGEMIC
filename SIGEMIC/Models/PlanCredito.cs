using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGEMIC.Models
{
    [Table("planes_credito")]
    public class PlanCredito
    {
        [Key]
        [Column("id_plan")]
        public uint IdPlan { get; set; }

        [Required]
        [StringLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(255)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Required]
        [Column("tipo_tasa")]
        public string TipoTasa { get; set; } = string.Empty;

        [Column("porcentaje_interes", TypeName = "decimal(7,4)")]
        public decimal PorcentajeInteres { get; set; }

        [Required]
        [Column("estado")]
        public string Estado { get; set; } = "ACTIVO";

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        public ICollection<Credito> Creditos { get; set; } = new List<Credito>();
    }
}