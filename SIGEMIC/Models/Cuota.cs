using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGEMIC.Models
{
    [Table("cuotas")]
    public class Cuota
    {
        [Key]
        [Column("id_cuota")]
        public ulong IdCuota { get; set; }

        [Column("id_credito")]
        public ulong IdCredito { get; set; }

        [Column("numero_cuota")]
        public uint NumeroCuota { get; set; }

        [Column("fecha_vencimiento")]
        public DateTime FechaVencimiento { get; set; }

        [Column("capital_programado", TypeName = "decimal(14,2)")]
        public decimal CapitalProgramado { get; set; }

        [Column("interes_programado", TypeName = "decimal(14,2)")]
        public decimal InteresProgramado { get; set; }

        [Column("cuota_programada", TypeName = "decimal(14,2)")]
        public decimal CuotaProgramada { get; set; }

        [Column("capital_pagado", TypeName = "decimal(14,2)")]
        public decimal CapitalPagado { get; set; }

        [Column("interes_pagado", TypeName = "decimal(14,2)")]
        public decimal InteresPagado { get; set; }

        [Column("mora_pagada", TypeName = "decimal(14,2)")]
        public decimal MoraPagada { get; set; }

        [Column("saldo_cuota", TypeName = "decimal(14,2)")]
        public decimal SaldoCuota { get; set; }

        [Required]
        [Column("estado")]
        public string Estado { get; set; } = "PENDIENTE";

        [ForeignKey(nameof(IdCredito))]
        public Credito Credito { get; set; } = null!;

        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}