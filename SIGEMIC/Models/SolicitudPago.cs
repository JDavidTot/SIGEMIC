using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGEMIC.Models
{
    [Table("solicitudes_pago")]
    public class SolicitudPago
    {
        [Key]
        [Column("id_solicitud")]
        public ulong IdSolicitud { get; set; }

        // =====================================================
        // LLAVES FORÁNEAS
        // =====================================================

        [Column("id_cliente")]
        public uint IdCliente { get; set; }

        [Column("id_credito")]
        public ulong IdCredito { get; set; }

        [Column("id_cuota")]
        public ulong? IdCuota { get; set; }

        [Column("id_banco")]
        public uint IdBanco { get; set; }

        [Column("id_usuario_revision")]
        public uint? IdUsuarioRevision { get; set; }

        // Pago generado cuando la solicitud es aprobada.
        // Mientras esté pendiente o rechazada permanece NULL.
        [Column("id_pago")]
        public ulong? IdPago { get; set; }


        // =====================================================
        // DATOS DE LA SOLICITUD
        // =====================================================

        [Required]
        [StringLength(30)]
        [Column("tipo_operacion")]
        public string TipoOperacion { get; set; } = string.Empty;


        [Column("monto", TypeName = "decimal(14,2)")]
        public decimal Monto { get; set; }


        [Required]
        [StringLength(100)]
        [Column("numero_boleta")]
        public string NumeroBoleta { get; set; } = string.Empty;


        [Required]
        [StringLength(255)]
        [Column("archivo_comprobante")]
        public string ArchivoComprobante { get; set; } = string.Empty;


        [Required]
        [StringLength(20)]
        [Column("estado")]
        public string Estado { get; set; } = "PENDIENTE";


        [Column("fecha_solicitud")]
        public DateTime FechaSolicitud { get; set; }


        [Column("fecha_revision")]
        public DateTime? FechaRevision { get; set; }


        [StringLength(255)]
        [Column("motivo_rechazo")]
        public string? MotivoRechazo { get; set; }


        // =====================================================
        // RELACIONES
        // =====================================================

        [ForeignKey(nameof(IdCliente))]
        public Cliente Cliente { get; set; } = null!;


        [ForeignKey(nameof(IdCredito))]
        public Credito Credito { get; set; } = null!;


        [ForeignKey(nameof(IdCuota))]
        public Cuota? Cuota { get; set; }


        [ForeignKey(nameof(IdBanco))]
        public Banco Banco { get; set; } = null!;


        [ForeignKey(nameof(IdUsuarioRevision))]
        public Usuario? UsuarioRevision { get; set; }


        // Pago real creado a partir de la solicitud.
        [ForeignKey(nameof(IdPago))]
        public Pago? Pago { get; set; }
    }
}