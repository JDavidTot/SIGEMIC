using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Security;
using SIGEMIC.Services;
using System.Security.Claims;

namespace SIGEMIC.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR,CAJERO")]
    public class SolicitudesPagoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly PagoService _pagoService;

        public SolicitudesPagoController(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            PagoService pagoService)
        {
            _context = context;
            _environment = environment;
            _pagoService = pagoService;
        }

        // =====================================================
        // OBTENER ID DEL USUARIO ACTUAL
        // =====================================================
        private uint? ObtenerIdUsuarioActual()
        {
            var idUsuarioTexto =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (uint.TryParse(
                idUsuarioTexto,
                out uint idUsuario))
            {
                return idUsuario;
            }

            return null;
        }

        // =====================================================
        // LISTADO DE SOLICITUDES
        // =====================================================
        [HttpGet]
        [PermisoAuthorize("SOLICITUDES_VER")]
        public async Task<IActionResult> Index(
            string estado = "PENDIENTE")
        {
            var consulta =
                _context.SolicitudesPago
                    .AsNoTracking()
                    .Include(s => s.Cliente)
                        .ThenInclude(c => c.Usuario)
                    .Include(s => s.Credito)
                    .Include(s => s.Cuota)
                    .Include(s => s.Banco)
                    .Include(s => s.UsuarioRevision)
                    .Include(s => s.Pago)
                    .AsQueryable();

            if (
                !string.IsNullOrWhiteSpace(estado) &&
                estado != "TODAS")
            {
                consulta =
                    consulta.Where(s =>
                        s.Estado == estado
                    );
            }

            var solicitudes =
                await consulta
                    .OrderByDescending(s =>
                        s.FechaSolicitud
                    )
                    .ThenByDescending(s =>
                        s.IdSolicitud
                    )
                    .ToListAsync();

            ViewBag.Estado =
                estado;

            ViewBag.Pendientes =
                await _context.SolicitudesPago
                    .CountAsync(s =>
                        s.Estado == "PENDIENTE"
                    );

            return View(solicitudes);
        }

        // =====================================================
        // DETALLE DE SOLICITUD
        // =====================================================
        [HttpGet]
        [PermisoAuthorize("SOLICITUDES_VER")]
        public async Task<IActionResult> Details(
            ulong id)
        {
            var solicitud =
                await _context.SolicitudesPago
                    .AsNoTracking()
                    .Include(s => s.Cliente)
                        .ThenInclude(c => c.Usuario)
                    .Include(s => s.Credito)
                    .Include(s => s.Cuota)
                    .Include(s => s.Banco)
                    .Include(s => s.UsuarioRevision)
                    .Include(s => s.Pago)
                    .FirstOrDefaultAsync(s =>
                        s.IdSolicitud == id
                    );

            if (solicitud == null)
            {
                return NotFound();
            }

            return View(solicitud);
        }

        // =====================================================
        // VER COMPROBANTE
        // =====================================================
        [HttpGet]
        [PermisoAuthorize("SOLICITUDES_VER")]
        public async Task<IActionResult> VerComprobante(
            ulong id)
        {
            var solicitud =
                await _context.SolicitudesPago
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s =>
                        s.IdSolicitud == id
                    );

            if (solicitud == null)
            {
                return NotFound();
            }

            var nombreArchivo =
                Path.GetFileName(
                    solicitud.ArchivoComprobante
                );

            var ruta =
                Path.Combine(
                    _environment.ContentRootPath,
                    "Storage",
                    "ComprobantesPago",
                    nombreArchivo
                );

            if (!System.IO.File.Exists(ruta))
            {
                TempData["Error"] =
                    "El comprobante no fue encontrado.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = solicitud.IdSolicitud
                    }
                );
            }

            var extension =
                Path.GetExtension(ruta)
                    .ToLowerInvariant();

            var contentType =
                extension switch
                {
                    ".jpg" =>
                        "image/jpeg",

                    ".jpeg" =>
                        "image/jpeg",

                    ".png" =>
                        "image/png",

                    ".pdf" =>
                        "application/pdf",

                    _ =>
                        "application/octet-stream"
                };

            return PhysicalFile(
                ruta,
                contentType
            );
        }

        // =====================================================
        // APROBAR SOLICITUD
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermisoAuthorize("SOLICITUDES_APROBAR")]
        public async Task<IActionResult> Aprobar(
            ulong id)
        {
            var idUsuario =
                ObtenerIdUsuarioActual();

            if (!idUsuario.HasValue)
            {
                return Unauthorized();
            }

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                var solicitud =
                    await _context.SolicitudesPago
                        .Include(s => s.Banco)
                        .Include(s => s.Pago)
                        .FirstOrDefaultAsync(s =>
                            s.IdSolicitud == id
                        );

                if (solicitud == null)
                {
                    await transaction.RollbackAsync();

                    return NotFound();
                }

                // =================================================
                // SOLO SOLICITUDES PENDIENTES
                // =================================================
                if (
                    solicitud.Estado !=
                    "PENDIENTE")
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] =
                        "Esta solicitud ya fue revisada.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id }
                    );
                }

                // =================================================
                // EVITAR DOBLE PAGO
                // =================================================
                if (solicitud.IdPago.HasValue)
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] =
                        "Esta solicitud ya tiene un pago asociado.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id }
                    );
                }

                // =================================================
                // FORMA DE PAGO DEPÓSITO
                // =================================================
                var formaPago =
                    await _context.FormasPago
                        .FirstOrDefaultAsync(f =>
                            f.Nombre == "DEPOSITO" &&
                            f.Estado == "ACTIVO"
                        );

                if (formaPago == null)
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] =
                        "No existe una forma de pago DEPOSITO activa en la base de datos.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id }
                    );
                }

                var banco =
                    solicitud.Banco?.Nombre ??
                    solicitud.IdBanco.ToString();

                ResultadoPagoService resultado;

                // =================================================
                // PAGO DE CUOTA
                // =================================================
                if (
                    solicitud.TipoOperacion ==
                    "PAGO_CUOTA")
                {
                    if (!solicitud.IdCuota.HasValue)
                    {
                        await transaction.RollbackAsync();

                        TempData["Error"] =
                            "La solicitud no tiene una cuota asociada.";

                        return RedirectToAction(
                            nameof(Details),
                            new { id }
                        );
                    }

                    var observacion =
                        $"Pago aprobado desde solicitud #{solicitud.IdSolicitud}. " +
                        $"Banco: {banco}. " +
                        $"Boleta: {solicitud.NumeroBoleta}.";

                    resultado =
                        await _pagoService
                            .RegistrarPagoCuotaAsync(
                                idCuota:
                                    solicitud.IdCuota.Value,

                                montoRecibido:
                                    solicitud.Monto,

                                idUsuario:
                                    idUsuario.Value,

                                idFormaPago:
                                    formaPago.IdFormaPago,

                                observacion:
                                    observacion
                            );
                }

                // =================================================
                // ABONO EXTRAORDINARIO
                // =================================================
                else if (
                    solicitud.TipoOperacion ==
                    "ABONO_EXTRAORDINARIO")
                {
                    var observacion =
                        $"Abono aprobado desde solicitud #{solicitud.IdSolicitud}. " +
                        $"Banco: {banco}. " +
                        $"Boleta: {solicitud.NumeroBoleta}.";

                    resultado =
                        await _pagoService
                            .RegistrarAbonoExtraordinarioAsync(
                                idCredito:
                                    solicitud.IdCredito,

                                montoAbono:
                                    solicitud.Monto,

                                idUsuario:
                                    idUsuario.Value,

                                idFormaPago:
                                    formaPago.IdFormaPago,

                                observacion:
                                    observacion
                            );
                }

                // =================================================
                // TIPO INVÁLIDO
                // =================================================
                else
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] =
                        "El tipo de operación de la solicitud no es válido.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id }
                    );
                }

                // =================================================
                // ERROR DEL SERVICIO DE PAGOS
                // =================================================
                if (!resultado.Exito)
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] =
                        resultado.Mensaje;

                    return RedirectToAction(
                        nameof(Details),
                        new { id }
                    );
                }

                if (resultado.Pago == null)
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] =
                        "El pago fue procesado, pero no fue posible obtener el registro generado.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id }
                    );
                }

                // =================================================
                // VINCULAR SOLICITUD CON PAGO GENERADO
                // =================================================
                solicitud.IdPago =
                    resultado.Pago.IdPago;

                solicitud.Estado =
                    "APROBADA";

                solicitud.IdUsuarioRevision =
                    idUsuario.Value;

                solicitud.FechaRevision =
                    DateTime.Now;

                solicitud.MotivoRechazo =
                    null;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Exito"] =
                    $"La solicitud fue aprobada y el pago fue aplicado correctamente. " +
                    $"Recibo: {resultado.Pago.NumeroRecibo}.";

                return RedirectToAction(
                    nameof(Details),
                    new { id }
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                Console.WriteLine(
                    ex.ToString()
                );

                TempData["Error"] =
                    $"No fue posible aprobar la solicitud: {ex.Message}";

                return RedirectToAction(
                    nameof(Details),
                    new { id }
                );
            }
        }

        // =====================================================
        // RECHAZAR SOLICITUD
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermisoAuthorize("SOLICITUDES_RECHAZAR")]
        public async Task<IActionResult> Rechazar(
            ulong id,
            string motivoRechazo)
        {
            var idUsuario =
                ObtenerIdUsuarioActual();

            if (!idUsuario.HasValue)
            {
                return Unauthorized();
            }

            var solicitud =
                await _context.SolicitudesPago
                    .FirstOrDefaultAsync(s =>
                        s.IdSolicitud == id
                    );

            if (solicitud == null)
            {
                return NotFound();
            }

            // =================================================
            // SOLO SOLICITUDES PENDIENTES
            // =================================================
            if (
                solicitud.Estado !=
                "PENDIENTE")
            {
                TempData["Error"] =
                    "Esta solicitud ya fue revisada.";

                return RedirectToAction(
                    nameof(Details),
                    new { id }
                );
            }

            // =================================================
            // MOTIVO OBLIGATORIO
            // =================================================
            if (
                string.IsNullOrWhiteSpace(
                    motivoRechazo
                ))
            {
                TempData["Error"] =
                    "Debe indicar el motivo del rechazo.";

                return RedirectToAction(
                    nameof(Details),
                    new { id }
                );
            }

            // =================================================
            // ACTUALIZAR SOLICITUD
            // =================================================
            solicitud.Estado =
                "RECHAZADA";

            solicitud.IdUsuarioRevision =
                idUsuario.Value;

            solicitud.FechaRevision =
                DateTime.Now;

            solicitud.MotivoRechazo =
                motivoRechazo.Trim();

            solicitud.IdPago =
                null;

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "La solicitud fue rechazada correctamente.";

            return RedirectToAction(
                nameof(Details),
                new { id }
            );
        }
    }
}