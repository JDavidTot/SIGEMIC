using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Models;
using System.Security.Claims;

namespace SIGEMIC.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR,CAJERO")]
    public class SolicitudesPagoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SolicitudesPagoController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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


            // =================================================
            // FILTRAR POR ESTADO
            // =================================================

            if (
                !string.IsNullOrWhiteSpace(estado) &&
                estado != "TODAS"
            )
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


            // =================================================
            // OBTENER SOLO EL NOMBRE DEL ARCHIVO
            // Evita manipulación de rutas
            // =================================================

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
                // =================================================
                // OBTENER SOLICITUD
                // =================================================

                var solicitud =
                    await _context.SolicitudesPago
                        .Include(s => s.Credito)
                        .Include(s => s.Cuota)
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
                // SOLO SE PUEDE APROBAR UNA SOLICITUD PENDIENTE
                // =================================================

                if (solicitud.Estado != "PENDIENTE")
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
                // EVITAR QUE YA TENGA PAGO VINCULADO
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
                // VALIDAR CRÉDITO
                // =================================================

                var credito =
                    await _context.Creditos
                        .FirstOrDefaultAsync(c =>
                            c.IdCredito ==
                            solicitud.IdCredito
                        );


                if (credito == null)
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] =
                        "El crédito relacionado con la solicitud no existe.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id }
                    );
                }


                if (
                    credito.Estado != "ACTIVO" &&
                    credito.Estado != "ATRASADO"
                )
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] =
                        "El crédito ya no está disponible para recibir pagos.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id }
                    );
                }


                // =================================================
                // FORMA DE PAGO
                // Las solicitudes del cliente son depósitos
                // =================================================

                var formaPago =
                    await _context.FormasPago
                        .FirstOrDefaultAsync(f =>
                            f.Nombre == "DEPOSITO"
                        );


                if (formaPago == null)
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] =
                        "No existe la forma de pago DEPOSITO en la base de datos.";

                    return RedirectToAction(
                        nameof(Details),
                        new { id }
                    );
                }


                // =================================================
                // PAGO DE CUOTA
                // =================================================

                if (
                    solicitud.TipoOperacion ==
                    "PAGO_CUOTA"
                )
                {
                    var resultado =
                        await AplicarPagoCuota(
                            solicitud,
                            credito,
                            formaPago,
                            idUsuario.Value
                        );


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
                }


                // =================================================
                // ABONO EXTRAORDINARIO
                // =================================================

                else if (
                    solicitud.TipoOperacion ==
                    "ABONO_EXTRAORDINARIO"
                )
                {
                    var resultado =
                        await AplicarAbonoExtraordinario(
                            solicitud,
                            credito,
                            formaPago,
                            idUsuario.Value
                        );


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
                }


                // =================================================
                // TIPO DE OPERACIÓN INVÁLIDO
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
                // MARCAR SOLICITUD COMO APROBADA
                // =================================================

                solicitud.Estado =
                    "APROBADA";

                solicitud.IdUsuarioRevision =
                    idUsuario.Value;

                solicitud.FechaRevision =
                    DateTime.Now;

                solicitud.MotivoRechazo =
                    null;


                // =================================================
                // GUARDAR CAMBIOS
                // Incluye:
                // - recibo definitivo
                // - id_pago en solicitud
                // - estado APROBADA
                // - cuota
                // - crédito
                // =================================================

                await _context.SaveChangesAsync();


                await transaction.CommitAsync();


                TempData["Exito"] =
                    "La solicitud fue aprobada y el pago fue aplicado correctamente.";


                return RedirectToAction(
                    nameof(Details),
                    new { id }
                );
            }
            catch
            {
                await transaction.RollbackAsync();

                throw;
            }
        }


        // =====================================================
        // RECHAZAR SOLICITUD
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
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
            // SOLO PENDIENTES
            // =================================================

            if (solicitud.Estado != "PENDIENTE")
            {
                TempData["Error"] =
                    "Esta solicitud ya fue revisada.";

                return RedirectToAction(
                    nameof(Details),
                    new { id }
                );
            }


            // =================================================
            // VALIDAR MOTIVO
            // =================================================

            if (string.IsNullOrWhiteSpace(
                motivoRechazo))
            {
                TempData["Error"] =
                    "Debe indicar el motivo del rechazo.";

                return RedirectToAction(
                    nameof(Details),
                    new { id }
                );
            }


            // =================================================
            // RECHAZAR
            // =================================================

            solicitud.Estado =
                "RECHAZADA";

            solicitud.IdUsuarioRevision =
                idUsuario.Value;

            solicitud.FechaRevision =
                DateTime.Now;

            solicitud.MotivoRechazo =
                motivoRechazo.Trim();


            // Una solicitud rechazada
            // nunca genera pago.

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


        // =====================================================
        // APLICAR PAGO DE CUOTA
        // =====================================================

        private async Task<ResultadoOperacion>
            AplicarPagoCuota(
                SolicitudPago solicitud,
                Credito credito,
                FormaPago formaPago,
                uint idUsuario)
        {
            // =================================================
            // VALIDAR CUOTA
            // =================================================

            if (!solicitud.IdCuota.HasValue)
            {
                return ResultadoOperacion.Error(
                    "La solicitud no tiene una cuota asociada."
                );
            }


            var cuota =
                await _context.Cuotas
                    .FirstOrDefaultAsync(c =>
                        c.IdCuota ==
                            solicitud.IdCuota.Value &&
                        c.IdCredito ==
                            credito.IdCredito
                    );


            if (cuota == null)
            {
                return ResultadoOperacion.Error(
                    "La cuota asociada ya no existe."
                );
            }


            // =================================================
            // VALIDAR ESTADO DE CUOTA
            // =================================================

            if (cuota.Estado == "PAGADA")
            {
                return ResultadoOperacion.Error(
                    "La cuota ya se encuentra pagada."
                );
            }


            if (
                cuota.Estado != "PENDIENTE" &&
                cuota.Estado != "PARCIAL" &&
                cuota.Estado != "ATRASADA"
            )
            {
                return ResultadoOperacion.Error(
                    "La cuota no está disponible para recibir pagos."
                );
            }


            // =================================================
            // VALIDAR MONTO
            // =================================================

            if (
                solicitud.Monto >
                cuota.SaldoCuota
            )
            {
                return ResultadoOperacion.Error(
                    $"El saldo actual de la cuota es Q{cuota.SaldoCuota:N2}. " +
                    "La solicitud no puede aprobarse porque el monto es superior."
                );
            }


            if (solicitud.Monto <= 0)
            {
                return ResultadoOperacion.Error(
                    "El monto del pago no es válido."
                );
            }


            // =================================================
            // CALCULAR INTERÉS PENDIENTE
            // =================================================

            var interesPendiente =
                Math.Max(
                    0m,
                    cuota.InteresProgramado -
                    cuota.InteresPagado
                );


            // =================================================
            // CALCULAR CAPITAL PENDIENTE
            // =================================================

            var capitalPendiente =
                Math.Max(
                    0m,
                    cuota.CapitalProgramado -
                    cuota.CapitalPagado
                );


            var montoRestante =
                solicitud.Monto;


            // =================================================
            // PRIMERO INTERÉS
            // =================================================

            var interesAplicado =
                Math.Min(
                    montoRestante,
                    interesPendiente
                );


            montoRestante =
                Math.Round(
                    montoRestante -
                    interesAplicado,
                    2
                );


            // =================================================
            // DESPUÉS CAPITAL
            // =================================================

            var capitalAplicado =
                Math.Min(
                    montoRestante,
                    capitalPendiente
                );


            montoRestante =
                Math.Round(
                    montoRestante -
                    capitalAplicado,
                    2
                );


            // =================================================
            // MORA
            // Actualmente permanece en cero
            // =================================================

            var moraAplicada =
                0m;


            if (montoRestante > 0.01m)
            {
                return ResultadoOperacion.Error(
                    "El monto de la solicitud es superior al saldo pendiente de la cuota."
                );
            }


            // =================================================
            // ACTUALIZAR INTERÉS PAGADO
            // =================================================

            cuota.InteresPagado =
                Math.Round(
                    cuota.InteresPagado +
                    interesAplicado,
                    2
                );


            // =================================================
            // ACTUALIZAR CAPITAL PAGADO
            // =================================================

            cuota.CapitalPagado =
                Math.Round(
                    cuota.CapitalPagado +
                    capitalAplicado,
                    2
                );


            // =================================================
            // ACTUALIZAR SALDO DE CUOTA
            // =================================================

            cuota.SaldoCuota =
                Math.Round(
                    Math.Max(
                        0m,
                        cuota.SaldoCuota -
                        solicitud.Monto
                    ),
                    2
                );


            // =================================================
            // ESTADO DE CUOTA
            // =================================================

            if (cuota.SaldoCuota <= 0.01m)
            {
                cuota.SaldoCuota =
                    0m;

                cuota.Estado =
                    "PAGADA";
            }
            else
            {
                cuota.Estado =
                    "PARCIAL";
            }


            // =================================================
            // ACTUALIZAR SALDO CAPITAL DEL CRÉDITO
            // Solamente se descuenta capital
            // =================================================

            credito.SaldoCapital =
                Math.Round(
                    Math.Max(
                        0m,
                        credito.SaldoCapital -
                        capitalAplicado
                    ),
                    2
                );


            // =================================================
            // CREAR PAGO
            // =================================================

            var pago =
                new Pago
                {
                    IdCredito =
                        credito.IdCredito,

                    IdCuota =
                        cuota.IdCuota,

                    IdUsuario =
                        idUsuario,

                    IdFormaPago =
                        formaPago.IdFormaPago,

                    NumeroRecibo =
                        CrearReciboTemporal(),

                    TipoOperacion =
                        "PAGO_CUOTA",

                    MontoTotal =
                        solicitud.Monto,

                    CapitalAplicado =
                        Math.Round(
                            capitalAplicado,
                            2
                        ),

                    InteresAplicado =
                        Math.Round(
                            interesAplicado,
                            2
                        ),

                    MoraAplicada =
                        moraAplicada,

                    FechaPago =
                        DateTime.Now,

                    Observacion =
                        $"Pago aprobado desde solicitud #{solicitud.IdSolicitud}. " +
                        $"Banco: {solicitud.Banco?.Nombre ?? solicitud.IdBanco.ToString()}. " +
                        $"Boleta: {solicitud.NumeroBoleta}."
                };


            _context.Pagos.Add(
                pago
            );


            // =================================================
            // GUARDAR PARA OBTENER ID DEL PAGO
            // =================================================

            await _context.SaveChangesAsync();


            // =================================================
            // GENERAR RECIBO DEFINITIVO
            // =================================================

            pago.NumeroRecibo =
                $"REC-{pago.IdPago:D6}";


            // =================================================
            // VINCULAR SOLICITUD CON PAGO
            // =================================================

            solicitud.IdPago =
                pago.IdPago;


            // =================================================
            // COMPROBAR SI EL CRÉDITO TERMINÓ
            // =================================================

            if (cuota.Estado == "PAGADA")
            {
                var hayOtrasCuotasPendientes =
                    await _context.Cuotas
                        .AnyAsync(c =>
                            c.IdCredito ==
                                credito.IdCredito &&
                            c.IdCuota !=
                                cuota.IdCuota &&
                            c.Estado !=
                                "PAGADA"
                        );


                if (!hayOtrasCuotasPendientes)
                {
                    credito.Estado =
                        "CANCELADO";

                    credito.SaldoCapital =
                        0m;

                    credito.FechaCancelacion =
                        DateTime.Now;
                }
            }


            return ResultadoOperacion.Ok();
        }


        // =====================================================
        // APLICAR ABONO EXTRAORDINARIO
        // =====================================================

        private async Task<ResultadoOperacion>
            AplicarAbonoExtraordinario(
                SolicitudPago solicitud,
                Credito credito,
                FormaPago formaPago,
                uint idUsuario)
        {
            // =================================================
            // VALIDAR MONTO
            // =================================================

            if (solicitud.Monto <= 0)
            {
                return ResultadoOperacion.Error(
                    "El monto del abono no es válido."
                );
            }


            if (
                solicitud.Monto >
                credito.SaldoCapital
            )
            {
                return ResultadoOperacion.Error(
                    $"El saldo actual del crédito es Q{credito.SaldoCapital:N2}. " +
                    "El abono no puede superar ese monto."
                );
            }


            // =================================================
            // NO PERMITIR ABONO SI EXISTE CUOTA
            // PARCIAL O ATRASADA
            // =================================================

            var cuotaConProblema =
                await _context.Cuotas
                    .AnyAsync(c =>
                        c.IdCredito ==
                            credito.IdCredito &&
                        (
                            c.Estado == "PARCIAL" ||
                            c.Estado == "ATRASADA"
                        )
                    );


            if (cuotaConProblema)
            {
                return ResultadoOperacion.Error(
                    "No se puede aprobar el abono extraordinario mientras exista una cuota parcial o atrasada."
                );
            }


            // =================================================
            // APLICAR TODO EL ABONO A CAPITAL
            // =================================================

            credito.SaldoCapital =
                Math.Round(
                    Math.Max(
                        0m,
                        credito.SaldoCapital -
                        solicitud.Monto
                    ),
                    2
                );


            // =================================================
            // CREAR PAGO
            // =================================================

            var pago =
                new Pago
                {
                    IdCredito =
                        credito.IdCredito,

                    IdCuota =
                        null,

                    IdUsuario =
                        idUsuario,

                    IdFormaPago =
                        formaPago.IdFormaPago,

                    NumeroRecibo =
                        CrearReciboTemporal(),

                    TipoOperacion =
                        "ABONO_EXTRAORDINARIO",

                    MontoTotal =
                        solicitud.Monto,

                    CapitalAplicado =
                        solicitud.Monto,

                    InteresAplicado =
                        0m,

                    MoraAplicada =
                        0m,

                    FechaPago =
                        DateTime.Now,

                    Observacion =
                        $"Abono aprobado desde solicitud #{solicitud.IdSolicitud}. " +
                        $"Banco: {solicitud.Banco?.Nombre ?? solicitud.IdBanco.ToString()}. " +
                        $"Boleta: {solicitud.NumeroBoleta}."
                };


            _context.Pagos.Add(
                pago
            );


            // =================================================
            // GUARDAR PARA OBTENER ID DEL PAGO
            // =================================================

            await _context.SaveChangesAsync();


            // =================================================
            // GENERAR RECIBO DEFINITIVO
            // =================================================

            pago.NumeroRecibo =
                $"REC-{pago.IdPago:D6}";


            // =================================================
            // VINCULAR SOLICITUD CON PAGO
            // =================================================

            solicitud.IdPago =
                pago.IdPago;


            // =================================================
            // SI EL ABONO CANCELÓ TODO EL CRÉDITO
            // =================================================

            if (credito.SaldoCapital <= 0.01m)
            {
                credito.SaldoCapital =
                    0m;

                credito.Estado =
                    "CANCELADO";

                credito.FechaCancelacion =
                    DateTime.Now;


                var cuotasPendientes =
                    await _context.Cuotas
                        .Where(c =>
                            c.IdCredito ==
                                credito.IdCredito &&
                            c.Estado ==
                                "PENDIENTE"
                        )
                        .ToListAsync();


                if (cuotasPendientes.Any())
                {
                    _context.Cuotas.RemoveRange(
                        cuotasPendientes
                    );
                }


                return ResultadoOperacion.Ok();
            }


            // =================================================
            // OBTENER CUOTAS FUTURAS
            // =================================================

            var futuras =
                await _context.Cuotas
                    .Where(c =>
                        c.IdCredito ==
                            credito.IdCredito &&
                        c.Estado ==
                            "PENDIENTE"
                    )
                    .OrderBy(c =>
                        c.NumeroCuota
                    )
                    .ToListAsync();


            if (!futuras.Any())
            {
                return ResultadoOperacion.Error(
                    "No existen cuotas pendientes para recalcular el crédito."
                );
            }


            // =================================================
            // CALCULAR TASA MENSUAL
            // =================================================

            decimal tasaMensual;


            if (
                credito.TipoTasaAplicada ==
                "ANUAL"
            )
            {
                tasaMensual =
                    credito.PorcentajeInteresAplicado
                    / 12m
                    / 100m;
            }
            else
            {
                tasaMensual =
                    credito.PorcentajeInteresAplicado
                    / 100m;
            }


            // =================================================
            // RECALCULAR PLAN
            // Mantener cuota aproximada y reducir plazo
            // =================================================

            var saldoCalculo =
                credito.SaldoCapital;


            var cuotasUtilizadas =
                0;


            foreach (var cuota in futuras)
            {
                if (saldoCalculo <= 0.01m)
                {
                    break;
                }


                var interes =
                    Math.Round(
                        saldoCalculo *
                        tasaMensual,
                        2
                    );


                var capital =
                    credito.CuotaOriginal -
                    interes;


                if (capital <= 0)
                {
                    return ResultadoOperacion.Error(
                        "No fue posible recalcular el crédito con la tasa actual."
                    );
                }


                if (capital > saldoCalculo)
                {
                    capital =
                        saldoCalculo;
                }


                var cuotaProgramada =
                    Math.Round(
                        capital +
                        interes,
                        2
                    );


                // =============================================
                // ACTUALIZAR CUOTA
                // =============================================

                cuota.CapitalProgramado =
                    Math.Round(
                        capital,
                        2
                    );


                cuota.InteresProgramado =
                    Math.Round(
                        interes,
                        2
                    );


                cuota.CuotaProgramada =
                    cuotaProgramada;


                cuota.CapitalPagado =
                    0m;


                cuota.InteresPagado =
                    0m;


                cuota.MoraPagada =
                    0m;


                cuota.SaldoCuota =
                    cuotaProgramada;


                cuota.Estado =
                    "PENDIENTE";


                saldoCalculo =
                    Math.Round(
                        Math.Max(
                            0m,
                            saldoCalculo -
                            capital
                        ),
                        2
                    );


                cuotasUtilizadas++;
            }


            // =================================================
            // COMPROBAR QUE EL RECÁLCULO TERMINÓ
            // =================================================

            if (saldoCalculo > 0.01m)
            {
                return ResultadoOperacion.Error(
                    "No fue posible completar el nuevo plan de pagos."
                );
            }


            // =================================================
            // ELIMINAR CUOTAS FUTURAS QUE YA NO SE NECESITAN
            // =================================================
            //
            // Esta lógica se mantiene igual a la que ya veníamos
            // utilizando. Más adelante la cambiaremos por una
            // estrategia que conserve historial.
            // =================================================

            var cuotasSobrantes =
                futuras
                    .Skip(
                        cuotasUtilizadas
                    )
                    .ToList();


            if (cuotasSobrantes.Any())
            {
                _context.Cuotas.RemoveRange(
                    cuotasSobrantes
                );
            }


            // =================================================
            // ACTUALIZAR FECHA FINAL DEL CRÉDITO
            // =================================================

            var ultimaCuota =
                futuras[
                    cuotasUtilizadas - 1
                ];


            credito.FechaFinalizacionEstimada =
                ultimaCuota.FechaVencimiento;


            return ResultadoOperacion.Ok();
        }


        // =====================================================
        // CREAR NÚMERO DE RECIBO TEMPORAL
        // =====================================================

        private static string CrearReciboTemporal()
        {
            return
                "TMP-" +
                Guid.NewGuid()
                    .ToString("N")
                    .Substring(0, 20);
        }


        // =====================================================
        // RESULTADO INTERNO DE OPERACIONES
        // =====================================================

        private class ResultadoOperacion
        {
            public bool Exito { get; set; }

            public string Mensaje { get; set; }
                = string.Empty;


            // =================================================
            // OPERACIÓN EXITOSA
            // =================================================

            public static ResultadoOperacion Ok()
            {
                return new ResultadoOperacion
                {
                    Exito = true
                };
            }


            // =================================================
            // OPERACIÓN FALLIDA
            // =================================================

            public static ResultadoOperacion Error(
                string mensaje)
            {
                return new ResultadoOperacion
                {
                    Exito = false,
                    Mensaje = mensaje
                };
            }
        }
    }
}