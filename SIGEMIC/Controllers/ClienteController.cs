using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Models;
using System.Security.Claims;

namespace SIGEMIC.Controllers
{
    [Authorize(Roles = "CLIENTE")]
    public class ClienteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;


        public ClienteController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }


        // =====================================================
        // OBTENER CLIENTE ACTUAL
        // =====================================================

        private async Task<Cliente?> ObtenerClienteActualAsync()
        {
            var idUsuarioTexto =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );


            if (!uint.TryParse(
                idUsuarioTexto,
                out uint idUsuario))
            {
                return null;
            }


            return await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.IdUsuario == idUsuario
                );
        }


        // =====================================================
        // INICIO DEL CLIENTE
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cliente =
                await ObtenerClienteActualAsync();


            if (cliente == null)
            {
                return NotFound();
            }


            var credito =
                await _context.Creditos
                    .Include(c => c.Cuotas)
                    .Where(c =>
                        c.IdCliente ==
                            cliente.IdCliente &&
                        (
                            c.Estado == "ACTIVO" ||
                            c.Estado == "ATRASADO"
                        )
                    )
                    .OrderByDescending(c =>
                        c.FechaOtorgamiento
                    )
                    .FirstOrDefaultAsync();


            // =================================================
            // CLIENTE SIN CRÉDITO ACTIVO
            // =================================================

            if (credito == null)
            {
                ViewBag.TieneCredito = false;

                ViewBag.CodigoCredito = "-";

                ViewBag.MontoOriginal =
                    0m;

                ViewBag.SaldoCapital =
                    0m;

                ViewBag.CuotaOriginal =
                    0m;

                ViewBag.EstadoCredito =
                    "-";

                ViewBag.CuotasPendientes =
                    0;

                ViewBag.TotalPagado =
                    0m;


                return View();
            }


            // =================================================
            // TOTAL PAGADO
            // =================================================

            var totalPagado =
                await _context.Pagos
                    .Where(p =>
                        p.IdCredito ==
                            credito.IdCredito
                    )
                    .Select(p =>
                        (decimal?)p.MontoTotal
                    )
                    .SumAsync()
                ?? 0m;


            // =================================================
            // CUOTAS PENDIENTES
            // =================================================

            var cuotasPendientes =
                credito.Cuotas.Count(c =>
                    c.Estado == "PENDIENTE" ||
                    c.Estado == "PARCIAL" ||
                    c.Estado == "ATRASADA"
                );


            ViewBag.TieneCredito =
                true;

            ViewBag.CodigoCredito =
                credito.CodigoCredito;

            ViewBag.MontoOriginal =
                credito.MontoOriginal;

            ViewBag.SaldoCapital =
                credito.SaldoCapital;

            ViewBag.CuotaOriginal =
                credito.CuotaOriginal;

            ViewBag.EstadoCredito =
                credito.Estado;

            ViewBag.CuotasPendientes =
                cuotasPendientes;

            ViewBag.TotalPagado =
                totalPagado;


            return View();
        }


        // =====================================================
        // MI CRÉDITO
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Credito()
        {
            var cliente =
                await ObtenerClienteActualAsync();


            if (cliente == null)
            {
                return NotFound();
            }


            var credito =
                await _context.Creditos
                    .Include(c =>
                        c.PlanCredito
                    )
                    .Include(c =>
                        c.Cuotas
                    )
                    .Where(c =>
                        c.IdCliente ==
                            cliente.IdCliente &&
                        (
                            c.Estado == "ACTIVO" ||
                            c.Estado == "ATRASADO"
                        )
                    )
                    .OrderByDescending(c =>
                        c.FechaOtorgamiento
                    )
                    .FirstOrDefaultAsync();


            if (credito == null)
            {
                TempData["Info"] =
                    "Actualmente no tienes un crédito activo.";


                return RedirectToAction(
                    nameof(Index)
                );
            }


            return View(credito);
        }


        // =====================================================
        // MIS CUOTAS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Cuotas()
        {
            var cliente =
                await ObtenerClienteActualAsync();


            if (cliente == null)
            {
                return NotFound();
            }


            var credito =
                await _context.Creditos
                    .Include(c => c.Cuotas)
                    .Where(c =>
                        c.IdCliente ==
                            cliente.IdCliente &&
                        (
                            c.Estado == "ACTIVO" ||
                            c.Estado == "ATRASADO"
                        )
                    )
                    .OrderByDescending(c =>
                        c.FechaOtorgamiento
                    )
                    .FirstOrDefaultAsync();


            if (credito == null)
            {
                TempData["Info"] =
                    "Actualmente no tienes un crédito activo.";


                return RedirectToAction(
                    nameof(Index)
                );
            }


            // =================================================
            // DATOS DEL CRÉDITO PARA LA VISTA
            // =================================================

            ViewBag.CodigoCredito =
                credito.CodigoCredito;

            ViewBag.MontoOriginal =
                credito.MontoOriginal;

            ViewBag.SaldoCapital =
                credito.SaldoCapital;

            ViewBag.CuotaOriginal =
                credito.CuotaOriginal;

            ViewBag.EstadoCredito =
                credito.Estado;


            // =================================================
            // LA VISTA ESPERA IEnumerable<Cuota>
            // =================================================

            var cuotas =
                credito.Cuotas
                    .OrderBy(c =>
                        c.NumeroCuota
                    )
                    .ToList();


            return View(cuotas);
        }


        // =====================================================
        // ESTADO DE CUENTA / MIS PAGOS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> EstadoCuenta()
        {
            var cliente =
                await ObtenerClienteActualAsync();


            if (cliente == null)
            {
                return NotFound();
            }


            // =================================================
            // SE PERMITE VER EL ÚLTIMO CRÉDITO AUNQUE
            // YA ESTÉ CANCELADO
            // =================================================

            var credito =
                await _context.Creditos
                    .Include(c => c.Cuotas)
                    .Where(c =>
                        c.IdCliente ==
                            cliente.IdCliente
                    )
                    .OrderByDescending(c =>
                        c.FechaOtorgamiento
                    )
                    .FirstOrDefaultAsync();


            if (credito == null)
            {
                TempData["Info"] =
                    "No tienes créditos registrados.";


                return RedirectToAction(
                    nameof(Index)
                );
            }


            var pagos =
                await _context.Pagos
                    .Include(p =>
                        p.Cuota
                    )
                    .Include(p =>
                        p.FormaPago
                    )
                    .Where(p =>
                        p.IdCredito ==
                            credito.IdCredito
                    )
                    .OrderByDescending(p =>
                        p.FechaPago
                    )
                    .ThenByDescending(p =>
                        p.IdPago
                    )
                    .Select(p =>
                        new PagoHistorialViewModel
                        {
                            IdPago =
                                p.IdPago,

                            NumeroRecibo =
                                p.NumeroRecibo,

                            FechaPago =
                                p.FechaPago,

                            TipoOperacion =
                                p.TipoOperacion,

                            NumeroCuota =
                                p.Cuota != null
                                    ? p.Cuota.NumeroCuota
                                    : null,

                            FormaPago =
                                p.FormaPago.Nombre,

                            MontoTotal =
                                p.MontoTotal,

                            CapitalAplicado =
                                p.CapitalAplicado,

                            InteresAplicado =
                                p.InteresAplicado,

                            MoraAplicada =
                                p.MoraAplicada,

                            Observacion =
                                p.Observacion
                        }
                    )
                    .ToListAsync();


            var totalPagado =
                pagos.Sum(p =>
                    p.MontoTotal
                );


            var model =
                new EstadoCuentaViewModel
                {
                    CodigoCredito =
                        credito.CodigoCredito,

                    MontoOriginal =
                        credito.MontoOriginal,

                    SaldoCapital =
                        credito.SaldoCapital,

                    CuotaOriginal =
                        credito.CuotaOriginal,

                    EstadoCredito =
                        credito.Estado,

                    TotalPagado =
                        totalPagado,

                    Pagos =
                        pagos
                };


            return View(model);
        }


        // =====================================================
        // SOLICITAR PAGO - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> SolicitarPago()
        {
            var cliente =
                await ObtenerClienteActualAsync();


            if (cliente == null)
            {
                return NotFound();
            }


            var model =
                new SolicitarPagoViewModel
                {
                    TipoOperacion =
                        "PAGO_CUOTA"
                };


            await CargarDatosSolicitudPago(
                model,
                cliente.IdCliente
            );


            return View(model);
        }


        // =====================================================
        // SOLICITAR PAGO - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SolicitarPago(
            SolicitarPagoViewModel model)
        {
            var cliente =
                await ObtenerClienteActualAsync();


            if (cliente == null)
            {
                return NotFound();
            }


            // =================================================
            // VALIDAR TIPO DE OPERACIÓN
            // =================================================

            if (
                model.TipoOperacion !=
                    "PAGO_CUOTA" &&
                model.TipoOperacion !=
                    "ABONO_EXTRAORDINARIO"
            )
            {
                ModelState.AddModelError(
                    nameof(model.TipoOperacion),
                    "El tipo de operación seleccionado no es válido."
                );
            }


            // =================================================
            // VALIDAR CRÉDITO
            // =================================================

            var credito =
                await _context.Creditos
                    .FirstOrDefaultAsync(c =>
                        c.IdCredito ==
                            model.IdCredito &&
                        c.IdCliente ==
                            cliente.IdCliente &&
                        (
                            c.Estado == "ACTIVO" ||
                            c.Estado == "ATRASADO"
                        )
                    );


            if (credito == null)
            {
                ModelState.AddModelError(
                    nameof(model.IdCredito),
                    "El crédito seleccionado no es válido."
                );
            }


            // =================================================
            // VALIDAR CUOTA
            // =================================================

            Cuota? cuota =
                null;


            if (
                model.TipoOperacion ==
                "PAGO_CUOTA"
            )
            {
                if (!model.IdCuota.HasValue)
                {
                    ModelState.AddModelError(
                        nameof(model.IdCuota),
                        "Seleccione la cuota que desea pagar."
                    );
                }
                else
                {
                    cuota =
                        await _context.Cuotas
                            .FirstOrDefaultAsync(c =>
                                c.IdCuota ==
                                    model.IdCuota.Value &&
                                c.IdCredito ==
                                    model.IdCredito &&
                                (
                                    c.Estado == "PENDIENTE" ||
                                    c.Estado == "PARCIAL" ||
                                    c.Estado == "ATRASADA"
                                )
                            );


                    if (cuota == null)
                    {
                        ModelState.AddModelError(
                            nameof(model.IdCuota),
                            "La cuota seleccionada no es válida."
                        );
                    }
                    else
                    {
                        // =========================================
                        // EVITAR DOS SOLICITUDES PENDIENTES
                        // PARA LA MISMA CUOTA
                        // =========================================

                        var solicitudPendiente =
                            await _context.SolicitudesPago
                                .AnyAsync(s =>
                                    s.IdCuota ==
                                        cuota.IdCuota &&
                                    s.Estado ==
                                        "PENDIENTE"
                                );


                        if (solicitudPendiente)
                        {
                            ModelState.AddModelError(
                                nameof(model.IdCuota),
                                "Esta cuota ya tiene una solicitud de pago pendiente de revisión."
                            );
                        }


                        // =========================================
                        // NO SUPERAR SALDO DE CUOTA
                        // =========================================

                        if (
                            model.Monto >
                            cuota.SaldoCuota
                        )
                        {
                            ModelState.AddModelError(
                                nameof(model.Monto),
                                $"El monto no puede superar el saldo de la cuota: Q{cuota.SaldoCuota:N2}."
                            );
                        }
                    }
                }
            }
            else
            {
                // Los abonos extraordinarios
                // no pertenecen a una cuota.

                model.IdCuota =
                    null;
            }


            // =================================================
            // VALIDAR ABONO EXTRAORDINARIO
            // =================================================

            if (
                model.TipoOperacion ==
                    "ABONO_EXTRAORDINARIO" &&
                credito != null
            )
            {
                var abonosPendientes =
                    await _context.SolicitudesPago
                        .Where(s =>
                            s.IdCredito ==
                                credito.IdCredito &&
                            s.TipoOperacion ==
                                "ABONO_EXTRAORDINARIO" &&
                            s.Estado ==
                                "PENDIENTE"
                        )
                        .Select(s =>
                            (decimal?)s.Monto
                        )
                        .SumAsync()
                    ?? 0m;


                if (
                    model.Monto +
                    abonosPendientes >
                    credito.SaldoCapital
                )
                {
                    ModelState.AddModelError(
                        nameof(model.Monto),
                        "El monto solicitado, sumado a otros abonos pendientes, supera el saldo del crédito."
                    );
                }
            }


            // =================================================
            // VALIDAR BANCO
            // =================================================

            var banco =
                await _context.Bancos
                    .FirstOrDefaultAsync(b =>
                        b.IdBanco ==
                            model.IdBanco &&
                        b.Estado ==
                            "ACTIVO"
                    );


            if (banco == null)
            {
                ModelState.AddModelError(
                    nameof(model.IdBanco),
                    "Seleccione un banco válido."
                );
            }


            // =================================================
            // VALIDAR NÚMERO DE BOLETA
            // =================================================

            var numeroBoleta =
                model.NumeroBoleta?
                    .Trim();


            if (
                string.IsNullOrWhiteSpace(
                    numeroBoleta
                )
            )
            {
                ModelState.AddModelError(
                    nameof(model.NumeroBoleta),
                    "Ingrese el número de boleta."
                );
            }
            else
            {
                var boletaExiste =
                    await _context.SolicitudesPago
                        .AnyAsync(s =>
                            s.IdBanco ==
                                model.IdBanco &&
                            s.NumeroBoleta ==
                                numeroBoleta
                        );


                if (boletaExiste)
                {
                    ModelState.AddModelError(
                        nameof(model.NumeroBoleta),
                        "Este número de boleta ya fue registrado para el banco seleccionado."
                    );
                }
            }


            // =================================================
            // VALIDAR MONTO
            // =================================================

            if (model.Monto <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.Monto),
                    "El monto debe ser mayor que cero."
                );
            }


            // =================================================
            // VALIDAR COMPROBANTE
            // =================================================

            string? extension =
                null;


            if (
                model.Comprobante == null ||
                model.Comprobante.Length == 0
            )
            {
                ModelState.AddModelError(
                    nameof(model.Comprobante),
                    "Debe adjuntar el comprobante."
                );
            }
            else
            {
                extension =
                    Path.GetExtension(
                        model.Comprobante.FileName
                    )
                    .ToLowerInvariant();


                var extensionesPermitidas =
                    new[]
                    {
                        ".jpg",
                        ".jpeg",
                        ".png",
                        ".pdf"
                    };


                if (
                    !extensionesPermitidas
                        .Contains(extension)
                )
                {
                    ModelState.AddModelError(
                        nameof(model.Comprobante),
                        "Solo se permiten archivos JPG, JPEG, PNG o PDF."
                    );
                }


                const long tamanoMaximo =
                    5 * 1024 * 1024;


                if (
                    model.Comprobante.Length >
                    tamanoMaximo
                )
                {
                    ModelState.AddModelError(
                        nameof(model.Comprobante),
                        "El comprobante no puede superar los 5 MB."
                    );
                }


                var tiposPermitidos =
                    new[]
                    {
                        "image/jpeg",
                        "image/png",
                        "application/pdf"
                    };


                if (
                    !tiposPermitidos.Contains(
                        model.Comprobante.ContentType
                    )
                )
                {
                    ModelState.AddModelError(
                        nameof(model.Comprobante),
                        "El tipo de archivo seleccionado no es válido."
                    );
                }
            }


            // =================================================
            // SI HAY ERRORES
            // =================================================

            if (!ModelState.IsValid)
            {
                await CargarDatosSolicitudPago(
                    model,
                    cliente.IdCliente
                );


                return View(model);
            }


            // =================================================
            // CARPETA PRIVADA DE COMPROBANTES
            // =================================================

            var carpeta =
                Path.Combine(
                    _environment.ContentRootPath,
                    "Storage",
                    "ComprobantesPago"
                );


            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(
                    carpeta
                );
            }


            // =================================================
            // GENERAR NOMBRE SEGURO
            // =================================================

            var nombreArchivo =
                $"{Guid.NewGuid():N}{extension}";


            var rutaCompleta =
                Path.Combine(
                    carpeta,
                    nombreArchivo
                );


            // =================================================
            // GUARDAR COMPROBANTE
            // =================================================

            await using (
                var stream =
                    new FileStream(
                        rutaCompleta,
                        FileMode.Create
                    )
            )
            {
                await model.Comprobante!
                    .CopyToAsync(stream);
            }


            // =================================================
            // CREAR SOLICITUD
            // =================================================

            try
            {
                var solicitud =
                    new SolicitudPago
                    {
                        IdCliente =
                            cliente.IdCliente,

                        IdCredito =
                            model.IdCredito,

                        IdCuota =
                            model.TipoOperacion ==
                                "PAGO_CUOTA"
                                ? model.IdCuota
                                : null,

                        IdBanco =
                            model.IdBanco,

                        IdUsuarioRevision =
                            null,

                        // Al estar pendiente todavía
                        // no existe un pago real.
                        IdPago =
                            null,

                        TipoOperacion =
                            model.TipoOperacion,

                        Monto =
                            Math.Round(
                                model.Monto,
                                2
                            ),

                        NumeroBoleta =
                            numeroBoleta!,

                        ArchivoComprobante =
                            nombreArchivo,

                        Estado =
                            "PENDIENTE",

                        FechaSolicitud =
                            DateTime.Now,

                        FechaRevision =
                            null,

                        MotivoRechazo =
                            null
                    };


                _context.SolicitudesPago.Add(
                    solicitud
                );


                await _context.SaveChangesAsync();
            }
            catch
            {
                // =============================================
                // SI MYSQL FALLA ELIMINAMOS EL ARCHIVO
                // =============================================

                if (
                    System.IO.File.Exists(
                        rutaCompleta
                    )
                )
                {
                    System.IO.File.Delete(
                        rutaCompleta
                    );
                }


                throw;
            }


            TempData["Exito"] =
                "La solicitud fue enviada correctamente. " +
                "El pago está pendiente de revisión y todavía no ha sido aplicado al crédito.";


            return RedirectToAction(
                nameof(SolicitarPago)
            );
        }


        // =====================================================
        // MIS SOLICITUDES
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> MisSolicitudes()
        {
            var cliente =
                await ObtenerClienteActualAsync();


            if (cliente == null)
            {
                return NotFound();
            }


            var solicitudes =
                await _context.SolicitudesPago
                    .AsNoTracking()
                    .Include(s =>
                        s.Credito
                    )
                    .Include(s =>
                        s.Cuota
                    )
                    .Include(s =>
                        s.Banco
                    )
                    .Include(s =>
                        s.Pago
                    )
                    .Where(s =>
                        s.IdCliente ==
                            cliente.IdCliente
                    )
                    .OrderByDescending(s =>
                        s.FechaSolicitud
                    )
                    .ThenByDescending(s =>
                        s.IdSolicitud
                    )
                    .ToListAsync();


            return View(solicitudes);
        }


        // =====================================================
        // DETALLE DE MI SOLICITUD
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> DetalleSolicitud(
            ulong id)
        {
            var cliente =
                await ObtenerClienteActualAsync();


            if (cliente == null)
            {
                return NotFound();
            }


            var solicitud =
                await _context.SolicitudesPago
                    .AsNoTracking()
                    .Include(s =>
                        s.Credito
                    )
                    .Include(s =>
                        s.Cuota
                    )
                    .Include(s =>
                        s.Banco
                    )
                    .Include(s =>
                        s.Pago
                    )
                    .Include(s =>
                        s.UsuarioRevision
                    )
                    .FirstOrDefaultAsync(s =>
                        s.IdSolicitud ==
                            id &&
                        s.IdCliente ==
                            cliente.IdCliente
                    );


            if (solicitud == null)
            {
                return NotFound();
            }


            return View(solicitud);
        }


        // =====================================================
        // VER MI COMPROBANTE
        // =====================================================

        [HttpGet]
        public async Task<IActionResult>
            VerComprobanteSolicitud(
                ulong id)
        {
            var cliente =
                await ObtenerClienteActualAsync();


            if (cliente == null)
            {
                return NotFound();
            }


            // =================================================
            // IMPORTANTE:
            // Solo puede consultar comprobantes propios.
            // =================================================

            var solicitud =
                await _context.SolicitudesPago
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s =>
                        s.IdSolicitud ==
                            id &&
                        s.IdCliente ==
                            cliente.IdCliente
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
                    nameof(DetalleSolicitud),
                    new
                    {
                        id =
                            solicitud.IdSolicitud
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
        // AJAX
        // OBTENER CUOTAS PENDIENTES DEL CRÉDITO
        // =====================================================

        [HttpGet]
        public async Task<IActionResult>
            CuotasPendientesPorCredito(
                ulong idCredito)
        {
            var cliente =
                await ObtenerClienteActualAsync();


            if (cliente == null)
            {
                return Unauthorized();
            }


            // =================================================
            // VALIDAR QUE EL CRÉDITO SEA DEL CLIENTE
            // =================================================

            var creditoValido =
                await _context.Creditos
                    .AnyAsync(c =>
                        c.IdCredito ==
                            idCredito &&
                        c.IdCliente ==
                            cliente.IdCliente &&
                        (
                            c.Estado == "ACTIVO" ||
                            c.Estado == "ATRASADO"
                        )
                    );


            if (!creditoValido)
            {
                return Forbid();
            }


            var cuotas =
                await _context.Cuotas
                    .Where(c =>
                        c.IdCredito ==
                            idCredito &&
                        (
                            c.Estado == "PENDIENTE" ||
                            c.Estado == "PARCIAL" ||
                            c.Estado == "ATRASADA"
                        )
                    )
                    .OrderBy(c =>
                        c.NumeroCuota
                    )
                    .Select(c =>
                        new
                        {
                            idCuota =
                                c.IdCuota,

                            numeroCuota =
                                c.NumeroCuota,

                            saldoCuota =
                                c.SaldoCuota,

                            fechaVencimiento =
                                c.FechaVencimiento
                                    .ToString(
                                        "dd/MM/yyyy"
                                    )
                        }
                    )
                    .ToListAsync();


            return Json(cuotas);
        }


        // =====================================================
        // CARGAR DATOS PARA SOLICITAR PAGO
        // =====================================================

        private async Task CargarDatosSolicitudPago(
            SolicitarPagoViewModel model,
            uint idCliente)
        {
            // =================================================
            // CRÉDITOS ACTIVOS
            // =================================================

            var creditos =
                await _context.Creditos
                    .Where(c =>
                        c.IdCliente ==
                            idCliente &&
                        (
                            c.Estado == "ACTIVO" ||
                            c.Estado == "ATRASADO"
                        )
                    )
                    .OrderByDescending(c =>
                        c.FechaOtorgamiento
                    )
                    .ToListAsync();


            if (
                model.IdCredito == 0 &&
                creditos.Any()
            )
            {
                model.IdCredito =
                    creditos.First()
                        .IdCredito;
            }


            model.Creditos =
                creditos
                    .Select(c =>
                        new SelectListItem
                        {
                            Value =
                                c.IdCredito
                                    .ToString(),

                            Text =
                                $"{c.CodigoCredito} - Saldo Q{c.SaldoCapital:N2}",

                            Selected =
                                c.IdCredito ==
                                model.IdCredito
                        }
                    )
                    .ToList();


            // =================================================
            // BANCOS ACTIVOS
            // =================================================

            model.Bancos =
                await _context.Bancos
                    .Where(b =>
                        b.Estado ==
                            "ACTIVO"
                    )
                    .OrderBy(b =>
                        b.Nombre
                    )
                    .ToListAsync();


            // =================================================
            // CUOTAS
            // =================================================

            model.Cuotas =
                new List<SelectListItem>();


            if (
                model.IdCredito > 0 &&
                model.TipoOperacion ==
                    "PAGO_CUOTA"
            )
            {
                var cuotas =
                    await _context.Cuotas
                        .Where(c =>
                            c.IdCredito ==
                                model.IdCredito &&
                            (
                                c.Estado == "PENDIENTE" ||
                                c.Estado == "PARCIAL" ||
                                c.Estado == "ATRASADA"
                            )
                        )
                        .OrderBy(c =>
                            c.NumeroCuota
                        )
                        .ToListAsync();


                model.Cuotas =
                    cuotas
                        .Select(c =>
                            new SelectListItem
                            {
                                Value =
                                    c.IdCuota
                                        .ToString(),

                                Text =
                                    $"Cuota #{c.NumeroCuota} - Q{c.SaldoCuota:N2} - {c.FechaVencimiento:dd/MM/yyyy}",

                                Selected =
                                    model.IdCuota.HasValue &&
                                    c.IdCuota ==
                                        model.IdCuota.Value
                            }
                        )
                        .ToList();
            }
        }
    }
}