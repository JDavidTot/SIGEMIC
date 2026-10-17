using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIGEMIC.Models;

[Table("permisos")]
public class Permiso
{
    [Key]
    [Column("id_permiso")]
    public uint IdPermiso { get; set; }

    [Required]
    [StringLength(80)]
    [Column("codigo")]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(255)]
    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Required]
    [StringLength(50)]
    [Column("modulo")]
    public string Modulo { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    [Column("estado")]
    public string Estado { get; set; } = "ACTIVO";

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}