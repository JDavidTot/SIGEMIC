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

        public PagosController(
            ApplicationDbContext context,
            PagoService pagoService)
        {
            _context = context;
            _pagoService = pagoService;
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

            try
            {
                var resultado =
                    await _pagoService
                        .RegistrarPagoCuotaAsync(
                            idCuota: model.IdCuota,
                            montoRecibido: model.MontoPago,
                            idUsuario: idUsuario,
                            idFormaPago: model.IdFormaPago,
                            observacion: model.Observacion
                        );

                if (!resultado.Exito)
                {
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

            try
            {
                var resultado =
                    await _pagoService
                        .RegistrarAbonoExtraordinarioAsync(
                            idCredito: model.IdCredito,
                            montoAbono: model.MontoAbono,
                            idUsuario: idUsuario,
                            idFormaPago: model.IdFormaPago,
                            observacion: model.Observacion
                        );

                if (!resultado.Exito)
                {
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
    }
}