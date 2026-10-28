using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Security;
using SIGEMIC.Services;
using SIGEMIC.ViewModels;

namespace SIGEMIC.Controllers;

[Authorize]
public class PerfilController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly AuditoriaService _auditoriaService;
    private readonly ILogger<PerfilController> _logger;

    public PerfilController(
        ApplicationDbContext context,
        AuditoriaService auditoriaService,
        ILogger<PerfilController> logger)
    {
        _context = context;
        _auditoriaService = auditoriaService;
        _logger = logger;
    }

    // =====================================================
    // PERFIL DEL ADMINISTRADOR
    // =====================================================

    [HttpGet]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Administrador()
    {
        uint? idUsuario =
            ObtenerIdUsuarioActual();

        if (!idUsuario.HasValue)
        {
            return Unauthorized();
        }

        var usuario =
            await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario ==
                        idUsuario.Value &&
                    u.Rol ==
                        "ADMINISTRADOR"
                );

        if (usuario == null)
        {
            return NotFound();
        }

        var model =
            new PerfilAdministradorViewModel
            {
                IdUsuario =
                    usuario.IdUsuario,

                Nombres =
                    usuario.Nombres,

                Apellidos =
                    usuario.Apellidos,

                Username =
                    usuario.Username,

                Rol =
                    usuario.Rol,

                Estado =
                    usuario.Estado,

                Correo =
                    CorreoHelper.LeerCorreo(
                        usuario.CorreoCifrado
                    )
                    ?? string.Empty
            };

        return View(model);
    }

    // =====================================================
    // ACTUALIZAR PERFIL
    // =====================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMINISTRADOR")]
    public async Task<IActionResult> Administrador(
        PerfilAdministradorViewModel model)
    {
        uint? idUsuario =
            ObtenerIdUsuarioActual();

        if (!idUsuario.HasValue)
        {
            return Unauthorized();
        }

        var usuario =
            await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario ==
                        idUsuario.Value &&
                    u.Rol ==
                        "ADMINISTRADOR"
                );

        if (usuario == null)
        {
            return NotFound();
        }

        // =============================================
        // NO CONFIAR EN DATOS DEL FORMULARIO
        // =============================================

        model.IdUsuario =
            usuario.IdUsuario;

        model.Nombres =
            usuario.Nombres;

        model.Apellidos =
            usuario.Apellidos;

        model.Username =
            usuario.Username;

        model.Rol =
            usuario.Rol;

        model.Estado =
            usuario.Estado;

        // =============================================
        // VALIDAR MODELO
        // =============================================

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.Correo =
            CorreoHelper.Normalizar(
                model.Correo
            );

        byte[] correoHashNuevo =
            CorreoHelper.CalcularHash(
                model.Correo
            );

        // =============================================
        // VALIDAR CORREO DUPLICADO
        // =============================================

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

            return View(model);
        }

        string? correoAnterior =
            CorreoHelper.LeerCorreo(
                usuario.CorreoCifrado
            );

        // =============================================
        // SIN CAMBIOS
        // =============================================

        if (
            string.Equals(
                CorreoHelper.Normalizar(
                    correoAnterior
                    ?? string.Empty
                ),
                model.Correo,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            TempData["Exito"] =
                "No se detectaron cambios en el correo.";

            return RedirectToAction(
                nameof(Administrador)
            );
        }

        try
        {
            usuario.CorreoCifrado =
                CorreoHelper.ConvertirABytes(
                    model.Correo
                );

            usuario.CorreoHash =
                correoHashNuevo;

            await _context.SaveChangesAsync();

            // =========================================
            // AUDITORÍA
            // No guardamos el correo real.
            // =========================================

            string estadoAnterior =
                string.IsNullOrWhiteSpace(
                    correoAnterior
                )
                    ? "Sin dato"
                    : "Dato registrado";

            await RegistrarAuditoriaSeguroAsync(
                "PERFIL_ADMIN_ACTUALIZADO",
                "usuarios",
                usuario.IdUsuario.ToString(),

                $"Perfil del administrador {usuario.Username} actualizado. " +
                $"Cambios: Correo: {estadoAnterior} → Dato actualizado."
            );

            TempData["Exito"] =
                "El correo del administrador fue actualizado correctamente.";

            return RedirectToAction(
                nameof(Administrador)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No fue posible actualizar el perfil del administrador {IdUsuario}.",
                usuario.IdUsuario
            );

            ModelState.AddModelError(
                string.Empty,
                "No fue posible actualizar el perfil."
            );

            return View(model);
        }
    }

    // =====================================================
    // ID DEL USUARIO ACTUAL
    // =====================================================

    private uint? ObtenerIdUsuarioActual()
    {
        string? idTexto =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        if (
            uint.TryParse(
                idTexto,
                out uint idUsuario
            )
        )
        {
            return idUsuario;
        }

        return null;
    }

    // =====================================================
    // VALIDAR CORREO GLOBAL
    // =====================================================

    private async Task<bool> ExisteCorreoAsync(
        byte[] correoHash,
        uint excluirIdUsuario)
    {
        var hashes =
            await _context.Usuarios
                .AsNoTracking()
                .Where(u =>
                    u.IdUsuario !=
                        excluirIdUsuario &&
                    u.CorreoHash != null
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
}