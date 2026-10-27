using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGEMIC.Models
{
    [Table("bancos")]
    public class Banco
    {
        [Key]
        [Column("id_banco")]
        public uint IdBanco { get; set; }

        [Required]
        [StringLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("numero_cuenta")]
        public string NumeroCuenta { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Column("tipo_cuenta")]
        public string TipoCuenta { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [Column("beneficiario")]
        public string Beneficiario { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Column("estado")]
        public string Estado { get; set; } = "ACTIVO";

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        // =====================================================
        // RELACIONES
        // =====================================================

        public ICollection<SolicitudPago> SolicitudesPago { get; set; }
            = new List<SolicitudPago>();

        public ICollection<Pago> Pagos { get; set; }
            = new List<Pago>();
    }
}