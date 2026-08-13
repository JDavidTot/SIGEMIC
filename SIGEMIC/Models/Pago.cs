using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGEMIC.Models
{
    [Table("pagos")]
    public class Pago
    {
        [Key]
        [Column("id_pago")]
        public ulong IdPago { get; set; }

        [Column("id_credito")]
        public ulong IdCredito { get; set; }

        [Column("id_cuota")]
        public ulong? IdCuota { get; set; }

        [Column("id_usuario")]
        public uint IdUsuario { get; set; }

        [Column("id_forma_pago")]
        public uint IdFormaPago { get; set; }

        [Required]
        [StringLength(40)]
        [Column("numero_recibo")]
        public string NumeroRecibo { get; set; } = string.Empty;

        [Required]
        [Column("tipo_operacion")]
        public string TipoOperacion { get; set; } = string.Empty;

        [Column("monto_total", TypeName = "decimal(14,2)")]
        public decimal MontoTotal { get; set; }

        [Column("capital_aplicado", TypeName = "decimal(14,2)")]
        public decimal CapitalAplicado { get; set; }

        [Column("interes_aplicado", TypeName = "decimal(14,2)")]
        public decimal InteresAplicado { get; set; }

        [Column("mora_aplicada", TypeName = "decimal(14,2)")]
        public decimal MoraAplicada { get; set; }

        [Column("fecha_pago")]
        public DateTime FechaPago { get; set; }

        [StringLength(500)]
        [Column("observacion")]
        public string? Observacion { get; set; }

        [ForeignKey(nameof(IdCredito))]
        public Credito Credito { get; set; } = null!;

        [ForeignKey(nameof(IdCuota))]
        public Cuota? Cuota { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public Usuario Usuario { get; set; } = null!;

        [ForeignKey(nameof(IdFormaPago))]
        public FormaPago FormaPago { get; set; } = null!;
    }
}