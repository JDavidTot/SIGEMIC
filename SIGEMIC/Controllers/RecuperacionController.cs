using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Models;
using SIGEMIC.Security;
using SIGEMIC.ViewModels;

namespace SIGEMIC.Controllers;

[AllowAnonymous]
public class RecuperacionController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RecuperacionController> _logger;

    public RecuperacionController(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<RecuperacionController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }


    // =====================================================
    // OLVIDÉ MI CONTRASEÑA - GET
    // =====================================================

    [HttpGet]
    public IActionResult OlvidePassword()
    {
        if (
            User.Identity?.IsAuthenticated ==
            true
        )
        {
            return RedirectToAction(
                "Index",
                "Home"
            );
        }

        return View(
            new OlvidePasswordViewModel()
        );
    }


    // =====================================================
    // OLVIDÉ MI CONTRASEÑA - POST
    // =====================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OlvidePassword(
        OlvidePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string username =
            model.Usuario.Trim();

        // =================================================
        // BUSCAR USUARIO
        // =================================================

        var usuario =
            await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Username == username &&
                    u.Estado == "ACTIVO"
                );

        /*
         * MUY IMPORTANTE:
         *
         * No mostramos si el usuario existe o no.
         * Esto evita enumeración de cuentas.
         */

        if (
            usuario != null &&
            usuario.CorreoCifrado != null &&
            usuario.CorreoHash != null
        )
        {
            try
            {
                // =========================================
                // EVITAR SOLICITUDES DEMASIADO SEGUIDAS
                // =========================================

                DateTime limiteReciente =
                    DateTime.Now.AddMinutes(-2);

                bool existeReciente =
                    await _context
                        .RecuperacionesPassword
                        .AnyAsync(r =>
                            r.IdUsuario ==
                                usuario.IdUsuario &&
                            !r.Utilizado &&
                            r.FechaSolicitud >=
                                limiteReciente
                        );

                if (!existeReciente)
                {
                    // =====================================
                    // INVALIDAR TOKENS ANTERIORES
                    // =====================================

                    var anteriores =
                        await _context
                            .RecuperacionesPassword
                            .Where(r =>
                                r.IdUsuario ==
                                    usuario.IdUsuario &&
                                !r.Utilizado
                            )
                            .ToListAsync();

                    foreach (
                        var anterior
                        in anteriores
                    )
                    {
                        anterior.Utilizado =
                            true;

                        anterior.FechaUtilizacion =
                            DateTime.Now;
                    }


                    // =====================================
                    // GENERAR TOKEN SEGURO
                    // =====================================

                    byte[] tokenBytes =
                        RandomNumberGenerator
                            .GetBytes(32);

                    string token =
                        WebEncoders
                            .Base64UrlEncode(
                                tokenBytes
                            );


                    // =====================================
                    // GUARDAR SOLO EL HASH
                    // =====================================

                    byte[] tokenHash =
                        SHA256.HashData(
                            Encoding.UTF8.GetBytes(
                                token
                            )
                        );


                    string? direccionIp =
                        HttpContext
                            .Connection
                            .RemoteIpAddress?
                            .ToString();


                    var recuperacion =
                        new RecuperacionPassword
                        {
                            IdUsuario =
                                usuario.IdUsuario,

                            TokenHash =
                                tokenHash,

                            FechaSolicitud =
                                DateTime.Now,

                            FechaExpiracion =
                                DateTime.Now
                                    .AddMinutes(30),

                            Utilizado =
                                false,

                            DireccionIp =
                                direccionIp
                        };


                    _context
                        .RecuperacionesPassword
                        .Add(
                            recuperacion
                        );

                    await _context
                        .SaveChangesAsync();


                    // =====================================
                    // CREAR ENLACE
                    // =====================================

                    string? enlace =
                        Url.Action(
                            nameof(
                                RestablecerPassword
                            ),
                            "Recuperacion",
                            new
                            {
                                token
                            },
                            Request.Scheme
                        );


                    string? correo =
                        CorreoHelper.LeerCorreo(
                            usuario.CorreoCifrado
                        );


                    // =====================================
                    // ENVIAR CORREO
                    // =====================================

                    bool enviado =
                        await EnviarCorreoRecuperacionAsync(
                            correo,
                            usuario.Nombres,
                            enlace
                        );


                    // =====================================
                    // SI NO SE PUDO ENVIAR
                    // INVALIDAR EL TOKEN
                    // =====================================

                    if (!enviado)
                    {
                        recuperacion.Utilizado =
                            true;

                        recuperacion.FechaUtilizacion =
                            DateTime.Now;

                        await _context
                            .SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                /*
                 * Nunca mostramos el error técnico
                 * porque eso podría revelar si la
                 * cuenta existe.
                 */

                _logger.LogError(
                    ex,
                    "Error durante una solicitud de recuperación de contraseña."
                );
            }
        }


        // =================================================
        // RESPUESTA GENÉRICA
        // =================================================

        return RedirectToAction(
            nameof(Confirmacion)
        );
    }


    // =====================================================
    // CONFIRMACIÓN
    // =====================================================

    [HttpGet]
    public IActionResult Confirmacion()
    {
        return View();
    }


    // =====================================================
    // RESTABLECER - GET
    // =====================================================

    [HttpGet]
    public async Task<IActionResult> RestablecerPassword(
        string? token)
    {
        if (
            string.IsNullOrWhiteSpace(
                token
            )
        )
        {
            return View(
                "EnlaceInvalido"
            );
        }


        var recuperacion =
            await BuscarTokenValidoAsync(
                token
            );


        if (recuperacion == null)
        {
            return View(
                "EnlaceInvalido"
            );
        }


        var model =
            new RestablecerPasswordViewModel
            {
                Token =
                    token
            };


        return View(model);
    }


    // =====================================================
    // RESTABLECER - POST
    // =====================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestablecerPassword(
        RestablecerPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }


        var recuperacion =
            await BuscarTokenValidoAsync(
                model.Token
            );


        if (recuperacion == null)
        {
            return View(
                "EnlaceInvalido"
            );
        }


        var usuario =
            await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario ==
                        recuperacion.IdUsuario
                );


        if (usuario == null)
        {
            return View(
                "EnlaceInvalido"
            );
        }


        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();


        try
        {
            // =========================================
            // NUEVA CONTRASEÑA
            // =========================================

            var hasher =
                new PasswordHasher<Usuario>();

            usuario.PasswordHash =
                hasher.HashPassword(
                    usuario,
                    model.NuevaContrasena
                );


            // =========================================
            // REINICIAR INTENTOS FALLIDOS
            // =========================================

            usuario.IntentosFallidos =
                0;


            // =========================================
            // MARCAR TOKEN COMO UTILIZADO
            // =========================================

            recuperacion.Utilizado =
                true;

            recuperacion.FechaUtilizacion =
                DateTime.Now;


            // =========================================
            // INVALIDAR TODOS LOS DEMÁS TOKENS
            // =========================================

            var otrosTokens =
                await _context
                    .RecuperacionesPassword
                    .Where(r =>
                        r.IdUsuario ==
                            usuario.IdUsuario &&
                        r.IdRecuperacion !=
                            recuperacion.IdRecuperacion &&
                        !r.Utilizado
                    )
                    .ToListAsync();


            foreach (
                var otro
                in otrosTokens
            )
            {
                otro.Utilizado =
                    true;

                otro.FechaUtilizacion =
                    DateTime.Now;
            }


            await _context
                .SaveChangesAsync();


            await transaction
                .CommitAsync();


            _logger.LogInformation(
                "Contraseña restablecida para el usuario ID {IdUsuario}.",
                usuario.IdUsuario
            );


            return RedirectToAction(
                nameof(PasswordActualizado)
            );
        }
        catch (Exception ex)
        {
            await transaction
                .RollbackAsync();


            _logger.LogError(
                ex,
                "Error al restablecer la contraseña del usuario ID {IdUsuario}.",
                usuario.IdUsuario
            );


            ModelState.AddModelError(
                string.Empty,
                "No fue posible actualizar la contraseña."
            );


            return View(model);
        }
    }


    // =====================================================
    // PASSWORD ACTUALIZADO
    // =====================================================

    [HttpGet]
    public IActionResult PasswordActualizado()
    {
        return View();
    }


    // =====================================================
    // BUSCAR TOKEN VÁLIDO
    // =====================================================

    private async Task<RecuperacionPassword?>
        BuscarTokenValidoAsync(
            string token)
    {
        byte[] hashBuscado =
            SHA256.HashData(
                Encoding.UTF8.GetBytes(
                    token
                )
            );


        DateTime ahora =
            DateTime.Now;


        /*
         * Traemos únicamente tokens que
         * todavía podrían ser válidos.
         *
         * Luego hacemos comparación en memoria
         * con FixedTimeEquals.
         */

        var candidatos =
            await _context
                .RecuperacionesPassword
                .Where(r =>
                    !r.Utilizado &&
                    r.FechaExpiracion >=
                        ahora
                )
                .ToListAsync();


        foreach (
            var recuperacion
            in candidatos
        )
        {
            if (
                recuperacion.TokenHash.Length ==
                    hashBuscado.Length &&
                CryptographicOperations
                    .FixedTimeEquals(
                        recuperacion.TokenHash,
                        hashBuscado
                    )
            )
            {
                return recuperacion;
            }
        }


        return null;
    }


    // =====================================================
    // ENVÍO DE CORREO
    // =====================================================

    private async Task<bool>
        EnviarCorreoRecuperacionAsync(
            string? destinatario,
            string nombre,
            string? enlace)
    {
        if (
            string.IsNullOrWhiteSpace(
                destinatario
            ) ||
            string.IsNullOrWhiteSpace(
                enlace
            )
        )
        {
            return false;
        }


        string? host =
            _configuration[
                "Correo:SmtpHost"
            ];

        string? puertoTexto =
            _configuration[
                "Correo:SmtpPort"
            ];

        string? usuarioSmtp =
            _configuration[
                "Correo:Usuario"
            ];

        string? passwordSmtp =
            _configuration[
                "Correo:Password"
            ];

        string? remitente =
            _configuration[
                "Correo:Remitente"
            ];


        if (
            string.IsNullOrWhiteSpace(
                host
            ) ||
            !int.TryParse(
                puertoTexto,
                out int puerto
            ) ||
            string.IsNullOrWhiteSpace(
                usuarioSmtp
            ) ||
            string.IsNullOrWhiteSpace(
                passwordSmtp
            ) ||
            string.IsNullOrWhiteSpace(
                remitente
            )
        )
        {
            _logger.LogWarning(
                "La configuración SMTP de SIGEMIC no está completa."
            );

            return false;
        }


        try
        {
            using var mensaje =
                new MailMessage();


            mensaje.From =
                new MailAddress(
                    remitente,
                    "SIGEMIC"
                );


            mensaje.To.Add(
                destinatario
            );


            mensaje.Subject =
                "Recuperación de contraseña - SIGEMIC";


            mensaje.IsBodyHtml =
                true;


            mensaje.Body =
                $"""
                <div style="
                    font-family:Arial,sans-serif;
                    color:#212529;
                    max-width:600px;
                    margin:auto;
                ">

                    <h2 style="color:#0d6efd;">
                        SIGEMIC
                    </h2>

                    <p>
                        Hola {WebUtility.HtmlEncode(nombre)},
                    </p>

                    <p>
                        Recibimos una solicitud para
                        restablecer la contraseña de
                        tu cuenta.
                    </p>

                    <p>
                        Puedes crear una nueva contraseña
                        utilizando el siguiente enlace:
                    </p>

                    <p style="margin:30px 0;">
                        <a href="{WebUtility.HtmlEncode(enlace)}"
                           style="
                               background:#0d6efd;
                               color:white;
                               padding:12px 22px;
                               border-radius:8px;
                               text-decoration:none;
                           ">
                            Restablecer contraseña
                        </a>
                    </p>

                    <p>
                        Este enlace tiene una vigencia
                        de <strong>30 minutos</strong>
                        y solo puede utilizarse una vez.
                    </p>

                    <p>
                        Si no solicitaste este cambio,
                        puedes ignorar este mensaje.
                    </p>

                    <hr>

                    <small style="color:#6c757d;">
                        Sistema de Gestión de Microcréditos
                        - SIGEMIC
                    </small>

                </div>
                """;


            using var cliente =
                new SmtpClient(
                    host,
                    puerto
                );


            cliente.EnableSsl =
                true;


            cliente.Credentials =
                new NetworkCredential(
                    usuarioSmtp,
                    passwordSmtp
                );


            await cliente
                .SendMailAsync(
                    mensaje
                );


            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No fue posible enviar el correo de recuperación."
            );

            return false;
        }
    }
}