using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGEMIC.Models
{
    [Table("usuarios")]
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public uint IdUsuario { get; set; }

        [Required]
        [StringLength(100)]
        [Column("nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("correo_cifrado")]
        public byte[]? CorreoCifrado { get; set; }

        [Required]
        [Column("rol")]
        public string Rol { get; set; } = string.Empty;

        [Required]
        [Column("estado")]
        public string Estado { get; set; } = "ACTIVO";

        [Column("intentos_fallidos")]
        public byte IntentosFallidos { get; set; }

        [Column("ultimo_acceso")]
        public DateTime? UltimoAcceso { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        public Cliente? Cliente { get; set; }
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();

public ICollection<Auditoria> Auditorias { get; set; }
    = new List<Auditoria>();
    }
}