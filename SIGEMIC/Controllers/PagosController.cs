using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Models;
using SIGEMIC.Security;
using SIGEMIC.Services;

namespace SIGEMIC.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR,CAJERO")]
    public class PagosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PagoService _pagoService;
        private readonly AuditoriaService _auditoriaService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PagosController> _logger;

        public PagosController(
            ApplicationDbContext context,
            PagoService pagoService,
            AuditoriaService auditoriaService,
            IWebHostEnvironment environment,
            ILogger<PagosController> logger)
        {
            _context = context;
            _pagoService = pagoService;
            _auditoriaService = auditoriaService;
            _environment = environment;
            _logger = logger;
        }

        // =====================================================
        // CLIENTES CON CRÉDITOS ACTIVOS
        // =====================================================
        [HttpGet]
        [PermisoAuthorize("PAGOS_REGISTRAR")]
        public async Task<IActionResult> Index()
        {
            var clientes = await _context.Clientes
                .Include(c => c.Usuario)
                .Where(c =>
                    c.Estado == "ACTIVO" &&
                    c.Creditos.Any(cr =>
                        cr.Estado == "ACTIVO" ||
                        cr.Estado == "ATRASADO"))
                .OrderBy(c => c.Usuario.Nombres)
                .ThenBy(c => c.Usuario.Apellidos)
                .ToListAsync();

            return View(clientes);
        }

        // =====================================================
        // CUOTAS PENDIENTES DE UN CLIENTE
        // =====================================================
        [HttpGet]
        [PermisoAuthorize("PAGOS_REGISTRAR")]
        public async Task<IActionResult> CuotasCliente(uint id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.IdCliente == id);

            if (cliente == null)
                return NotFound();

            var cuotas = await _context.Cuotas
                .Include(c => c.Credito)
                .Where(c =>
                    c.Credito.IdCliente == id &&
                    (
                        c.Credito.Estado == "ACTIVO" ||
                        c.Credito.Estado == "ATRASADO"
                    ) &&
                    (
                        c.Estado == "PENDIENTE" ||
                        c.Estado == "PARCIAL" ||
                        c.Estado == "ATRASADA"
                    ))
                .OrderBy(c => c.FechaVencimiento)
                .ToListAsync();

            ViewBag.IdCliente = cliente.IdCliente;
            ViewBag.CodigoCliente = cliente.CodigoCliente;
            ViewBag.NombreCliente =
                cliente.Usuario.Nombres + " " +
                cliente.Usuario.Apellidos;

            return View(cuotas);
        }

        // =====================================================
        // REGISTRAR PAGO - GET
        // =====================================================
        [HttpGet]
        [PermisoAuthorize("PAGOS_REGISTRAR")]
        public async Task<IActionResult> Registrar(ulong id)
        {
            var cuota = await _context.Cuotas
                .Include(c => c.Credito)
                    .ThenInclude(c => c.Cliente)
                        .ThenInclude(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.IdCuota == id);

            if (cuota == null)
                return NotFound();

            if (cuota.Estado == "PAGADA")
            {
                TempData["MensajeError"] =
                    "La cuota seleccionada ya está pagada.";

                return RedirectToAction(
                    nameof(CuotasCliente),
                    new { id = cuota.Credito.IdCliente }
                );
            }

            if (cuota.Credito.Estado == "CANCELADO")
            {
                TempData["MensajeError"] =
                    "No se pueden registrar pagos en un crédito cancelado.";

                return RedirectToAction(
                    nameof(CuotasCliente),
                    new { id = cuota.Credito.IdCliente }
                );
            }

            decimal capitalPendiente = Math.Round(
                cuota.CapitalProgramado -
                cuota.CapitalPagado,
                2
            );

            decimal interesPendiente = Math.Round(
                cuota.InteresProgramado -
                cuota.InteresPagado,
                2
            );

            decimal moraPendiente = 0m;

            decimal totalPendiente = Math.Round(
                capitalPendiente +
                interesPendiente +
                moraPendiente,
                2
            );

            var model = new RegistrarPagoViewModel
            {
                IdCuota = cuota.IdCuota,
                IdCredito = cuota.IdCredito,
                CodigoCredito = cuota.Credito.CodigoCredito,
                Cliente =
                    cuota.Credito.Cliente.Usuario.Nombres + " " +
                    cuota.Credito.Cliente.Usuario.Apellidos,
                NumeroCuota = cuota.NumeroCuota,
                FechaVencimiento = cuota.FechaVencimiento,
                CapitalPendiente = capitalPendiente,
                InteresPendiente = interesPendiente,
                MoraPendiente = moraPendiente,
                TotalPagar = totalPendiente,
                MontoPago = totalPendiente
            };

            await CargarFormasPago(model);

            return View(model);
        }

        // =====================================================
        // REGISTRAR PAGO - POST
        // PAGO COMPLETO O PARCIAL
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermisoAuthorize("PAGOS_REGISTRAR")]
        public async Task<IActionResult> Registrar(
            RegistrarPagoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarDatosCuota(model);
                await CargarFormasPago(model);
                return View(model);
            }

            var idUsuarioTexto =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!uint.TryParse(
                    idUsuarioTexto,
                    out uint idUsuario))
            {
                return Unauthorized();
            }

            string? archivoGuardado = null;

            var preparacion =
                await PrepararDatosBancariosAsync(
                    model.IdFormaPago,
                    model.IdBanco,
                    model.NumeroBoleta,
                    model.Comprobante
                );

            if (!preparacion.Exito)
            {
                ModelState.AddModelError(
                    string.Empty,
                    preparacion.MensajeError ??
                    "No fue posible validar los datos del pago."
                );

                await CargarDatosCuota(model);
                await CargarFormasPago(model);

                return View(model);
            }

            archivoGuardado =
                preparacion.ArchivoGuardado;

            try
            {
                var cuotaAntes = await _context.Cuotas
                    .AsNoTracking()
                    .Include(c => c.Credito)
                        .ThenInclude(c => c.Cliente)
                    .FirstOrDefaultAsync(c =>
                        c.IdCuota == model.IdCuota
                    );

                var formaPagoNombre = await _context.FormasPago
                    .AsNoTracking()
                    .Where(f =>
                        f.IdFormaPago == model.IdFormaPago
                    )
                    .Select(f => f.Nombre)
                    .FirstOrDefaultAsync();

                var resultado =
                    await _pagoService
                        .RegistrarPagoCuotaAsync(
                            idCuota: model.IdCuota,
                            montoRecibido: model.MontoPago,
                            idUsuario: idUsuario,
                            idFormaPago: model.IdFormaPago,
                            observacion: model.Observacion,
                            idBanco: model.IdBanco,
                            numeroBoleta: model.NumeroBoleta,
                            archivoComprobante: archivoGuardado
                        );

                if (!resultado.Exito)
                {
                    EliminarComprobanteSiExiste(
                        archivoGuardado
                    );

                    await RegistrarAuditoriaSeguroAsync(
                        "PAGO_RECHAZADO",
                        "cuotas",
                        model.IdCuota.ToString(),
                        $"Intento de pago rechazado. " +
                        $"Monto solicitado: Q{model.MontoPago:N2}. " +
                        $"Motivo: {resultado.Mensaje}",
                        "FALLIDO"
                    );

                    ModelState.AddModelError(
                        string.Empty,
                        resultado.Mensaje
                    );

                    await CargarDatosCuota(model);
                    await CargarFormasPago(model);

                    return View(model);
                }

                var pago = resultado.Pago;
                var cuota = resultado.Cuota;
                var credito = resultado.Credito;

                if (
                    pago == null ||
                    cuota == null ||
                    credito == null
                )
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "El pago fue procesado, pero no fue posible obtener la información resultante."
                    );

                    await CargarDatosCuota(model);
                    await CargarFormasPago(model);

                    return View(model);
                }

                string clienteCodigo =
                    cuotaAntes?.Credito.Cliente.CodigoCliente
                    ?? "No disponible";

                string estadoCuotaAnterior =
                    cuotaAntes?.Estado
                    ?? "No disponible";

                string estadoCreditoAnterior =
                    cuotaAntes?.Credito.Estado
                    ?? "No disponible";

                decimal saldoCuotaAnterior =
                    cuotaAntes?.SaldoCuota
                    ?? 0m;

                decimal saldoCapitalAnterior =
                    cuotaAntes?.Credito.SaldoCapital
                    ?? 0m;

                string descripcionAuditoria =
                    $"Pago {pago.NumeroRecibo} registrado. " +
                    $"Crédito: {credito.CodigoCredito}; " +
                    $"Cliente: {clienteCodigo}; " +
                    $"Cuota: #{cuota.NumeroCuota}; " +
                    $"Forma de pago: {formaPagoNombre ?? "No disponible"}; " +
                    $"Monto: Q{pago.MontoTotal:N2}; " +
                    $"Capital aplicado: Q{pago.CapitalAplicado:N2}; " +
                    $"Interés aplicado: Q{pago.InteresAplicado:N2}; " +
                    $"Mora aplicada: Q{pago.MoraAplicada:N2}; " +
                    CrearDetalleBancarioAuditoria(
                        formaPagoNombre,
                        pago.IdBanco,
                        pago.NumeroBoleta
                    ) +
                    $"Cambios: " +
                    $"Saldo de cuota: Q{saldoCuotaAnterior:N2} → Q{cuota.SaldoCuota:N2}; " +
                    $"Estado de cuota: {estadoCuotaAnterior} → {cuota.Estado}; " +
                    $"Saldo capital: Q{saldoCapitalAnterior:N2} → Q{credito.SaldoCapital:N2}; " +
                    $"Estado del crédito: {estadoCreditoAnterior} → {credito.Estado}.";

                await RegistrarAuditoriaSeguroAsync(
                    "PAGO_REGISTRADO",
                    "pagos",
                    pago.IdPago.ToString(),
                    descripcionAuditoria
                );

                if (cuota.Estado == "PAGADA")
                {
                    TempData["MensajeExito"] =
                        $"Pago registrado correctamente. " +
                        $"Recibo: {pago.NumeroRecibo} - " +
                        $"Cuota #{cuota.NumeroCuota} PAGADA - " +
                        $"Total: Q {pago.MontoTotal:N2}";
                }
                else
                {
                    TempData["MensajeExito"] =
                        $"Pago parcial registrado correctamente. " +
                        $"Recibo: {pago.NumeroRecibo} - " +
                        $"Pagado: Q {pago.MontoTotal:N2} - " +
                        $"Saldo de cuota: Q {cuota.SaldoCuota:N2}";
                }

                return RedirectToAction(
                    nameof(CuotasCliente),
                    new { id = credito.IdCliente }
                );
            }
            catch (Exception ex)
            {
                EliminarComprobanteSiExiste(
                    archivoGuardado
                );

                Console.WriteLine(ex.ToString());

                ModelState.AddModelError(
                    string.Empty,
                    $"No fue posible registrar el pago: {ex.Message}"
                );

                await CargarDatosCuota(model);
                await CargarFormasPago(model);

                return View(model);
            }
        }

        // =====================================================
        // ABONO EXTRAORDINARIO - GET
        // =====================================================
        [HttpGet]
        [PermisoAuthorize("PAGOS_ABONO_CAPITAL")]
        public async Task<IActionResult> AbonoExtraordinario(
            ulong id)
        {
            var credito = await _context.Creditos
                .Include(c => c.Cliente)
                    .ThenInclude(c => c.Usuario)
                .FirstOrDefaultAsync(c =>
                    c.IdCredito == id);

            if (credito == null)
                return NotFound();

            if (
                credito.Estado != "ACTIVO" &&
                credito.Estado != "ATRASADO"
            )
            {
                TempData["MensajeError"] =
                    "El crédito no se encuentra activo.";

                return RedirectToAction(nameof(Index));
            }

            bool cuotaIrregular =
                await _context.Cuotas
                    .AnyAsync(c =>
                        c.IdCredito == credito.IdCredito &&
                        (
                            c.Estado == "PARCIAL" ||
                            c.Estado == "ATRASADA"
                        ));

            if (cuotaIrregular)
            {
                TempData["MensajeError"] =
                    "Primero debe completar las cuotas parciales o atrasadas.";

                return RedirectToAction(
                    nameof(CuotasCliente),
                    new { id = credito.IdCliente }
                );
            }

            var model =
                new AbonoExtraordinarioViewModel
                {
                    IdCredito = credito.IdCredito,
                    CodigoCredito = credito.CodigoCredito,
                    Cliente =
                        credito.Cliente.Usuario.Nombres + " " +
                        credito.Cliente.Usuario.Apellidos,
                    SaldoCapital = credito.SaldoCapital,
                    CuotaActual = credito.CuotaOriginal,
                    PlazoOriginal = credito.PlazoMeses
                };

            await CargarFormasPagoAbono(model);

            return View(model);
        }

        // =====================================================
        // ABONO EXTRAORDINARIO - POST
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermisoAuthorize("PAGOS_ABONO_CAPITAL")]
        public async Task<IActionResult> AbonoExtraordinario(
            AbonoExtraordinarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarDatosAbono(model);
                await CargarFormasPagoAbono(model);

                return View(model);
            }

            var idUsuarioTexto =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (!uint.TryParse(
                    idUsuarioTexto,
                    out uint idUsuario))
            {
                return Unauthorized();
            }

            string? archivoGuardado = null;

            var preparacion =
                await PrepararDatosBancariosAsync(
                    model.IdFormaPago,
                    model.IdBanco,
                    model.NumeroBoleta,
                    model.Comprobante
                );

            if (!preparacion.Exito)
            {
                ModelState.AddModelError(
                    string.Empty,
                    preparacion.MensajeError ??
                    "No fue posible validar los datos del abono."
                );

                await CargarDatosAbono(model);
                await CargarFormasPagoAbono(model);

                return View(model);
            }

            archivoGuardado =
                preparacion.ArchivoGuardado;

            try
            {
                var creditoAntes = await _context.Creditos
                    .AsNoTracking()
                    .Include(c => c.Cliente)
                    .FirstOrDefaultAsync(c =>
                        c.IdCredito == model.IdCredito
                    );

                int cuotasPendientesAntes =
                    await _context.Cuotas
                        .AsNoTracking()
                        .CountAsync(c =>
                            c.IdCredito == model.IdCredito &&
                            c.Estado != "PAGADA"
                        );

                var formaPagoAbonoNombre = await _context.FormasPago
                    .AsNoTracking()
                    .Where(f =>
                        f.IdFormaPago == model.IdFormaPago
                    )
                    .Select(f => f.Nombre)
                    .FirstOrDefaultAsync();

                var resultado =
                    await _pagoService
                        .RegistrarAbonoExtraordinarioAsync(
                            idCredito: model.IdCredito,
                            montoAbono: model.MontoAbono,
                            idUsuario: idUsuario,
                            idFormaPago: model.IdFormaPago,
                            observacion: model.Observacion,
                            idBanco: model.IdBanco,
                            numeroBoleta: model.NumeroBoleta,
                            archivoComprobante: archivoGuardado
                        );

                if (!resultado.Exito)
                {
                    EliminarComprobanteSiExiste(
                        archivoGuardado
                    );

                    await RegistrarAuditoriaSeguroAsync(
                        "ABONO_CAPITAL_RECHAZADO",
                        "creditos",
                        model.IdCredito.ToString(),
                        $"Intento de abono extraordinario rechazado. " +
                        $"Monto solicitado: Q{model.MontoAbono:N2}. " +
                        $"Motivo: {resultado.Mensaje}",
                        "FALLIDO"
                    );

                    ModelState.AddModelError(
                        string.Empty,
                        resultado.Mensaje
                    );

                    await CargarDatosAbono(model);
                    await CargarFormasPagoAbono(model);

                    return View(model);
                }

                var pago = resultado.Pago;
                var credito = resultado.Credito;

                if (
                    pago == null ||
                    credito == null
                )
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "El abono fue procesado, pero no fue posible obtener la información resultante."
                    );

                    await CargarDatosAbono(model);
                    await CargarFormasPagoAbono(model);

                    return View(model);
                }

                string clienteCodigoAbono =
                    creditoAntes?.Cliente.CodigoCliente
                    ?? "No disponible";

                string estadoCreditoAnteriorAbono =
                    creditoAntes?.Estado
                    ?? "No disponible";

                decimal saldoCapitalAnteriorAbono =
                    creditoAntes?.SaldoCapital
                    ?? 0m;

                string fechaFinalAnterior =
                    creditoAntes == null
                        ? "No disponible"
                        : creditoAntes
                            .FechaFinalizacionEstimada
                            .ToString("dd/MM/yyyy");

                string fechaFinalNueva =
                    credito.FechaFinalizacionEstimada
                        .ToString("dd/MM/yyyy");

                string descripcionAbono =
                    $"Abono extraordinario {pago.NumeroRecibo} registrado. " +
                    $"Crédito: {credito.CodigoCredito}; " +
                    $"Cliente: {clienteCodigoAbono}; " +
                    $"Forma de pago: {formaPagoAbonoNombre ?? "No disponible"}; " +
                    $"Monto: Q{pago.MontoTotal:N2}; " +
                    $"Capital aplicado: Q{pago.CapitalAplicado:N2}; " +
                    CrearDetalleBancarioAuditoria(
                        formaPagoAbonoNombre,
                        pago.IdBanco,
                        pago.NumeroBoleta
                    ) +
                    $"Cambios: " +
                    $"Saldo capital: Q{saldoCapitalAnteriorAbono:N2} → Q{credito.SaldoCapital:N2}; " +
                    $"Estado del crédito: {estadoCreditoAnteriorAbono} → {credito.Estado}; " +
                    $"Cuotas pendientes: {cuotasPendientesAntes} → {resultado.CuotasRestantes}; " +
                    $"Fecha final estimada: {fechaFinalAnterior} → {fechaFinalNueva}.";

                await RegistrarAuditoriaSeguroAsync(
                    "ABONO_CAPITAL_REGISTRADO",
                    "pagos",
                    pago.IdPago.ToString(),
                    descripcionAbono
                );

                if (resultado.CreditoCancelado)
                {
                    TempData["MensajeExito"] =
                        $"Abono registrado correctamente. " +
                        $"Recibo: {pago.NumeroRecibo}. " +
                        $"El crédito {credito.CodigoCredito} fue cancelado.";

                    return RedirectToAction(
                        nameof(Index)
                    );
                }

                TempData["MensajeExito"] =
                    $"Abono extraordinario registrado. " +
                    $"Recibo: {pago.NumeroRecibo} - " +
                    $"Abono: Q {pago.MontoTotal:N2} - " +
                    $"Nuevo saldo: Q {credito.SaldoCapital:N2} - " +
                    $"Cuotas restantes: {resultado.CuotasRestantes}.";

                return RedirectToAction(
                    nameof(CuotasCliente),
                    new { id = credito.IdCliente }
                );
            }
            catch (Exception ex)
            {
                EliminarComprobanteSiExiste(
                    archivoGuardado
                );

                Console.WriteLine(
                    ex.ToString()
                );

                ModelState.AddModelError(
                    string.Empty,
                    $"No fue posible registrar el abono: {ex.Message}"
                );

                await CargarDatosAbono(model);
                await CargarFormasPagoAbono(model);

                return View(model);
            }
        }

        // =====================================================
        // HISTORIAL DE PAGOS
        // =====================================================
        [HttpGet]
        [PermisoAuthorize("PAGOS_HISTORIAL")]
        public async Task<IActionResult> Historial(
            string? cliente,
            string? recibo,
            string? credito,
            string? tipoOperacion)
        {
            var consulta = _context.Pagos
                .Include(p => p.Credito)
                    .ThenInclude(c => c.Cliente)
                        .ThenInclude(c => c.Usuario)
                .Include(p => p.Cuota)
                .Include(p => p.FormaPago)
                .Include(p => p.Banco)
                .Include(p => p.Usuario)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(cliente))
            {
                string textoCliente =
                    cliente.Trim();

                consulta = consulta.Where(p =>
                    p.Credito.Cliente.Usuario.Nombres
                        .Contains(textoCliente) ||
                    p.Credito.Cliente.Usuario.Apellidos
                        .Contains(textoCliente) ||
                    p.Credito.Cliente.CodigoCliente
                        .Contains(textoCliente)
                );
            }

            if (!string.IsNullOrWhiteSpace(recibo))
            {
                string textoRecibo =
                    recibo.Trim();

                consulta = consulta.Where(p =>
                    p.NumeroRecibo.Contains(textoRecibo)
                );
            }

            if (!string.IsNullOrWhiteSpace(credito))
            {
                string textoCredito =
                    credito.Trim();

                consulta = consulta.Where(p =>
                    p.Credito.CodigoCredito
                        .Contains(textoCredito)
                );
            }

            if (!string.IsNullOrWhiteSpace(tipoOperacion))
            {
                consulta = consulta.Where(p =>
                    p.TipoOperacion == tipoOperacion
                );
            }

            var pagos = await consulta
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.IdPago)
                .ToListAsync();

            ViewBag.Cliente = cliente;
            ViewBag.Recibo = recibo;
            ViewBag.Credito = credito;
            ViewBag.TipoOperacion = tipoOperacion;

            return View(pagos);
        }


        // =====================================================
        // VER COMPROBANTE DE UN PAGO
        // =====================================================
        [HttpGet]
        [PermisoAuthorize("PAGOS_HISTORIAL")]
        public async Task<IActionResult> VerComprobantePago(
            ulong id)
        {
            var pago = await _context.Pagos
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.IdPago == id
                );

            if (pago == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(
                pago.ArchivoComprobante
            ))
            {
                TempData["MensajeError"] =
                    "Este pago no tiene un comprobante asociado.";

                return RedirectToAction(
                    nameof(Historial)
                );
            }

            string nombreArchivo =
                Path.GetFileName(
                    pago.ArchivoComprobante
                );

            string ruta =
                Path.Combine(
                    _environment.ContentRootPath,
                    "Storage",
                    "ComprobantesPago",
                    nombreArchivo
                );

            if (!System.IO.File.Exists(ruta))
            {
                TempData["MensajeError"] =
                    "El archivo del comprobante no fue encontrado.";

                return RedirectToAction(
                    nameof(Historial)
                );
            }

            string extension =
                Path.GetExtension(ruta)
                    .ToLowerInvariant();

            string contentType =
                extension switch
                {
                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".pdf" => "application/pdf",
                    _ => "application/octet-stream"
                };

            return PhysicalFile(
                ruta,
                contentType
            );
        }

        // =====================================================
        // CARGAR FORMAS DE PAGO PARA CUOTAS
        // =====================================================
        private async Task CargarFormasPago(
            RegistrarPagoViewModel model)
        {
            model.FormasPago =
                await _context.FormasPago
                    .Where(f =>
                        f.Estado == "ACTIVO")
                    .OrderBy(f =>
                        f.Nombre)
                    .Select(f =>
                        new SelectListItem
                        {
                            Value =
                                f.IdFormaPago.ToString(),

                            Text =
                                f.Nombre
                        })
                    .ToListAsync();

            model.Bancos =
                await _context.Bancos
                    .AsNoTracking()
                    .Where(b =>
                        b.Estado == "ACTIVO")
                    .OrderBy(b =>
                        b.Nombre)
                    .ToListAsync();
        }

        // =====================================================
        // RECARGAR DATOS DE UNA CUOTA
        // =====================================================
        private async Task CargarDatosCuota(
            RegistrarPagoViewModel model)
        {
            var cuota = await _context.Cuotas
                .Include(c => c.Credito)
                    .ThenInclude(c => c.Cliente)
                        .ThenInclude(c => c.Usuario)
                .FirstOrDefaultAsync(c =>
                    c.IdCuota == model.IdCuota);

            if (cuota == null)
                return;

            model.IdCredito =
                cuota.IdCredito;

            model.CodigoCredito =
                cuota.Credito.CodigoCredito;

            model.Cliente =
                cuota.Credito.Cliente.Usuario.Nombres +
                " " +
                cuota.Credito.Cliente.Usuario.Apellidos;

            model.NumeroCuota =
                cuota.NumeroCuota;

            model.FechaVencimiento =
                cuota.FechaVencimiento;

            model.CapitalPendiente =
                Math.Round(
                    cuota.CapitalProgramado -
                    cuota.CapitalPagado,
                    2
                );

            model.InteresPendiente =
                Math.Round(
                    cuota.InteresProgramado -
                    cuota.InteresPagado,
                    2
                );

            model.MoraPendiente =
                0m;

            model.TotalPagar =
                Math.Round(
                    model.CapitalPendiente +
                    model.InteresPendiente +
                    model.MoraPendiente,
                    2
                );
        }

        // =====================================================
        // FORMAS DE PAGO PARA ABONO EXTRAORDINARIO
        // =====================================================
        private async Task CargarFormasPagoAbono(
            AbonoExtraordinarioViewModel model)
        {
            model.FormasPago =
                await _context.FormasPago
                    .Where(f =>
                        f.Estado == "ACTIVO")
                    .OrderBy(f =>
                        f.Nombre)
                    .Select(f =>
                        new SelectListItem
                        {
                            Value =
                                f.IdFormaPago.ToString(),

                            Text =
                                f.Nombre
                        })
                    .ToListAsync();

            model.Bancos =
                await _context.Bancos
                    .AsNoTracking()
                    .Where(b =>
                        b.Estado == "ACTIVO")
                    .OrderBy(b =>
                        b.Nombre)
                    .ToListAsync();
        }

        // =====================================================
        // RECARGAR DATOS DEL CRÉDITO PARA ABONO
        // =====================================================
        private async Task CargarDatosAbono(
            AbonoExtraordinarioViewModel model)
        {
            var credito =
                await _context.Creditos
                    .Include(c => c.Cliente)
                        .ThenInclude(c => c.Usuario)
                    .FirstOrDefaultAsync(c =>
                        c.IdCredito ==
                        model.IdCredito);

            if (credito == null)
                return;

            model.CodigoCredito =
                credito.CodigoCredito;

            model.Cliente =
                credito.Cliente.Usuario.Nombres +
                " " +
                credito.Cliente.Usuario.Apellidos;

            model.SaldoCapital =
                credito.SaldoCapital;

            model.CuotaActual =
                credito.CuotaOriginal;

            model.PlazoOriginal =
                credito.PlazoMeses;
        }

        // =====================================================
        // VALIDAR Y GUARDAR DATOS BANCARIOS
        // =====================================================
        private async Task<PreparacionComprobante>
            PrepararDatosBancariosAsync(
                uint idFormaPago,
                uint? idBanco,
                string? numeroBoleta,
                IFormFile? comprobante)
        {
            var formaPago =
                await _context.FormasPago
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f =>
                        f.IdFormaPago == idFormaPago &&
                        f.Estado == "ACTIVO"
                    );

            if (formaPago == null)
            {
                return PreparacionComprobante.Fallo(
                    "La forma de pago seleccionada no es válida."
                );
            }

            string formaNormalizada =
                formaPago.Nombre
                    .Trim()
                    .ToUpperInvariant();

            bool requiereComprobante =
                formaNormalizada == "TRANSFERENCIA" ||
                formaNormalizada == "DEPOSITO";

            if (!requiereComprobante)
            {
                return PreparacionComprobante.Ok(
                    archivoGuardado: null
                );
            }

            if (!idBanco.HasValue)
            {
                return PreparacionComprobante.Fallo(
                    "Debe seleccionar el banco utilizado."
                );
            }

            var banco =
                await _context.Bancos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b =>
                        b.IdBanco == idBanco.Value &&
                        b.Estado == "ACTIVO"
                    );

            if (banco == null)
            {
                return PreparacionComprobante.Fallo(
                    "El banco seleccionado no existe o está inactivo."
                );
            }

            string boleta =
                numeroBoleta?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(boleta))
            {
                return PreparacionComprobante.Fallo(
                    "Debe ingresar el número de boleta o comprobante."
                );
            }

            if (boleta.Length > 100)
            {
                return PreparacionComprobante.Fallo(
                    "El número de boleta no puede superar los 100 caracteres."
                );
            }

            bool existeEnPagos =
                await _context.Pagos
                    .AsNoTracking()
                    .AnyAsync(p =>
                        p.IdBanco == idBanco.Value &&
                        p.NumeroBoleta == boleta
                    );

            if (existeEnPagos)
            {
                return PreparacionComprobante.Fallo(
                    $"La boleta {boleta} ya fue registrada para {banco.Nombre}."
                );
            }

            bool existeEnSolicitudes =
                await _context.SolicitudesPago
                    .AsNoTracking()
                    .AnyAsync(s =>
                        s.IdBanco == idBanco.Value &&
                        s.NumeroBoleta == boleta
                    );

            if (existeEnSolicitudes)
            {
                return PreparacionComprobante.Fallo(
                    $"La boleta {boleta} ya está registrada en una solicitud de pago para {banco.Nombre}."
                );
            }

            if (
                comprobante == null ||
                comprobante.Length <= 0
            )
            {
                return PreparacionComprobante.Fallo(
                    "Debe adjuntar el comprobante del depósito o transferencia."
                );
            }

            const long maximoBytes =
                5 * 1024 * 1024;

            if (comprobante.Length > maximoBytes)
            {
                return PreparacionComprobante.Fallo(
                    "El comprobante no puede superar los 5 MB."
                );
            }

            string extension =
                Path.GetExtension(
                    comprobante.FileName
                )
                .ToLowerInvariant();

            string[] extensionesPermitidas =
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".pdf"
            };

            if (!extensionesPermitidas.Contains(extension))
            {
                return PreparacionComprobante.Fallo(
                    "El comprobante debe ser JPG, JPEG, PNG o PDF."
                );
            }

            string nombreArchivo =
                $"pago_{Guid.NewGuid():N}{extension}";

            string carpeta =
                Path.Combine(
                    _environment.ContentRootPath,
                    "Storage",
                    "ComprobantesPago"
                );

            Directory.CreateDirectory(carpeta);

            string rutaCompleta =
                Path.Combine(
                    carpeta,
                    nombreArchivo
                );

            await using (
                var stream =
                    new FileStream(
                        rutaCompleta,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None
                    )
            )
            {
                await comprobante.CopyToAsync(
                    stream
                );
            }

            return PreparacionComprobante.Ok(
                nombreArchivo
            );
        }

        // =====================================================
        // ELIMINAR COMPROBANTE SI LA OPERACIÓN FALLA
        // =====================================================
        private void EliminarComprobanteSiExiste(
            string? nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(
                    nombreArchivo))
            {
                return;
            }

            try
            {
                string ruta =
                    Path.Combine(
                        _environment.ContentRootPath,
                        "Storage",
                        "ComprobantesPago",
                        Path.GetFileName(
                            nombreArchivo
                        )
                    );

                if (System.IO.File.Exists(ruta))
                {
                    System.IO.File.Delete(ruta);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "No fue posible eliminar el comprobante temporal {Archivo}.",
                    nombreArchivo
                );
            }
        }

        // =====================================================
        // DETALLE BANCARIO PARA AUDITORÍA
        // =====================================================
        private static string CrearDetalleBancarioAuditoria(
            string? formaPago,
            uint? idBanco,
            string? numeroBoleta)
        {
            string forma =
                formaPago?.Trim().ToUpperInvariant()
                ?? string.Empty;

            if (
                forma != "TRANSFERENCIA" &&
                forma != "DEPOSITO"
            )
            {
                return string.Empty;
            }

            return
                $"Banco ID: {idBanco?.ToString() ?? "Sin dato"}; " +
                $"Boleta: {numeroBoleta ?? "Sin dato"}; ";
        }

        private sealed class PreparacionComprobante
        {
            public bool Exito { get; init; }
            public string? ArchivoGuardado { get; init; }
            public string? MensajeError { get; init; }

            public static PreparacionComprobante Ok(
                string? archivoGuardado)
            {
                return new PreparacionComprobante
                {
                    Exito = true,
                    ArchivoGuardado = archivoGuardado
                };
            }

            public static PreparacionComprobante Fallo(
                string error)
            {
                return new PreparacionComprobante
                {
                    Exito = false,
                    MensajeError = error
                };
            }
        }

        // =====================================================
        // AUDITORÍA SEGURA
        // =====================================================
        private async Task RegistrarAuditoriaSeguroAsync(
            string accion,
            string tablaAfectada,
            string registroAfectado,
            string descripcion,
            string resultado = "EXITOSO")
        {
            try
            {
                await _auditoriaService
                    .RegistrarUsuarioActualAsync(
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