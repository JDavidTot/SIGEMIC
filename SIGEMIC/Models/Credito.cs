using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGEMIC.Models
{
    [Table("creditos")]
    public class Credito
    {
        [Key]
        [Column("id_credito")]
        public ulong IdCredito { get; set; }

        [Required]
        [StringLength(30)]
        [Column("codigo_credito")]
        public string CodigoCredito { get; set; } = string.Empty;

        [Column("id_cliente")]
        public uint IdCliente { get; set; }

        [Column("id_plan")]
        public uint IdPlan { get; set; }

        [Column("monto_original", TypeName = "decimal(14,2)")]
        public decimal MontoOriginal { get; set; }

        [Required]
        [Column("tipo_tasa_aplicada")]
        public string TipoTasaAplicada { get; set; } = string.Empty;

        [Column("porcentaje_interes_aplicado", TypeName = "decimal(7,4)")]
        public decimal PorcentajeInteresAplicado { get; set; }

        [Column("plazo_meses")]
        public uint PlazoMeses { get; set; }

        [Column("cuota_original", TypeName = "decimal(14,2)")]
        public decimal CuotaOriginal { get; set; }

        [Column("saldo_capital", TypeName = "decimal(14,2)")]
        public decimal SaldoCapital { get; set; }

        [Required]
        [Column("estado")]
        public string Estado { get; set; } = "PENDIENTE";

        [Column("fecha_otorgamiento")]
        public DateTime FechaOtorgamiento { get; set; }

        [Column("fecha_inicio")]
        public DateTime FechaInicio { get; set; }

        [Column("fecha_finalizacion_estimada")]
        public DateTime FechaFinalizacionEstimada { get; set; }

        [Column("fecha_cancelacion")]
        public DateTime? FechaCancelacion { get; set; }

        [ForeignKey(nameof(IdCliente))]
        public Cliente Cliente { get; set; } = null!;

        [ForeignKey(nameof(IdPlan))]
        public PlanCredito PlanCredito { get; set; } = null!;

        public ICollection<Cuota> Cuotas { get; set; } = new List<Cuota>();
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}