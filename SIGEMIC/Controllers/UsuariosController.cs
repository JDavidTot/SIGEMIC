using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Models;
using SIGEMIC.Security;
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
    // LISTADO GENERAL
    // ADMINISTRADORES Y CAJEROS
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var usuarios =
            await _context.Usuarios
                .AsNoTracking()
                .Where(u =>
                    u.Rol == "ADMINISTRADOR" ||
                    u.Rol == "CAJERO"
                )
                .OrderBy(u =>
                    u.Rol
                )
                .ThenBy(u =>
                    u.Nombres
                )
                .ThenBy(u =>
                    u.Apellidos
                )
                .ToListAsync();

        return View(usuarios);
    }


    // =====================================================
    // CREAR - GET
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model =
            new CrearUsuarioViewModel
            {
                Estado = "ACTIVO"
            };

        await CargarPermisos(
            model
        );

        return View(model);
    }


    // =====================================================
    // CREAR - POST
    // =====================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CrearUsuarioViewModel model)
    {
        model.Nombres =
            model.Nombres?.Trim()
            ?? string.Empty;

        model.Apellidos =
            model.Apellidos?.Trim()
            ?? string.Empty;

        model.Username =
            model.Username?.Trim()
            ?? string.Empty;

        model.Rol =
            model.Rol?.Trim()
                .ToUpperInvariant()
            ?? string.Empty;

        model.Estado =
            model.Estado?.Trim()
                .ToUpperInvariant()
            ?? "ACTIVO";


        // =================================================
        // VALIDAR ROL
        // =================================================

        if (
            model.Rol != "CAJERO" &&
            model.Rol != "ADMINISTRADOR"
        )
        {
            ModelState.AddModelError(
                nameof(model.Rol),
                "Seleccione un rol válido."
            );
        }


        // =================================================
        // VALIDAR ESTADO
        // =================================================

        if (
            model.Estado != "ACTIVO" &&
            model.Estado != "INACTIVO"
        )
        {
            ModelState.AddModelError(
                nameof(model.Estado),
                "Seleccione un estado válido."
            );
        }


        // =================================================
        // NORMALIZAR CORREO
        // =================================================

        byte[]? correoHash = null;

        if (
            !string.IsNullOrWhiteSpace(
                model.Correo
            )
        )
        {
            model.Correo =
                CorreoHelper.Normalizar(
                    model.Correo
                );

            correoHash =
                CorreoHelper.CalcularHash(
                    model.Correo
                );
        }


        // =================================================
        // VALIDAR USERNAME DUPLICADO
        // =================================================

        if (
            !string.IsNullOrWhiteSpace(
                model.Username
            )
        )
        {
            bool usernameExiste =
                await _context.Usuarios
                    .AnyAsync(u =>
                        u.Username ==
                        model.Username
                    );

            if (usernameExiste)
            {
                ModelState.AddModelError(
                    nameof(model.Username),
                    "Ese nombre de usuario ya existe."
                );
            }
        }


        // =================================================
        // VALIDAR CORREO DUPLICADO
        // =================================================

        if (
            correoHash != null &&
            await ExisteCorreoAsync(
                correoHash
            )
        )
        {
            ModelState.AddModelError(
                nameof(model.Correo),
                "El correo electrónico ya está asociado a otra cuenta."
            );
        }


        if (!ModelState.IsValid)
        {
            await CargarPermisos(
                model
            );

            return View(model);
        }


        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        try
        {
            // =================================================
            // CREAR USUARIO
            // =================================================

            var usuario =
                new Usuario
                {
                    Nombres =
                        model.Nombres,

                    Apellidos =
                        model.Apellidos,

                    Username =
                        model.Username,

                    CorreoCifrado =
                        CorreoHelper.ConvertirABytes(
                            model.Correo
                        ),

                    CorreoHash =
                        correoHash,

                    Rol =
                        model.Rol,

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


            // =================================================
            // SI ES CAJERO:
            // GUARDAR PERMISOS
            // =================================================

            var codigosPermisos =
                new List<string>();


            if (
                usuario.Rol ==
                "CAJERO"
            )
            {
                var idsSeleccionados =
                    model.Permisos
                        .Where(p =>
                            p.Seleccionado
                        )
                        .Select(p =>
                            p.IdPermiso
                        )
                        .Distinct()
                        .ToList();


                var idsFinales =
                    await ObtenerPermisosConDependencias(
                        idsSeleccionados
                    );


                foreach (
                    uint idPermiso
                    in idsFinales
                )
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


                codigosPermisos =
                    await ObtenerCodigosPermisosAsync(
                        idsFinales
                    );
            }


            // =================================================
            // ADMINISTRADOR
            //
            // NO SE INSERTAN PERMISOS.
            // SU ROL YA LE DA ACCESO COMPLETO.
            // =================================================


            await transaction.CommitAsync();


            // =================================================
            // AUDITORÍA
            // =================================================

            string descripcion;


            if (
                usuario.Rol ==
                "ADMINISTRADOR"
            )
            {
                descripcion =
                    $"Usuario {usuario.Username} creado; " +
                    $"Nombre: {usuario.Nombres} {usuario.Apellidos}; " +
                    $"Rol: ADMINISTRADOR; " +
                    $"Correo: Registrado; " +
                    $"Estado: {usuario.Estado}; " +
                    "Acceso: Completo.";
            }
            else
            {
                string permisosTexto =
                    codigosPermisos.Count > 0
                        ? string.Join(
                            ", ",
                            codigosPermisos
                        )
                        : "Sin permisos asignados";


                descripcion =
                    $"Usuario {usuario.Username} creado; " +
                    $"Nombre: {usuario.Nombres} {usuario.Apellidos}; " +
                    $"Rol: CAJERO; " +
                    $"Correo: Registrado; " +
                    $"Estado: {usuario.Estado}; " +
                    $"Permisos iniciales: {permisosTexto}.";
            }


            await RegistrarAuditoriaSeguroAsync(
                "USUARIO_CREADO",
                "usuarios",
                usuario.IdUsuario.ToString(),
                descripcion
            );


            TempData["Exito"] =
                usuario.Rol == "ADMINISTRADOR"
                    ? "El administrador fue creado correctamente."
                    : "El cajero fue creado correctamente.";


            return RedirectToAction(
                nameof(Index)
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();


            _logger.LogError(
                ex,
                "No fue posible crear el usuario."
            );


            ModelState.AddModelError(
                string.Empty,
                $"No fue posible crear el usuario: {ex.Message}"
            );


            await CargarPermisos(
                model
            );


            return View(model);
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
                    (
                        u.Rol == "ADMINISTRADOR" ||
                        u.Rol == "CAJERO"
                    )
                );


        if (usuario == null)
        {
            return NotFound();
        }


        var model =
            new EditarUsuarioViewModel
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
                    CorreoHelper.LeerCorreo(
                        usuario.CorreoCifrado
                    )
                    ?? string.Empty,

                Rol =
                    usuario.Rol,

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
        EditarUsuarioViewModel model)
    {
        var usuario =
            await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario ==
                        model.IdUsuario &&
                    (
                        u.Rol ==
                            "ADMINISTRADOR" ||
                        u.Rol ==
                            "CAJERO"
                    )
                );


        if (usuario == null)
        {
            return NotFound();
        }


        // =================================================
        // EL ROL NO SE CAMBIA DESDE EDITAR
        // =================================================

        ModelState.Remove(
            nameof(model.Rol)
        );

        model.Rol =
            usuario.Rol;


        model.Nombres =
            model.Nombres?.Trim()
            ?? string.Empty;

        model.Apellidos =
            model.Apellidos?.Trim()
            ?? string.Empty;

        model.Username =
            model.Username?.Trim()
            ?? string.Empty;

        model.Estado =
            model.Estado?.Trim()
                .ToUpperInvariant()
            ?? usuario.Estado;


        // =================================================
        // EVITAR QUE EL ADMINISTRADOR ACTUAL
        // SE INACTIVE A SÍ MISMO
        // =================================================

        if (
            usuario.Rol ==
                "ADMINISTRADOR" &&
            EsUsuarioActual(
                usuario.IdUsuario
            ) &&
            model.Estado !=
                "ACTIVO"
        )
        {
            ModelState.AddModelError(
                nameof(model.Estado),
                "No puedes inactivar tu propia cuenta mientras tienes la sesión iniciada."
            );
        }


        // =================================================
        // VALIDAR ESTADO
        // =================================================

        if (
            model.Estado != "ACTIVO" &&
            model.Estado != "INACTIVO"
        )
        {
            ModelState.AddModelError(
                nameof(model.Estado),
                "Seleccione un estado válido."
            );
        }


        // =================================================
        // USERNAME DUPLICADO
        // =================================================

        bool usernameDuplicado =
            await _context.Usuarios
                .AnyAsync(u =>
                    u.Username ==
                        model.Username &&
                    u.IdUsuario !=
                        usuario.IdUsuario
                );


        if (usernameDuplicado)
        {
            ModelState.AddModelError(
                nameof(model.Username),
                "Ese nombre de usuario ya está siendo utilizado."
            );
        }


        // =================================================
        // CORREO
        // =================================================

        byte[]? correoHashNuevo =
            null;


        if (
            !string.IsNullOrWhiteSpace(
                model.Correo
            )
        )
        {
            model.Correo =
                CorreoHelper.Normalizar(
                    model.Correo
                );


            correoHashNuevo =
                CorreoHelper.CalcularHash(
                    model.Correo
                );


            if (
                await ExisteCorreoAsync(
                    correoHashNuevo,
                    usuario.IdUsuario
                )
            )
            {
                ModelState.AddModelError(
                    nameof(model.Correo),
                    "El correo electrónico ya está asociado a otra cuenta."
                );
            }
        }


        // =================================================
        // CONTRASEÑA
        // =================================================

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


        // =================================================
        // VALORES ANTERIORES
        // =================================================

        string nombresAnterior =
            usuario.Nombres;

        string apellidosAnterior =
            usuario.Apellidos;

        string usernameAnterior =
            usuario.Username;

        string estadoAnterior =
            usuario.Estado;

        string? correoAnterior =
            CorreoHelper.LeerCorreo(
                usuario.CorreoCifrado
            );


        // =================================================
        // CAMBIOS
        // =================================================

        var cambios =
            new List<string>();


        AgregarCambio(
            cambios,
            "Nombres",
            nombresAnterior,
            model.Nombres
        );


        AgregarCambio(
            cambios,
            "Apellidos",
            apellidosAnterior,
            model.Apellidos
        );


        AgregarCambio(
            cambios,
            "Usuario",
            usernameAnterior,
            model.Username
        );


        AgregarCambioProtegido(
            cambios,
            "Correo",
            correoAnterior,
            model.Correo
        );


        AgregarCambio(
            cambios,
            "Estado",
            estadoAnterior,
            model.Estado
        );


        bool contrasenaActualizada =
            !string.IsNullOrWhiteSpace(
                model.NuevaContrasena
            );


        if (contrasenaActualizada)
        {
            cambios.Add(
                "Contraseña: Sin cambios → Actualizada"
            );
        }


        try
        {
            // =================================================
            // ACTUALIZAR
            // =================================================

            usuario.Nombres =
                model.Nombres;

            usuario.Apellidos =
                model.Apellidos;

            usuario.Username =
                model.Username;

            usuario.Estado =
                model.Estado;

            usuario.CorreoCifrado =
                CorreoHelper.ConvertirABytes(
                    model.Correo
                );

            usuario.CorreoHash =
                correoHashNuevo;


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


            // =================================================
            // AUDITORÍA
            // =================================================

            string descripcion =
                cambios.Count > 0
                    ? $"Usuario {usuario.Username} actualizado; " +
                      $"Rol: {usuario.Rol}; " +
                      $"Cambios: {string.Join("; ", cambios)}."
                    : $"Usuario {usuario.Username} revisado; " +
                      $"Rol: {usuario.Rol}; " +
                      "No se detectaron cambios.";


            await RegistrarAuditoriaSeguroAsync(
                "USUARIO_EDITADO",
                "usuarios",
                usuario.IdUsuario.ToString(),
                descripcion
            );


            TempData["Exito"] =
                "El usuario fue actualizado correctamente.";


            return RedirectToAction(
                nameof(Index)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No fue posible actualizar el usuario {IdUsuario}.",
                usuario.IdUsuario
            );


            ModelState.AddModelError(
                string.Empty,
                $"No fue posible actualizar el usuario: {ex.Message}"
            );


            return View(model);
        }
    }


    // =====================================================
    // PERMISOS - GET
    // SOLO CAJERO
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
                    up.IdUsuario == id
                )
                .Select(up =>
                    up.IdPermiso
                )
                .ToListAsync();


        var permisos =
            await _context.Permisos
                .AsNoTracking()
                .Where(p =>
                    p.Estado == "ACTIVO"
                )
                .OrderBy(p =>
                    p.Modulo
                )
                .ThenBy(p =>
                    p.Nombre
                )
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
                    }
                )
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
                    u.Rol ==
                        "CAJERO"
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
            var actuales =
                await _context.UsuarioPermisos
                    .Where(up =>
                        up.IdUsuario ==
                            usuario.IdUsuario
                    )
                    .ToListAsync();


            var idsAnteriores =
                actuales
                    .Select(up =>
                        up.IdPermiso
                    )
                    .ToHashSet();


            _context.UsuarioPermisos
                .RemoveRange(
                    actuales
                );


            var idsSeleccionados =
                model.Permisos
                    .Where(p =>
                        p.Seleccionado
                    )
                    .Select(p =>
                        p.IdPermiso
                    )
                    .Distinct()
                    .ToList();


            var idsFinales =
                await ObtenerPermisosConDependencias(
                    idsSeleccionados
                );


            foreach (
                uint idPermiso
                in idsFinales
            )
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


            // =================================================
            // COMPARAR PERMISOS
            // =================================================

            var idsFinalesSet =
                idsFinales.ToHashSet();


            var idsAgregados =
                idsFinalesSet
                    .Except(
                        idsAnteriores
                    )
                    .ToList();


            var idsRetirados =
                idsAnteriores
                    .Except(
                        idsFinalesSet
                    )
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


            if (
                codigosAgregados.Count > 0
            )
            {
                cambios.Add(
                    "Permisos agregados: " +
                    "Sin dato → " +
                    string.Join(
                        ", ",
                        codigosAgregados
                    )
                );
            }


            if (
                codigosRetirados.Count > 0
            )
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
                "Los permisos fueron actualizados correctamente.";


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
    // CARGAR PERMISOS
    // =====================================================

    private async Task CargarPermisos(
        CrearUsuarioViewModel model)
    {
        var seleccionados =
            model.Permisos
                .Where(p =>
                    p.Seleccionado
                )
                .Select(p =>
                    p.IdPermiso
                )
                .ToHashSet();


        model.Permisos =
            await _context.Permisos
                .AsNoTracking()
                .Where(p =>
                    p.Estado == "ACTIVO"
                )
                .OrderBy(p =>
                    p.Modulo
                )
                .ThenBy(p =>
                    p.Nombre
                )
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
                    }
                )
                .ToListAsync();
    }


    // =====================================================
    // DEPENDENCIAS DE PERMISOS
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
                    p.Estado ==
                    "ACTIVO"
                )
                .Select(p =>
                    new
                    {
                        p.IdPermiso,
                        p.Codigo
                    }
                )
                .ToListAsync();


        var codigoPorId =
            permisosActivos
                .ToDictionary(
                    p => p.IdPermiso,
                    p => p.Codigo
                );


        var idPorCodigo =
            permisosActivos
                .ToDictionary(
                    p => p.Codigo,
                    p => p.IdPermiso,
                    StringComparer
                        .OrdinalIgnoreCase
                );


        var codigosSeleccionados =
            ids
                .Where(id =>
                    codigoPorId
                        .ContainsKey(id)
                )
                .Select(id =>
                    codigoPorId[id]
                )
                .ToHashSet(
                    StringComparer
                        .OrdinalIgnoreCase
                );


        void AgregarDependencia(
            string hijo,
            string padre)
        {
            if (
                codigosSeleccionados
                    .Contains(hijo) &&
                idPorCodigo.TryGetValue(
                    padre,
                    out uint idPadre
                )
            )
            {
                ids.Add(
                    idPadre
                );

                codigosSeleccionados.Add(
                    padre
                );
            }
        }


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

        AgregarDependencia(
            "CREDITOS_CREAR",
            "CREDITOS_VER"
        );

        AgregarDependencia(
            "CREDITOS_DETALLE",
            "CREDITOS_VER"
        );

        AgregarDependencia(
            "PAGOS_ABONO_CAPITAL",
            "PAGOS_REGISTRAR"
        );

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
                )
            )
            .Select(p =>
                p.IdPermiso
            )
            .ToList();
    }


    // =====================================================
    // OBTENER CÓDIGOS
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
                )
            )
            .OrderBy(p =>
                p.Modulo
            )
            .ThenBy(p =>
                p.Codigo
            )
            .Select(p =>
                p.Codigo
            )
            .ToListAsync();
    }


    // =====================================================
    // VALIDAR CORREO GLOBAL
    // =====================================================

    private async Task<bool> ExisteCorreoAsync(
        byte[] correoHash,
        uint? excluirIdUsuario = null)
    {
        var hashes =
            await _context.Usuarios
                .AsNoTracking()
                .Where(u =>
                    u.CorreoHash != null &&
                    (
                        !excluirIdUsuario.HasValue ||
                        u.IdUsuario !=
                            excluirIdUsuario.Value
                    )
                )
                .Select(u =>
                    u.CorreoHash!
                )
                .ToListAsync();


        return hashes.Any(hash =>
            hash.Length ==
                correoHash.Length &&
            CryptographicOperations
                .FixedTimeEquals(
                    hash,
                    correoHash
                )
        );
    }


    // =====================================================
    // USUARIO ACTUAL
    // =====================================================

    private bool EsUsuarioActual(
        uint idUsuario)
    {
        string? idTexto =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );


        return
            uint.TryParse(
                idTexto,
                out uint idActual
            ) &&
            idActual ==
                idUsuario;
    }


    // =====================================================
    // AUDITORÍA
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
                "No fue posible registrar la auditoría {Accion}.",
                accion
            );
        }
    }


    // =====================================================
    // CAMBIOS DE AUDITORÍA
    // =====================================================

    private static void AgregarCambio(
        ICollection<string> cambios,
        string campo,
        string? anterior,
        string? nuevo)
    {
        string anteriorNormalizado =
            NormalizarParaComparar(
                anterior
            );


        string nuevoNormalizado =
            NormalizarParaComparar(
                nuevo
            );


        if (
            string.Equals(
                anteriorNormalizado,
                nuevoNormalizado,
                StringComparison.Ordinal
            )
        )
        {
            return;
        }


        cambios.Add(
            $"{campo}: " +
            $"{MostrarValor(anterior)} → " +
            $"{MostrarValor(nuevo)}"
        );
    }


    private static void AgregarCambioProtegido(
        ICollection<string> cambios,
        string campo,
        string? anterior,
        string? nuevo)
    {
        string anteriorNormalizado =
            NormalizarParaComparar(
                anterior
            );


        string nuevoNormalizado =
            NormalizarParaComparar(
                nuevo
            );


        if (
            string.Equals(
                anteriorNormalizado,
                nuevoNormalizado,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return;
        }


        string valorAnterior =
            string.IsNullOrWhiteSpace(
                anterior
            )
                ? "Sin dato"
                : "Dato registrado";


        string valorNuevo =
            string.IsNullOrWhiteSpace(
                nuevo
            )
                ? "Sin dato"
                : "Dato actualizado";


        cambios.Add(
            $"{campo}: " +
            $"{valorAnterior} → " +
            $"{valorNuevo}"
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