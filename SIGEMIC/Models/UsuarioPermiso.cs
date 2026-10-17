using System.ComponentModel.DataAnnotations.Schema;

namespace SIGEMIC.Models;

[Table("usuario_permisos")]
public class UsuarioPermiso
{
    [Column("id_usuario")]
    public uint IdUsuario { get; set; }

    [Column("id_permiso")]
    public uint IdPermiso { get; set; }

    [Column("fecha_asignacion")]
    public DateTime FechaAsignacion { get; set; } = DateTime.Now;

    [ForeignKey(nameof(IdUsuario))]
    public Usuario? Usuario { get; set; }

    [ForeignKey(nameof(IdPermiso))]
    public Permiso? Permiso { get; set; }
}