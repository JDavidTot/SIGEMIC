using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SIGEMIC.Data;
using SIGEMIC.Models;

namespace SIGEMIC.Services
{
    public class PagoService
    {
        private readonly ApplicationDbContext _context;

        public PagoService(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // REGISTRAR PAGO COMPLETO O PARCIAL DE UNA CUOTA
        // =====================================================

        public async Task<ResultadoPagoService>
            RegistrarPagoCuotaAsync(
                ulong idCuota,
                decimal montoRecibido,
                uint idUsuario,
                uint idFormaPago,
                string? observacion = null)
        {
            IDbContextTransaction? transaccionPropia =
                null;


            // =================================================
            // SI EL CONTROLADOR YA TIENE UNA TRANSACCIÓN,
            // UTILIZAMOS ESA MISMA.
            //
            // SI NO EXISTE, EL SERVICIO CREA UNA.
            // =================================================

            if (_context.Database.CurrentTransaction == null)
            {
                transaccionPropia =
                    await _context.Database
                        .BeginTransactionAsync();
            }


            try
            {
                // =================================================
                // OBTENER CUOTA Y CRÉDITO
                // =================================================

                var cuota =
                    await _context.Cuotas
                        .Include(c => c.Credito)
                        .FirstOrDefaultAsync(c =>
                            c.IdCuota == idCuota
                        );


                if (cuota == null)
                {
                    return ResultadoPagoService.Error(
                        "La cuota seleccionada no existe."
                    );
                }


                var credito =
                    cuota.Credito;


                // =================================================
                // VALIDAR ESTADO DEL CRÉDITO
                // =================================================

                if (
                    credito.Estado != "ACTIVO" &&
                    credito.Estado != "ATRASADO"
                )
                {
                    return ResultadoPagoService.Error(
                        "El crédito no está disponible para recibir pagos."
                    );
                }


                // =================================================
                // VALIDAR ESTADO DE LA CUOTA
                // =================================================

                if (cuota.Estado == "PAGADA")
                {
                    return ResultadoPagoService.Error(
                        "La cuota seleccionada ya está pagada."
                    );
                }


                if (
                    cuota.Estado != "PENDIENTE" &&
                    cuota.Estado != "PARCIAL" &&
                    cuota.Estado != "ATRASADA"
                )
                {
                    return ResultadoPagoService.Error(
                        "La cuota no está disponible para recibir pagos."
                    );
                }


                // =================================================
                // VALIDAR FORMA DE PAGO
                // =================================================

                var formaPago =
                    await _context.FormasPago
                        .FirstOrDefaultAsync(f =>
                            f.IdFormaPago ==
                                idFormaPago &&
                            f.Estado ==
                                "ACTIVO"
                        );


                if (formaPago == null)
                {
                    return ResultadoPagoService.Error(
                        "La forma de pago seleccionada no es válida."
                    );
                }


                // =================================================
                // SALDOS REALES DE LA CUOTA
                // =================================================

                decimal capitalPendiente =
                    Math.Round(
                        Math.Max(
                            0m,
                            cuota.CapitalProgramado -
                            cuota.CapitalPagado
                        ),
                        2
                    );


                decimal interesPendiente =
                    Math.Round(
                        Math.Max(
                            0m,
                            cuota.InteresProgramado -
                            cuota.InteresPagado
                        ),
                        2
                    );


                // =================================================
                // MORA
                // Actualmente SIGEMIC todavía no la aplica.
                // =================================================

                decimal moraPendiente =
                    0m;


                decimal saldoCuota =
                    Math.Round(
                        capitalPendiente +
                        interesPendiente +
                        moraPendiente,
                        2
                    );


                // =================================================
                // VALIDAR MONTO
                // =================================================

                montoRecibido =
                    Math.Round(
                        montoRecibido,
                        2
                    );


                if (montoRecibido <= 0)
                {
                    return ResultadoPagoService.Error(
                        "El monto debe ser mayor que cero."
                    );
                }


                if (montoRecibido > saldoCuota)
                {
                    return ResultadoPagoService.Error(
                        $"El monto no puede ser mayor al saldo de la cuota: Q {saldoCuota:N2}."
                    );
                }


                // =================================================
                // DISTRIBUIR EL PAGO
                //
                // 1. INTERÉS
                // 2. CAPITAL
                // 3. MORA CUANDO SEA IMPLEMENTADA
                // =================================================

                decimal disponible =
                    montoRecibido;


                decimal interesAplicado =
                    Math.Min(
                        disponible,
                        interesPendiente
                    );


                disponible =
                    Math.Round(
                        disponible -
                        interesAplicado,
                        2
                    );


                decimal capitalAplicado =
                    Math.Min(
                        disponible,
                        capitalPendiente
                    );


                disponible =
                    Math.Round(
                        disponible -
                        capitalAplicado,
                        2
                    );


                decimal moraAplicada =
                    0m;


                if (disponible > 0.01m)
                {
                    return ResultadoPagoService.Error(
                        "El monto recibido supera el saldo pendiente de la cuota."
                    );
                }


                // =================================================
                // ACTUALIZAR CUOTA
                // =================================================

                cuota.CapitalPagado =
                    Math.Round(
                        cuota.CapitalPagado +
                        capitalAplicado,
                        2
                    );


                cuota.InteresPagado =
                    Math.Round(
                        cuota.InteresPagado +
                        interesAplicado,
                        2
                    );


                cuota.MoraPagada =
                    Math.Round(
                        cuota.MoraPagada +
                        moraAplicada,
                        2
                    );


                decimal nuevoSaldoCuota =
                    Math.Round(
                        cuota.CuotaProgramada -
                        cuota.CapitalPagado -
                        cuota.InteresPagado -
                        cuota.MoraPagada,
                        2
                    );


                if (nuevoSaldoCuota < 0)
                {
                    nuevoSaldoCuota =
                        0m;
                }


                cuota.SaldoCuota =
                    nuevoSaldoCuota;


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
                // ACTUALIZAR CAPITAL DEL CRÉDITO
                // =================================================

                credito.SaldoCapital =
                    Math.Round(
                        credito.SaldoCapital -
                        capitalAplicado,
                        2
                    );


                if (credito.SaldoCapital < 0)
                {
                    credito.SaldoCapital =
                        0m;
                }


                // =================================================
                // VERIFICAR CUOTAS RESTANTES
                // =================================================

                bool existenCuotasPendientes =
                    await _context.Cuotas
                        .AnyAsync(c =>
                            c.IdCredito ==
                                credito.IdCredito &&
                            c.IdCuota !=
                                cuota.IdCuota &&
                            c.Estado !=
                                "PAGADA"
                        );


                // =================================================
                // CANCELAR CRÉDITO SI YA TERMINÓ
                // =================================================

                bool creditoCancelado =
                    false;


                if (
                    cuota.Estado == "PAGADA" &&
                    !existenCuotasPendientes
                )
                {
                    credito.SaldoCapital =
                        0m;

                    credito.Estado =
                        "CANCELADO";

                    credito.FechaCancelacion =
                        DateTime.Today;

                    creditoCancelado =
                        true;
                }


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
                            idFormaPago,

                        NumeroRecibo =
                            CrearReciboTemporal(),

                        TipoOperacion =
                            "PAGO_CUOTA",

                        MontoTotal =
                            montoRecibido,

                        CapitalAplicado =
                            capitalAplicado,

                        InteresAplicado =
                            interesAplicado,

                        MoraAplicada =
                            moraAplicada,

                        FechaPago =
                            DateTime.Now,

                        Observacion =
                            observacion
                    };


                _context.Pagos.Add(
                    pago
                );


                // =================================================
                // PRIMER GUARDADO
                // Obtiene el ID autoincremental.
                // =================================================

                await _context.SaveChangesAsync();


                // =================================================
                // RECIBO DEFINITIVO
                // =================================================

                pago.NumeroRecibo =
                    $"REC-{pago.IdPago:D6}";


                await _context.SaveChangesAsync();


                // =================================================
                // COMMIT SOLO SI EL SERVICIO CREÓ LA TRANSACCIÓN
                // =================================================

                if (transaccionPropia != null)
                {
                    await transaccionPropia
                        .CommitAsync();
                }


                return ResultadoPagoService.Ok(
                    pago: pago,
                    credito: credito,
                    cuota: cuota,
                    creditoCancelado:
                        creditoCancelado
                );
            }
            catch
            {
                if (transaccionPropia != null)
                {
                    await transaccionPropia
                        .RollbackAsync();
                }


                throw;
            }
            finally
            {
                if (transaccionPropia != null)
                {
                    await transaccionPropia
                        .DisposeAsync();
                }
            }
        }


        // =====================================================
        // REGISTRAR ABONO EXTRAORDINARIO A CAPITAL
        // =====================================================

        public async Task<ResultadoPagoService>
            RegistrarAbonoExtraordinarioAsync(
                ulong idCredito,
                decimal montoAbono,
                uint idUsuario,
                uint idFormaPago,
                string? observacion = null)
        {
            IDbContextTransaction? transaccionPropia =
                null;


            if (_context.Database.CurrentTransaction == null)
            {
                transaccionPropia =
                    await _context.Database
                        .BeginTransactionAsync();
            }


            try
            {
                // =================================================
                // OBTENER CRÉDITO
                // =================================================

                var credito =
                    await _context.Creditos
                        .FirstOrDefaultAsync(c =>
                            c.IdCredito ==
                                idCredito
                        );


                if (credito == null)
                {
                    return ResultadoPagoService.Error(
                        "El crédito seleccionado no existe."
                    );
                }


                // =================================================
                // VALIDAR ESTADO
                // =================================================

                if (
                    credito.Estado != "ACTIVO" &&
                    credito.Estado != "ATRASADO"
                )
                {
                    return ResultadoPagoService.Error(
                        "El crédito no está disponible para recibir abonos."
                    );
                }


                // =================================================
                // VALIDAR CUOTAS PARCIALES O ATRASADAS
                // =================================================

                bool cuotaIrregular =
                    await _context.Cuotas
                        .AnyAsync(c =>
                            c.IdCredito ==
                                credito.IdCredito &&
                            (
                                c.Estado ==
                                    "PARCIAL" ||
                                c.Estado ==
                                    "ATRASADA"
                            )
                        );


                if (cuotaIrregular)
                {
                    return ResultadoPagoService.Error(
                        "Primero debe completar las cuotas parciales o atrasadas."
                    );
                }


                // =================================================
                // VALIDAR FORMA DE PAGO
                // =================================================

                var formaPago =
                    await _context.FormasPago
                        .FirstOrDefaultAsync(f =>
                            f.IdFormaPago ==
                                idFormaPago &&
                            f.Estado ==
                                "ACTIVO"
                        );


                if (formaPago == null)
                {
                    return ResultadoPagoService.Error(
                        "La forma de pago seleccionada no es válida."
                    );
                }


                // =================================================
                // VALIDAR MONTO
                // =================================================

                montoAbono =
                    Math.Round(
                        montoAbono,
                        2
                    );


                if (montoAbono <= 0)
                {
                    return ResultadoPagoService.Error(
                        "El monto debe ser mayor que cero."
                    );
                }


                if (montoAbono > credito.SaldoCapital)
                {
                    return ResultadoPagoService.Error(
                        $"El abono no puede superar el saldo de capital: Q {credito.SaldoCapital:N2}."
                    );
                }


                // =================================================
                // NUEVO SALDO DE CAPITAL
                // Todavía no modificamos la entidad.
                // =================================================

                decimal nuevoSaldoCapital =
                    Math.Round(
                        credito.SaldoCapital -
                        montoAbono,
                        2
                    );


                if (nuevoSaldoCapital < 0)
                {
                    nuevoSaldoCapital =
                        0m;
                }


                // =================================================
                // VARIABLES PARA RESULTADO
                // =================================================

                int cuotasNecesarias =
                    0;


                List<Cuota> cuotasFuturas =
                    new();


                // =================================================
                // SI NO CANCELA EL CRÉDITO,
                // PRECALCULAMOS EL NUEVO PLAZO.
                // =================================================

                if (nuevoSaldoCapital > 0.01m)
                {
                    // =============================================
                    // TASA MENSUAL
                    // =============================================

                    decimal tasaMensual;


                    if (
                        credito.TipoTasaAplicada ==
                        "ANUAL"
                    )
                    {
                        tasaMensual =
                            credito
                                .PorcentajeInteresAplicado
                            / 12m
                            / 100m;
                    }
                    else
                    {
                        tasaMensual =
                            credito
                                .PorcentajeInteresAplicado
                            / 100m;
                    }


                    // =============================================
                    // CUOTAS FUTURAS
                    // =============================================

                    cuotasFuturas =
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


                    if (!cuotasFuturas.Any())
                    {
                        return ResultadoPagoService.Error(
                            "No existen cuotas futuras para recalcular."
                        );
                    }


                    decimal cuotaObjetivo =
                        credito.CuotaOriginal;


                    decimal saldoTemporal =
                        nuevoSaldoCapital;


                    // =============================================
                    // CALCULAR CUÁNTAS CUOTAS SERÁN NECESARIAS
                    // =============================================

                    while (saldoTemporal > 0.01m)
                    {
                        decimal interes =
                            Math.Round(
                                saldoTemporal *
                                tasaMensual,
                                2
                            );


                        decimal capital =
                            Math.Round(
                                cuotaObjetivo -
                                interes,
                                2
                            );


                        if (capital <= 0)
                        {
                            return ResultadoPagoService.Error(
                                "La cuota actual no alcanza para amortizar el crédito."
                            );
                        }


                        if (capital > saldoTemporal)
                        {
                            capital =
                                saldoTemporal;
                        }


                        saldoTemporal =
                            Math.Round(
                                saldoTemporal -
                                capital,
                                2
                            );


                        cuotasNecesarias++;


                        if (cuotasNecesarias > 600)
                        {
                            return ResultadoPagoService.Error(
                                "No fue posible recalcular el nuevo plazo."
                            );
                        }
                    }


                    if (
                        cuotasNecesarias >
                        cuotasFuturas.Count
                    )
                    {
                        return ResultadoPagoService.Error(
                            "El nuevo calendario requiere más cuotas que las disponibles."
                        );
                    }


                    // =============================================
                    // APLICAR NUEVO CALENDARIO
                    // =============================================

                    decimal saldoRestante =
                        nuevoSaldoCapital;


                    for (
                        int i = 0;
                        i < cuotasNecesarias;
                        i++
                    )
                    {
                        var cuota =
                            cuotasFuturas[i];


                        decimal interes =
                            Math.Round(
                                saldoRestante *
                                tasaMensual,
                                2
                            );


                        decimal capital =
                            Math.Round(
                                cuotaObjetivo -
                                interes,
                                2
                            );


                        if (
                            i ==
                                cuotasNecesarias - 1 ||
                            capital >
                                saldoRestante
                        )
                        {
                            capital =
                                saldoRestante;
                        }


                        decimal totalCuota =
                            Math.Round(
                                capital +
                                interes,
                                2
                            );


                        cuota.CapitalProgramado =
                            capital;

                        cuota.InteresProgramado =
                            interes;

                        cuota.CuotaProgramada =
                            totalCuota;

                        cuota.CapitalPagado =
                            0m;

                        cuota.InteresPagado =
                            0m;

                        cuota.MoraPagada =
                            0m;

                        cuota.SaldoCuota =
                            totalCuota;

                        cuota.Estado =
                            "PENDIENTE";


                        saldoRestante =
                            Math.Round(
                                saldoRestante -
                                capital,
                                2
                            );
                    }


                    // =============================================
                    // NUEVA FECHA FINAL
                    // =============================================

                    var ultimaCuotaNecesaria =
                        cuotasFuturas[
                            cuotasNecesarias - 1
                        ];


                    credito.FechaFinalizacionEstimada =
                        ultimaCuotaNecesaria
                            .FechaVencimiento;


                    // =============================================
                    // ELIMINAR CUOTAS FUTURAS SOBRANTES
                    //
                    // Se conserva por ahora la misma lógica que
                    // SIGEMIC ya utilizaba.
                    // Más adelante la cambiaremos para conservar
                    // historial de recalculaciones.
                    // =============================================

                    if (
                        cuotasNecesarias <
                        cuotasFuturas.Count
                    )
                    {
                        var cuotasSobrantes =
                            cuotasFuturas
                                .Skip(
                                    cuotasNecesarias
                                )
                                .ToList();


                        _context.Cuotas.RemoveRange(
                            cuotasSobrantes
                        );
                    }
                }


                // =================================================
                // ACTUALIZAR CAPITAL
                // =================================================

                credito.SaldoCapital =
                    nuevoSaldoCapital;


                bool creditoCancelado =
                    false;


                // =================================================
                // SI CANCELÓ TODO EL CRÉDITO
                // =================================================

                if (credito.SaldoCapital <= 0.01m)
                {
                    credito.SaldoCapital =
                        0m;

                    credito.Estado =
                        "CANCELADO";

                    credito.FechaCancelacion =
                        DateTime.Today;

                    creditoCancelado =
                        true;


                    // =============================================
                    // QUITAR CUOTAS FUTURAS PENDIENTES
                    // =============================================

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


                    cuotasNecesarias =
                        0;
                }


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
                            idFormaPago,

                        NumeroRecibo =
                            CrearReciboTemporal(),

                        TipoOperacion =
                            "ABONO_EXTRAORDINARIO",

                        MontoTotal =
                            montoAbono,

                        CapitalAplicado =
                            montoAbono,

                        InteresAplicado =
                            0m,

                        MoraAplicada =
                            0m,

                        FechaPago =
                            DateTime.Now,

                        Observacion =
                            observacion
                    };


                _context.Pagos.Add(
                    pago
                );


                // =================================================
                // GUARDAR OPERACIÓN
                // =================================================

                await _context.SaveChangesAsync();


                // =================================================
                // RECIBO DEFINITIVO
                // =================================================

                pago.NumeroRecibo =
                    $"REC-{pago.IdPago:D6}";


                await _context.SaveChangesAsync();


                if (transaccionPropia != null)
                {
                    await transaccionPropia
                        .CommitAsync();
                }


                return ResultadoPagoService.Ok(
                    pago: pago,
                    credito: credito,
                    cuota: null,
                    creditoCancelado:
                        creditoCancelado,
                    cuotasRestantes:
                        cuotasNecesarias
                );
            }
            catch
            {
                if (transaccionPropia != null)
                {
                    await transaccionPropia
                        .RollbackAsync();
                }


                throw;
            }
            finally
            {
                if (transaccionPropia != null)
                {
                    await transaccionPropia
                        .DisposeAsync();
                }
            }
        }


        // =====================================================
        // CREAR RECIBO TEMPORAL
        // =====================================================

        private static string CrearReciboTemporal()
        {
            return
                $"TMP-{Guid.NewGuid().ToString("N")[..20]}";
        }
    }


    // =========================================================
    // RESULTADO DEL SERVICIO
    // =========================================================

    public class ResultadoPagoService
    {
        public bool Exito { get; set; }

        public string Mensaje { get; set; }
            = string.Empty;


        public Pago? Pago { get; set; }

        public Credito? Credito { get; set; }

        public Cuota? Cuota { get; set; }


        public bool CreditoCancelado { get; set; }

        public int CuotasRestantes { get; set; }


        // =====================================================
        // RESULTADO EXITOSO
        // =====================================================

        public static ResultadoPagoService Ok(
            Pago pago,
            Credito credito,
            Cuota? cuota,
            bool creditoCancelado,
            int cuotasRestantes = 0)
        {
            return new ResultadoPagoService
            {
                Exito =
                    true,

                Pago =
                    pago,

                Credito =
                    credito,

                Cuota =
                    cuota,

                CreditoCancelado =
                    creditoCancelado,

                CuotasRestantes =
                    cuotasRestantes
            };
        }


        // =====================================================
        // RESULTADO CON ERROR
        // =====================================================

        public static ResultadoPagoService Error(
            string mensaje)
        {
            return new ResultadoPagoService
            {
                Exito =
                    false,

                Mensaje =
                    mensaje
            };
        }
    }
}