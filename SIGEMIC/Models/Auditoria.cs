using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGEMIC.Models
{
    [Table("auditoria")]
    public class Auditoria
    {
        [Key]
        [Column("id_auditoria")]
        public ulong IdAuditoria { get; set; }

        [Column("id_usuario")]
        public uint? IdUsuario { get; set; }

        [Column("fecha_hora")]
        public DateTime FechaHora { get; set; }

        [Required]
        [StringLength(100)]
        [Column("accion")]
        public string Accion { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("tabla_afectada")]
        public string? TablaAfectada { get; set; }

        [StringLength(100)]
        [Column("registro_afectado")]
        public string? RegistroAfectado { get; set; }

        [StringLength(45)]
        [Column("direccion_ip")]
        public string? DireccionIp { get; set; }

        [StringLength(1000)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Required]
        [Column("resultado")]
        public string Resultado { get; set; } = "EXITOSO";

        [ForeignKey(nameof(IdUsuario))]
        public Usuario? Usuario { get; set; }
    }
}