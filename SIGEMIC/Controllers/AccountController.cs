using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Models;
using SIGEMIC.Services;
using System.Security.Claims;

namespace SIGEMIC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditoriaService _auditoriaService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            ApplicationDbContext context,
            AuditoriaService auditoriaService,
            ILogger<AccountController> logger)
        {
            _context = context;
            _auditoriaService = auditoriaService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await RegistrarAuditoriaSeguroAsync(
                    null,
                    "LOGIN_FALLIDO",
                    "usuarios",
                    model.Username,
                    $"Intento de inicio de sesión con datos incompletos. Usuario ingresado: {model.Username}.",
                    "FALLIDO"
                );

                return View(model);
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Username == model.Username &&
                    u.Estado == "ACTIVO"
                );

            if (usuario == null)
            {
                await RegistrarAuditoriaSeguroAsync(
                    null,
                    "LOGIN_FALLIDO",
                    "usuarios",
                    model.Username,
                    $"Intento de inicio de sesión fallido para el usuario: {model.Username}.",
                    "FALLIDO"
                );

                ModelState.AddModelError(
                    string.Empty,
                    "Usuario o contraseña incorrectos."
                );

                return View(model);
            }

            var hasher = new PasswordHasher<Usuario>();

            var resultado = hasher.VerifyHashedPassword(
                usuario,
                usuario.PasswordHash,
                model.Password
            );

            if (resultado == PasswordVerificationResult.Failed)
            {
                if (usuario.IntentosFallidos < byte.MaxValue)
                {
                    usuario.IntentosFallidos++;
                }

                await _context.SaveChangesAsync();

                await RegistrarAuditoriaSeguroAsync(
                    usuario.IdUsuario,
                    "LOGIN_FALLIDO",
                    "usuarios",
                    usuario.IdUsuario.ToString(),
                    $"Contraseña incorrecta para el usuario {usuario.Username}. Intentos fallidos acumulados: {usuario.IntentosFallidos}.",
                    "FALLIDO"
                );

                ModelState.AddModelError(
                    string.Empty,
                    "Usuario o contraseña incorrectos."
                );

                return View(model);
            }

            usuario.IntentosFallidos = 0;
            usuario.UltimoAcceso = DateTime.Now;

            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    usuario.Username
                ),

                new Claim(
                    ClaimTypes.Role,
                    usuario.Rol
                )
            };

            var identity = new ClaimsIdentity(
                claims,
                "SIGEMICCookie"
            );

            var principal = new ClaimsPrincipal(
                identity
            );

            await HttpContext.SignInAsync(
                "SIGEMICCookie",
                principal
            );

            await RegistrarAuditoriaSeguroAsync(
                usuario.IdUsuario,
                "LOGIN_EXITOSO",
                "usuarios",
                usuario.IdUsuario.ToString(),
                $"Inicio de sesión exitoso. Usuario: {usuario.Username}. Rol: {usuario.Rol}.",
                "EXITOSO"
            );

            if (usuario.Rol == "ADMINISTRADOR")
            {
                return RedirectToAction(
                    "Index",
                    "Admin"
                );
            }

            if (usuario.Rol == "CAJERO")
            {
                return RedirectToAction(
                    "Index",
                    "Cajero"
                );
            }

            if (usuario.Rol == "CLIENTE")
            {
                return RedirectToAction(
                    "Index",
                    "Cliente"
                );
            }

            return RedirectToAction(
                "Index",
                "Home"
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            uint? idUsuario = null;

            var idTexto = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (uint.TryParse(
                idTexto,
                out uint idObtenido))
            {
                idUsuario = idObtenido;
            }

            var username =
                User.Identity?.Name ??
                "DESCONOCIDO";

            await RegistrarAuditoriaSeguroAsync(
                idUsuario,
                "LOGOUT",
                "usuarios",
                idUsuario?.ToString(),
                $"Cierre de sesión del usuario: {username}.",
                "EXITOSO"
            );

            await HttpContext.SignOutAsync(
                "SIGEMICCookie"
            );

            return RedirectToAction(
                "Login",
                "Account"
            );
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            Response.StatusCode = 403;

            return View(
                "~/Views/Shared/AccessDenied.cshtml"
            );
        }

        private async Task RegistrarAuditoriaSeguroAsync(
            uint? idUsuario,
            string accion,
            string? tablaAfectada,
            string? registroAfectado,
            string? descripcion,
            string resultado)
        {
            try
            {
                await _auditoriaService.RegistrarAsync(
                    idUsuario,
                    accion,
                    tablaAfectada,
                    registroAfectado,
                    descripcion,
                    resultado
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
    }
}