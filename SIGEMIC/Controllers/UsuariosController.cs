using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Models;
using SIGEMIC.ViewModels;

namespace SIGEMIC.Controllers;

[Authorize(Roles = "ADMINISTRADOR")]
public class UsuariosController : Controller
{
    private readonly ApplicationDbContext _context;

    public UsuariosController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cajeros = await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.Rol == "CAJERO")
            .OrderBy(u => u.Nombres)
            .ThenBy(u => u.Apellidos)
            .ToListAsync();

        return View(cajeros);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new CrearCajeroViewModel();

        await CargarPermisos(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CrearCajeroViewModel model)
    {
        model.Username =
            model.Username.Trim();

        if (await _context.Usuarios.AnyAsync(u =>
            u.Username == model.Username))
        {
            ModelState.AddModelError(
                nameof(model.Username),
                "Ese nombre de usuario ya existe."
            );
        }

        if (!ModelState.IsValid)
        {
            await CargarPermisos(model);

            return View(model);
        }

        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        try
        {
            var usuario =
                new Usuario
                {
                    Nombres =
                        model.Nombres.Trim(),

                    Apellidos =
                        model.Apellidos.Trim(),

                    Username =
                        model.Username,

                    CorreoCifrado =
                        Encoding.UTF8.GetBytes(
                            model.Correo.Trim()
                        ),

                    Rol =
                        "CAJERO",

                    Estado =
                        model.Estado,

                    IntentosFallidos =
                        0,

                    UltimoAcceso =
                        null,

                    FechaCreacion =
                        DateTime.Now
                };

            var hasher =
                new PasswordHasher<Usuario>();

            usuario.PasswordHash =
                hasher.HashPassword(
                    usuario,
                    model.Contrasena
                );

            _context.Usuarios.Add(
                usuario
            );

            await _context.SaveChangesAsync();

            var idsSeleccionados =
                model.Permisos
                    .Where(p =>
                        p.Seleccionado)
                    .Select(p =>
                        p.IdPermiso)
                    .Distinct()
                    .ToList();

            var idsFinales =
                await ObtenerPermisosConDependencias(
                    idsSeleccionados
                );

            foreach (
                var idPermiso in idsFinales)
            {
                _context.UsuarioPermisos.Add(
                    new UsuarioPermiso
                    {
                        IdUsuario =
                            usuario.IdUsuario,

                        IdPermiso =
                            idPermiso,

                        FechaAsignacion =
                            DateTime.Now
                    }
                );
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            TempData["Exito"] =
                "El cajero fue creado correctamente.";

            return RedirectToAction(
                nameof(Index)
            );
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        uint id)
    {
        var usuario =
            await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == id &&
                    u.Rol == "CAJERO"
                );

        if (usuario == null)
            return NotFound();

        var model =
            new EditarCajeroViewModel
            {
                IdUsuario =
                    usuario.IdUsuario,

                Nombres =
                    usuario.Nombres,

                Apellidos =
                    usuario.Apellidos,

                Username =
                    usuario.Username,

                Correo =
                    usuario.CorreoCifrado != null
                        ? Encoding.UTF8.GetString(
                            usuario.CorreoCifrado
                        )
                        : string.Empty,

                Estado =
                    usuario.Estado
            };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        EditarCajeroViewModel model)
    {
        var usuario =
            await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario ==
                        model.IdUsuario &&
                    u.Rol == "CAJERO"
                );

        if (usuario == null)
            return NotFound();

        model.Username =
            model.Username.Trim();

        if (await _context.Usuarios
            .AnyAsync(u =>
                u.Username ==
                    model.Username &&
                u.IdUsuario !=
                    model.IdUsuario))
        {
            ModelState.AddModelError(
                nameof(model.Username),
                "Ese nombre de usuario ya está siendo utilizado."
            );
        }

        if (
            !string.IsNullOrWhiteSpace(
                model.NuevaContrasena
            ) &&
            model.NuevaContrasena.Length < 8
        )
        {
            ModelState.AddModelError(
                nameof(model.NuevaContrasena),
                "La nueva contraseña debe tener al menos 8 caracteres."
            );
        }

        if (!ModelState.IsValid)
            return View(model);

        usuario.Nombres =
            model.Nombres.Trim();

        usuario.Apellidos =
            model.Apellidos.Trim();

        usuario.Username =
            model.Username;

        usuario.CorreoCifrado =
            Encoding.UTF8.GetBytes(
                model.Correo.Trim()
            );

        usuario.Estado =
            model.Estado;

        if (
            !string.IsNullOrWhiteSpace(
                model.NuevaContrasena
            )
        )
        {
            var hasher =
                new PasswordHasher<Usuario>();

            usuario.PasswordHash =
                hasher.HashPassword(
                    usuario,
                    model.NuevaContrasena
                );
        }

        await _context.SaveChangesAsync();

        TempData["Exito"] =
            "El cajero fue actualizado correctamente.";

        return RedirectToAction(
            nameof(Index)
        );
    }

    [HttpGet]
    public async Task<IActionResult> Permisos(
        uint id)
    {
        var usuario =
            await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == id &&
                    u.Rol == "CAJERO"
                );

        if (usuario == null)
            return NotFound();

        var asignados =
            await _context.UsuarioPermisos
                .Where(up =>
                    up.IdUsuario == id)
                .Select(up =>
                    up.IdPermiso)
                .ToListAsync();

        var permisos =
            await _context.Permisos
                .AsNoTracking()
                .Where(p =>
                    p.Estado == "ACTIVO")
                .OrderBy(p =>
                    p.Modulo)
                .ThenBy(p =>
                    p.Nombre)
                .Select(p =>
                    new PermisoSeleccionViewModel
                    {
                        IdPermiso =
                            p.IdPermiso,

                        Codigo =
                            p.Codigo,

                        Nombre =
                            p.Nombre,

                        Descripcion =
                            p.Descripcion,

                        Modulo =
                            p.Modulo,

                        Seleccionado =
                            asignados.Contains(
                                p.IdPermiso
                            )
                    })
                .ToListAsync();

        var model =
            new PermisosCajeroViewModel
            {
                IdUsuario =
                    usuario.IdUsuario,

                NombreCompleto =
                    usuario.Nombres +
                    " " +
                    usuario.Apellidos,

                Username =
                    usuario.Username,

                Permisos =
                    permisos
            };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Permisos(
        PermisosCajeroViewModel model)
    {
        var usuario =
            await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario ==
                        model.IdUsuario &&
                    u.Rol == "CAJERO"
                );

        if (usuario == null)
            return NotFound();

        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        try
        {
            var actuales =
                await _context.UsuarioPermisos
                    .Where(up =>
                        up.IdUsuario ==
                            model.IdUsuario)
                    .ToListAsync();

            _context.UsuarioPermisos
                .RemoveRange(actuales);

            var idsSeleccionados =
                model.Permisos
                    .Where(p =>
                        p.Seleccionado)
                    .Select(p =>
                        p.IdPermiso)
                    .Distinct()
                    .ToList();

            var idsFinales =
                await ObtenerPermisosConDependencias(
                    idsSeleccionados
                );

            foreach (
                var idPermiso in idsFinales)
            {
                _context.UsuarioPermisos.Add(
                    new UsuarioPermiso
                    {
                        IdUsuario =
                            usuario.IdUsuario,

                        IdPermiso =
                            idPermiso,

                        FechaAsignacion =
                            DateTime.Now
                    }
                );
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            TempData["Exito"] =
                $"Los permisos de {usuario.Nombres} fueron actualizados.";

            return RedirectToAction(
                nameof(Index)
            );
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }

    // =====================================================
    // CARGAR PERMISOS EN CREACIÓN
    // =====================================================
    private async Task CargarPermisos(
        CrearCajeroViewModel model)
    {
        var seleccionados =
            model.Permisos
                .Where(p =>
                    p.Seleccionado)
                .Select(p =>
                    p.IdPermiso)
                .ToHashSet();

        model.Permisos =
            await _context.Permisos
                .AsNoTracking()
                .Where(p =>
                    p.Estado == "ACTIVO")
                .OrderBy(p =>
                    p.Modulo)
                .ThenBy(p =>
                    p.Nombre)
                .Select(p =>
                    new PermisoSeleccionViewModel
                    {
                        IdPermiso =
                            p.IdPermiso,

                        Codigo =
                            p.Codigo,

                        Nombre =
                            p.Nombre,

                        Descripcion =
                            p.Descripcion,

                        Modulo =
                            p.Modulo,

                        Seleccionado =
                            seleccionados.Contains(
                                p.IdPermiso
                            )
                    })
                .ToListAsync();
    }

    // =====================================================
    // NORMALIZAR PERMISOS Y AGREGAR DEPENDENCIAS
    // =====================================================
    private async Task<List<uint>>
        ObtenerPermisosConDependencias(
            IEnumerable<uint> idsSeleccionados)
    {
        var ids =
            idsSeleccionados
                .Distinct()
                .ToHashSet();

        var permisosActivos =
            await _context.Permisos
                .AsNoTracking()
                .Where(p =>
                    p.Estado == "ACTIVO")
                .Select(p =>
                    new
                    {
                        p.IdPermiso,
                        p.Codigo
                    })
                .ToListAsync();

        var codigoPorId =
            permisosActivos
                .ToDictionary(
                    p => p.IdPermiso,
                    p => p.Codigo,
                    EqualityComparer<uint>.Default
                );

        var idPorCodigo =
            permisosActivos
                .ToDictionary(
                    p => p.Codigo,
                    p => p.IdPermiso,
                    StringComparer.OrdinalIgnoreCase
                );

        var codigosSeleccionados =
            ids
                .Where(id =>
                    codigoPorId.ContainsKey(id))
                .Select(id =>
                    codigoPorId[id])
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase
                );

        void AgregarDependencia(
            string permisoHijo,
            string permisoPadre)
        {
            if (
                codigosSeleccionados.Contains(
                    permisoHijo
                ) &&
                idPorCodigo.TryGetValue(
                    permisoPadre,
                    out uint idPadre
                )
            )
            {
                ids.Add(idPadre);

                codigosSeleccionados.Add(
                    permisoPadre
                );
            }
        }

        // CLIENTES
        AgregarDependencia(
            "CLIENTES_CREAR",
            "CLIENTES_VER"
        );

        AgregarDependencia(
            "CLIENTES_EDITAR",
            "CLIENTES_VER"
        );

        AgregarDependencia(
            "CLIENTES_CAMBIAR_ESTADO",
            "CLIENTES_VER"
        );

        // CRÉDITOS
        AgregarDependencia(
            "CREDITOS_CREAR",
            "CREDITOS_VER"
        );

        AgregarDependencia(
            "CREDITOS_DETALLE",
            "CREDITOS_VER"
        );

        // PAGOS
        AgregarDependencia(
            "PAGOS_ABONO_CAPITAL",
            "PAGOS_REGISTRAR"
        );

        // SOLICITUDES
        AgregarDependencia(
            "SOLICITUDES_APROBAR",
            "SOLICITUDES_VER"
        );

        AgregarDependencia(
            "SOLICITUDES_RECHAZAR",
            "SOLICITUDES_VER"
        );

        var idsValidos =
            permisosActivos
                .Where(p =>
                    ids.Contains(p.IdPermiso))
                .Select(p =>
                    p.IdPermiso)
                .ToList();

        return idsValidos;
    }
}