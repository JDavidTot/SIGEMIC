using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;

namespace SIGEMIC.Services;

public class PermisoService
{
    private readonly ApplicationDbContext _context;

    public PermisoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> TienePermisoAsync(uint idUsuario, string codigoPermiso)
    {
        if (idUsuario == 0 || string.IsNullOrWhiteSpace(codigoPermiso))
            return false;

        var usuario = await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.IdUsuario == idUsuario)
            .Select(u => new
            {
                u.Rol,
                u.Estado
            })
            .FirstOrDefaultAsync();

        if (usuario == null || usuario.Estado != "ACTIVO")
            return false;

        // El administrador siempre tiene acceso completo.
        if (usuario.Rol == "ADMINISTRADOR")
            return true;

        // Los permisos personalizados se aplican al cajero.
        if (usuario.Rol != "CAJERO")
            return false;

        return await _context.UsuarioPermisos
            .AsNoTracking()
            .AnyAsync(up =>
                up.IdUsuario == idUsuario &&
                up.Permiso != null &&
                up.Permiso.Codigo == codigoPermiso &&
                up.Permiso.Estado == "ACTIVO");
    }

    public async Task<List<string>> ObtenerPermisosAsync(uint idUsuario)
    {
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.IdUsuario == idUsuario)
            .Select(u => new
            {
                u.Rol,
                u.Estado
            })
            .FirstOrDefaultAsync();

        if (usuario == null || usuario.Estado != "ACTIVO")
            return new List<string>();

        if (usuario.Rol == "ADMINISTRADOR")
        {
            return await _context.Permisos
                .AsNoTracking()
                .Where(p => p.Estado == "ACTIVO")
                .Select(p => p.Codigo)
                .ToListAsync();
        }

        if (usuario.Rol != "CAJERO")
            return new List<string>();

        return await _context.UsuarioPermisos
            .AsNoTracking()
            .Where(up =>
                up.IdUsuario == idUsuario &&
                up.Permiso != null &&
                up.Permiso.Estado == "ACTIVO")
            .Select(up => up.Permiso!.Codigo)
            .ToListAsync();
    }

    public async Task<bool> TieneAlgunPermisoAsync(uint idUsuario, params string[] permisos)
    {
        foreach (var permiso in permisos)
        {
            if (await TienePermisoAsync(idUsuario, permiso))
                return true;
        }

        return false;
    }
}