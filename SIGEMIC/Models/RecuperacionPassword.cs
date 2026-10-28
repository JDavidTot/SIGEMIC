using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGEMIC.Models;

[Table("recuperaciones_password")]
public class RecuperacionPassword
{
    [Key]
    [Column("id_recuperacion")]
    public ulong IdRecuperacion { get; set; }

    [Column("id_usuario")]
    public uint IdUsuario { get; set; }

    [Required]
    [Column("token_hash")]
    public byte[] TokenHash { get; set; }
        = Array.Empty<byte>();

    [Column("fecha_solicitud")]
    public DateTime FechaSolicitud { get; set; }

    [Column("fecha_expiracion")]
    public DateTime FechaExpiracion { get; set; }

    [Column("fecha_utilizacion")]
    public DateTime? FechaUtilizacion { get; set; }

    [Column("utilizado")]
    public bool Utilizado { get; set; }

    [StringLength(45)]
    [Column("direccion_ip")]
    public string? DireccionIp { get; set; }

    [ForeignKey(nameof(IdUsuario))]
    public Usuario Usuario { get; set; }
        = null!;

    [NotMapped]
    public bool Expirado =>
        DateTime.Now > FechaExpiracion;

    [NotMapped]
    public bool Disponible =>
        !Utilizado &&
        !Expirado;
}