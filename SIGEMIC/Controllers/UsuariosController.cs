using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Models;
using SIGEMIC.Services;
using SIGEMIC.ViewModels;

namespace SIGEMIC.Controllers;

[Authorize(Roles = "ADMINISTRADOR")]
public class UsuariosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly AuditoriaService _auditoriaService;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(
        ApplicationDbContext context,
        AuditoriaService auditoriaService,
        ILogger<UsuariosController> logger)
    {
        _context = context;
        _auditoriaService = auditoriaService;
        _logger = logger;
    }

    // =====================================================
    // LISTADO
    // =====================================================

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

    // =====================================================
    // CREAR - GET
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new CrearCajeroViewModel();

        await CargarPermisos(model);

        return View(model);
    }

    // =====================================================
    // CREAR - POST
    // =====================================================

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
            var usuario = new Usuario
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

            // ==========================================
            // PERMISOS SELECCIONADOS
            // ==========================================

            var idsSeleccionados =
                model.Permisos
                    .Where(p =>
                        p.Seleccionado)
                    .Select(p =>
                        p.IdPermiso)
                    .Distinct()
                    .ToList();

            // Agregar dependencias necesarias.
            var idsFinales =
                await ObtenerPermisosConDependencias(
                    idsSeleccionados
                );

            foreach (var idPermiso in idsFinales)
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

            // ==========================================
            // AUDITORÍA
            // ==========================================

            var codigosPermisos =
                await ObtenerCodigosPermisosAsync(
                    idsFinales
                );

            string permisosTexto =
                codigosPermisos.Count > 0
                    ? string.Join(
                        ", ",
                        codigosPermisos
                    )
                    : "Sin permisos asignados";

            await RegistrarAuditoriaSeguroAsync(
                "CAJERO_CREADO",
                "usuarios",
                usuario.IdUsuario.ToString(),

                $"Cajero {usuario.Username} creado. " +
                $"Nombre: {usuario.Nombres} {usuario.Apellidos}; " +
                $"Estado: {usuario.Estado}; " +
                $"Permisos iniciales: {permisosTexto}."
            );

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

    // =====================================================
    // EDITAR - GET
    // =====================================================

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
        {
            return NotFound();
        }

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

    // =====================================================
    // EDITAR - POST
    // =====================================================

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
        {
            return NotFound();
        }

        model.Username =
            model.Username.Trim();

        if (
            await _context.Usuarios.AnyAsync(u =>
                u.Username ==
                    model.Username &&
                u.IdUsuario !=
                    model.IdUsuario
            )
        )
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
        {
            return View(model);
        }

        // ==========================================
        // GUARDAR VALORES ANTERIORES
        // ==========================================

        string nombresAnterior =
            usuario.Nombres;

        string apellidosAnterior =
            usuario.Apellidos;

        string usernameAnterior =
            usuario.Username;

        string estadoAnterior =
            usuario.Estado;

        string? correoAnterior =
            usuario.CorreoCifrado != null
                ? Encoding.UTF8.GetString(
                    usuario.CorreoCifrado
                )
                : null;

        // ==========================================
        // NUEVOS VALORES
        // ==========================================

        string nombresNuevo =
            model.Nombres.Trim();

        string apellidosNuevo =
            model.Apellidos.Trim();

        string usernameNuevo =
            model.Username;

        string correoNuevo =
            model.Correo.Trim();

        string estadoNuevo =
            model.Estado;

        bool contrasenaActualizada =
            !string.IsNullOrWhiteSpace(
                model.NuevaContrasena
            );

        // ==========================================
        // DETECTAR CAMBIOS
        // ==========================================

        var cambios =
            new List<string>();

        AgregarCambio(
            cambios,
            "Nombres",
            nombresAnterior,
            nombresNuevo
        );

        AgregarCambio(
            cambios,
            "Apellidos",
            apellidosAnterior,
            apellidosNuevo
        );

        AgregarCambio(
            cambios,
            "Usuario",
            usernameAnterior,
            usernameNuevo
        );

        AgregarCambioProtegido(
            cambios,
            "Correo",
            correoAnterior,
            correoNuevo
        );

        AgregarCambio(
            cambios,
            "Estado",
            estadoAnterior,
            estadoNuevo
        );

        if (contrasenaActualizada)
        {
            cambios.Add(
                "Contraseña: Sin cambios → Actualizada"
            );
        }

        // ==========================================
        // ACTUALIZAR
        // ==========================================

        usuario.Nombres =
            nombresNuevo;

        usuario.Apellidos =
            apellidosNuevo;

        usuario.Username =
            usernameNuevo;

        usuario.CorreoCifrado =
            Encoding.UTF8.GetBytes(
                correoNuevo
            );

        usuario.Estado =
            estadoNuevo;

        if (contrasenaActualizada)
        {
            var hasher =
                new PasswordHasher<Usuario>();

            usuario.PasswordHash =
                hasher.HashPassword(
                    usuario,
                    model.NuevaContrasena!
                );
        }

        await _context.SaveChangesAsync();

        // ==========================================
        // AUDITORÍA
        // ==========================================

        string descripcion =
            cambios.Count > 0
                ? $"Cajero {usuario.Username} actualizado. " +
                  $"Cambios: {string.Join("; ", cambios)}."
                : $"Cajero {usuario.Username} actualizado. " +
                  "No se detectaron cambios en los datos editables.";

        await RegistrarAuditoriaSeguroAsync(
            "CAJERO_EDITADO",
            "usuarios",
            usuario.IdUsuario.ToString(),
            descripcion
        );

        TempData["Exito"] =
            "El cajero fue actualizado correctamente.";

        return RedirectToAction(
            nameof(Index)
        );
    }

    // =====================================================
    // PERMISOS - GET
    // =====================================================

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
        {
            return NotFound();
        }

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

    // =====================================================
    // PERMISOS - POST
    // =====================================================

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
        {
            return NotFound();
        }

        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        try
        {
            // ==========================================
            // PERMISOS ANTERIORES
            // ==========================================

            var actuales =
                await _context.UsuarioPermisos
                    .Where(up =>
                        up.IdUsuario ==
                            model.IdUsuario)
                    .ToListAsync();

            var idsAnteriores =
                actuales
                    .Select(up =>
                        up.IdPermiso)
                    .ToHashSet();

            _context.UsuarioPermisos
                .RemoveRange(actuales);

            // ==========================================
            // NUEVA SELECCIÓN
            // ==========================================

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

            foreach (var idPermiso in idsFinales)
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

            // ==========================================
            // COMPARAR ANTES Y DESPUÉS
            // ==========================================

            var idsFinalesSet =
                idsFinales.ToHashSet();

            var idsAgregados =
                idsFinalesSet
                    .Except(idsAnteriores)
                    .ToList();

            var idsRetirados =
                idsAnteriores
                    .Except(idsFinalesSet)
                    .ToList();

            var codigosAgregados =
                await ObtenerCodigosPermisosAsync(
                    idsAgregados
                );

            var codigosRetirados =
                await ObtenerCodigosPermisosAsync(
                    idsRetirados
                );

            var cambios =
                new List<string>();

            if (codigosAgregados.Count > 0)
            {
                cambios.Add(
                    "Permisos agregados: Sin dato → " +
                    string.Join(
                        ", ",
                        codigosAgregados
                    )
                );
            }

            if (codigosRetirados.Count > 0)
            {
                cambios.Add(
                    "Permisos retirados: " +
                    string.Join(
                        ", ",
                        codigosRetirados
                    ) +
                    " → Sin dato"
                );
            }

            // ==========================================
            // AUDITORÍA
            // ==========================================

            string descripcion =
                cambios.Count > 0
                    ? $"Permisos del cajero {usuario.Username} actualizados. " +
                      $"Cambios: {string.Join("; ", cambios)}."
                    : $"Permisos del cajero {usuario.Username} revisados. " +
                      "No se detectaron cambios.";

            await RegistrarAuditoriaSeguroAsync(
                "PERMISOS_MODIFICADOS",
                "usuario_permisos",
                usuario.IdUsuario.ToString(),
                descripcion
            );

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

        return permisosActivos
            .Where(p =>
                ids.Contains(
                    p.IdPermiso
                ))
            .Select(p =>
                p.IdPermiso)
            .ToList();
    }

    // =====================================================
    // OBTENER CÓDIGOS DE PERMISOS
    // =====================================================

    private async Task<List<string>>
        ObtenerCodigosPermisosAsync(
            IEnumerable<uint> idsPermisos)
    {
        var ids =
            idsPermisos
                .Distinct()
                .ToList();

        if (ids.Count == 0)
        {
            return new List<string>();
        }

        return await _context.Permisos
            .AsNoTracking()
            .Where(p =>
                ids.Contains(
                    p.IdPermiso
                ))
            .OrderBy(p =>
                p.Modulo)
            .ThenBy(p =>
                p.Codigo)
            .Select(p =>
                p.Codigo)
            .ToListAsync();
    }

    // =====================================================
    // AUDITORÍA SEGURA
    // =====================================================

    private async Task RegistrarAuditoriaSeguroAsync(
        string accion,
        string tablaAfectada,
        string registroAfectado,
        string descripcion)
    {
        try
        {
            await _auditoriaService
                .RegistrarUsuarioActualAsync(
                    accion,
                    tablaAfectada,
                    registroAfectado,
                    descripcion,
                    "EXITOSO"
                );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No fue posible registrar la auditoría de la acción {Accion}.",
                accion
            );
        }
    }

    // =====================================================
    // DETECTAR CAMBIOS NORMALES
    // =====================================================

    private static void AgregarCambio(
        ICollection<string> cambios,
        string campo,
        string? valorAnterior,
        string? valorNuevo)
    {
        string anterior =
            NormalizarParaComparar(
                valorAnterior
            );

        string nuevo =
            NormalizarParaComparar(
                valorNuevo
            );

        if (
            string.Equals(
                anterior,
                nuevo,
                StringComparison.Ordinal
            )
        )
        {
            return;
        }

        cambios.Add(
            $"{campo}: " +
            $"{MostrarValor(valorAnterior)} → " +
            $"{MostrarValor(valorNuevo)}"
        );
    }

    // =====================================================
    // DATOS SENSIBLES
    // El correo real no se copia al log.
    // =====================================================

    private static void AgregarCambioProtegido(
        ICollection<string> cambios,
        string campo,
        string? valorAnterior,
        string? valorNuevo)
    {
        string anterior =
            NormalizarParaComparar(
                valorAnterior
            );

        string nuevo =
            NormalizarParaComparar(
                valorNuevo
            );

        if (
            string.Equals(
                anterior,
                nuevo,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return;
        }

        string estadoAnterior =
            string.IsNullOrWhiteSpace(
                valorAnterior
            )
                ? "Sin dato"
                : "Dato registrado";

        string estadoNuevo =
            string.IsNullOrWhiteSpace(
                valorNuevo
            )
                ? "Sin dato"
                : "Dato actualizado";

        cambios.Add(
            $"{campo}: " +
            $"{estadoAnterior} → " +
            $"{estadoNuevo}"
        );
    }

    private static string NormalizarParaComparar(
        string? valor)
    {
        return string.IsNullOrWhiteSpace(
            valor
        )
            ? string.Empty
            : valor.Trim();
    }

    private static string MostrarValor(
        string? valor)
    {
        return string.IsNullOrWhiteSpace(
            valor
        )
            ? "Sin dato"
            : valor.Trim();
    }
}