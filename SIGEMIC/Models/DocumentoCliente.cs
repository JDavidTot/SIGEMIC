using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGEMIC.Models
{
    [Table("documentos_cliente")]
    public class DocumentoCliente
    {
        [Key]
        [Column("id_documento")]
        public ulong IdDocumento { get; set; }

        [Column("id_cliente")]
        public uint IdCliente { get; set; }

        [Required]
        [Column("tipo_documento")]
        public string TipoDocumento { get; set; } = string.Empty;

        [Required]
        [Column("numero_cifrado")]
        public byte[] NumeroCifrado { get; set; } = [];

        [Required]
        [Column("numero_hash")]
        public byte[] NumeroHash { get; set; } = [];

        [Column("fecha_emision")]
        public DateTime? FechaEmision { get; set; }

        [Column("fecha_vencimiento")]
        public DateTime? FechaVencimiento { get; set; }

        [Required]
        [Column("estado")]
        public string Estado { get; set; } = "ACTIVO";

        [ForeignKey(nameof(IdCliente))]
        public Cliente Cliente { get; set; } = null!;
    }
}