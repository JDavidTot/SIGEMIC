using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGEMIC.Models
{
    [Table("clientes")]
    public class Cliente
    {
        [Key]
        [Column("id_cliente")]
        public uint IdCliente { get; set; }

        [Required]
        [Column("id_usuario")]
        public uint IdUsuario { get; set; }

        [Required]
        [StringLength(30)]
        [Column("codigo_cliente")]
        public string CodigoCliente { get; set; } = string.Empty;

        [Required]
        [Column("dpi_cifrado")]
        public byte[] DpiCifrado { get; set; } = [];

        [Required]
        [Column("dpi_hash")]
        public byte[] DpiHash { get; set; } = [];

        [Column("fecha_nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [Column("direccion_cifrada")]
        public byte[]? DireccionCifrada { get; set; }

        [Column("telefono_cifrado")]
        public byte[]? TelefonoCifrado { get; set; }

        [StringLength(100)]
        [Column("profesion")]
        public string? Profesion { get; set; }

        [StringLength(150)]
        [Column("lugar_trabajo")]
        public string? LugarTrabajo { get; set; }

        [Column("ingreso_mensual", TypeName = "decimal(12,2)")]
        public decimal? IngresoMensual { get; set; }

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; }

        [Required]
        [Column("estado")]
        public string Estado { get; set; } = "ACTIVO";

        [ForeignKey(nameof(IdUsuario))]
        public Usuario Usuario { get; set; } = null!;

        public ICollection<Credito> Creditos { get; set; } = new List<Credito>();

public ICollection<DocumentoCliente> Documentos { get; set; }
    = new List<DocumentoCliente>();
    }
}