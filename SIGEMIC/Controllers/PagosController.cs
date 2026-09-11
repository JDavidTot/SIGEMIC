using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Models;

namespace SIGEMIC.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR,CAJERO")]
    public class PagosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PagosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // CLIENTES CON CRÉDITOS ACTIVOS


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


        // CUOTAS PENDIENTES DE UN CLIENTE


        public async Task<IActionResult> CuotasCliente(uint id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c =>
                    c.IdCliente == id);

            if (cliente == null)
            {
                return NotFound();
            }

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

            ViewBag.IdCliente =
                cliente.IdCliente;

            ViewBag.CodigoCliente =
                cliente.CodigoCliente;

            ViewBag.NombreCliente =
                cliente.Usuario.Nombres +
                " " +
                cliente.Usuario.Apellidos;

            return View(cuotas);
        }


        // REGISTRAR PAGO - GET


        [HttpGet]
        public async Task<IActionResult> Registrar(ulong id)
        {
            var cuota = await _context.Cuotas
                .Include(c => c.Credito)
                    .ThenInclude(c => c.Cliente)
                        .ThenInclude(c => c.Usuario)
                .FirstOrDefaultAsync(c =>
                    c.IdCuota == id);

            if (cuota == null)
            {
                return NotFound();
            }

            if (cuota.Estado == "PAGADA")
            {
                TempData["MensajeError"] =
                    "La cuota seleccionada ya está pagada.";

                return RedirectToAction(
                    nameof(CuotasCliente),
                    new
                    {
                        id = cuota.Credito.IdCliente
                    }
                );
            }

            if (cuota.Credito.Estado == "CANCELADO")
            {
                TempData["MensajeError"] =
                    "No se pueden registrar pagos en un crédito cancelado.";

                return RedirectToAction(
                    nameof(CuotasCliente),
                    new
                    {
                        id = cuota.Credito.IdCliente
                    }
                );
            }

            decimal capitalPendiente =
                Math.Round(
                    cuota.CapitalProgramado -
                    cuota.CapitalPagado,
                    2
                );

            decimal interesPendiente =
                Math.Round(
                    cuota.InteresProgramado -
                    cuota.InteresPagado,
                    2
                );

            decimal moraPendiente = 0m;

            decimal totalPendiente =
                Math.Round(
                    capitalPendiente +
                    interesPendiente +
                    moraPendiente,
                    2
                );

            var model =
                new RegistrarPagoViewModel
                {
                    IdCuota = cuota.IdCuota,

                    IdCredito = cuota.IdCredito,

                    CodigoCredito =  cuota.Credito.CodigoCredito,

                    Cliente = cuota.Credito.Cliente.Usuario.Nombres + " " + cuota.Credito.Cliente.Usuario.Apellidos,

                    NumeroCuota = cuota.NumeroCuota,

                    FechaVencimiento = cuota.FechaVencimiento,

                    CapitalPendiente = capitalPendiente,

                    InteresPendiente = interesPendiente,

                    MoraPendiente = moraPendiente,

                    TotalPagar = totalPendiente,

                    // Se carga por defecto el saldo completo, El cajero puede escribir una cantidad menor
                    MontoPago =
                        totalPendiente
                };

            await CargarFormasPago(model);

            return View(model);
        }

    
        // REGISTRAR PAGO - POST PAGO COMPLETO O PARCIAL


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(
            RegistrarPagoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarDatosCuota(model);
                await CargarFormasPago(model);

                return View(model);
            }

    
            // OBTENER USUARIO QUE REGISTRA EL PAGO
    

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


            // TRANSACCIÓN
     

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {

                // BUSCAR CUOTA


                var cuota = await _context.Cuotas
                    .Include(c => c.Credito)
                        .ThenInclude(c => c.Cliente)
                            .ThenInclude(c => c.Usuario)
                    .FirstOrDefaultAsync(c =>
                        c.IdCuota == model.IdCuota);

                if (cuota == null)
                {
                    await transaction.RollbackAsync();

                    return NotFound();
                }

 
                // VALIDAR CUOTA
   

                if (cuota.Estado == "PAGADA")
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        string.Empty,
                        "Esta cuota ya fue pagada."
                    );

                    await CargarDatosCuota(model);
                    await CargarFormasPago(model);

                    return View(model);
                }

                // VALIDAR CRÉDITO


                if (cuota.Credito.Estado == "CANCELADO")
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        string.Empty,
                        "El crédito ya está cancelado."
                    );

                    await CargarDatosCuota(model);
                    await CargarFormasPago(model);

                    return View(model);
                }

      
                // VALIDAR FORMA DE PAGO
  

                var formaPago =
                    await _context.FormasPago
                        .FirstOrDefaultAsync(f =>
                            f.IdFormaPago ==
                                model.IdFormaPago &&
                            f.Estado == "ACTIVO");

                if (formaPago == null)
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        nameof(model.IdFormaPago),
                        "La forma de pago seleccionada no es válida."
                    );

                    await CargarDatosCuota(model);
                    await CargarFormasPago(model);

                    return View(model);
                }

                // CALCULAR SALDOS REALES


                decimal capitalPendiente =
                    Math.Round(
                        cuota.CapitalProgramado -
                        cuota.CapitalPagado,
                        2
                    );

                decimal interesPendiente =
                    Math.Round(
                        cuota.InteresProgramado -
                        cuota.InteresPagado,
                        2
                    );

                // Mora aún no está habilitada
                decimal moraPendiente =
                    0m;

                decimal saldoCuota =
                    Math.Round(
                        capitalPendiente +
                        interesPendiente +
                        moraPendiente,
                        2
                    );


                // VALIDAR MONTO RECIBIDO
    

                decimal montoRecibido =
                    Math.Round(
                        model.MontoPago,
                        2
                    );

                if (montoRecibido <= 0)
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        nameof(model.MontoPago),
                        "El monto debe ser mayor que cero."
                    );

                    await CargarDatosCuota(model);
                    await CargarFormasPago(model);

                    return View(model);
                }

                if (montoRecibido > saldoCuota)
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        nameof(model.MontoPago),
                        $"El monto no puede ser mayor al saldo de la cuota: Q {saldoCuota:N2}."
                    );

                    await CargarDatosCuota(model);
                    await CargarFormasPago(model);

                    return View(model);
                }


                // DISTRIBUIR PAGO PRIMERO INTERÉS, DESPUÉS CAPITAL
        

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

                decimal moraAplicada = 0m;

      
                // CREAR REGISTRO DE PAGO


                var pago =
                    new Pago
                    {
                        IdCredito = cuota.IdCredito,

                        IdCuota = cuota.IdCuota,

                        IdUsuario = idUsuario,

                        IdFormaPago = model.IdFormaPago,

                        NumeroRecibo = $"TMP-{Guid.NewGuid().ToString("N")[..20]}",

                        TipoOperacion = "PAGO_CUOTA",

                        MontoTotal = montoRecibido,

                        CapitalAplicado = capitalAplicado,

                        InteresAplicado = interesAplicado,

                        MoraAplicada = moraAplicada,

                        FechaPago = DateTime.Now,

                        Observacion =  model.Observacion
                    };

                _context.Pagos.Add(pago);

                await _context.SaveChangesAsync();

               // GENERAR NÚMERO DE RECIBO
    

                pago.NumeroRecibo =
                    $"REC-{pago.IdPago:D6}";

                // ACTUALIZAR CUOTA


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
                    nuevoSaldoCuota = 0m;
                }

                cuota.SaldoCuota = nuevoSaldoCuota;

  
                // DEFINIR ESTADO DE CUOTA


                if (cuota.SaldoCuota <= 0.01m)
                {
                    cuota.SaldoCuota = 0m;

                    cuota.Estado = "PAGADA";
                }
                else
                {
                    cuota.Estado = "PARCIAL";
                }


                // ACTUALIZAR SALDO CAPITAL DEL CRÉDITO


                cuota.Credito.SaldoCapital =
                    Math.Round(
                        cuota.Credito.SaldoCapital -
                        capitalAplicado,
                        2
                    );

                if (cuota.Credito.SaldoCapital < 0)
                {
                    cuota.Credito.SaldoCapital =
                        0m;
                }

               // VERIFICAR CUOTAS RESTANTES


                bool existenCuotasPendientes =
                    await _context.Cuotas
                        .AnyAsync(c =>
                            c.IdCredito ==
                                cuota.IdCredito &&
                            c.IdCuota !=
                                cuota.IdCuota &&
                            c.Estado !=
                                "PAGADA");


                // CANCELAR EL CRÉDITO SI TERMINÓ


                if (
                    cuota.Estado == "PAGADA" &&
                    !existenCuotasPendientes
                )
                {
                    cuota.Credito.SaldoCapital =
                        0m;

                    cuota.Credito.Estado =
                        "CANCELADO";

                    cuota.Credito.FechaCancelacion =
                        DateTime.Today;
                }


                // GUARDAR CAMBIOS


                await _context.SaveChangesAsync();

                await transaction.CommitAsync();


                // MENSAJE
  

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

                // Regresar a las cuotas del mismo cliente.
                return RedirectToAction(
                    nameof(CuotasCliente),
                    new
                    {
                        id = cuota.Credito.IdCliente
                    }
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                Console.WriteLine(
                    ex.ToString()
                );

                ModelState.AddModelError(
                    string.Empty,
                    $"No fue posible registrar el pago: {ex.Message}"
                );

                await CargarDatosCuota(model);
                await CargarFormasPago(model);

                return View(model);
            }
        }


        // ABONO EXTRAORDINARIO - GET


        [HttpGet]
        public async Task<IActionResult> AbonoExtraordinario(
            ulong id)
        {
            var credito = await _context.Creditos
                .Include(c => c.Cliente)
                    .ThenInclude(c => c.Usuario)
                .FirstOrDefaultAsync(c =>
                    c.IdCredito == id);

            if (credito == null)
            {
                return NotFound();
            }

            if (
                credito.Estado != "ACTIVO" &&
                credito.Estado != "ATRASADO"
            )
            {
                TempData["MensajeError"] =
                    "El crédito no se encuentra activo.";

                return RedirectToAction(nameof(Index));
            }


            // NO PERMITIR ABONO SI EXISTE CUOTA PARCIAL O ATRASADA


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
                    new
                    {
                        id = credito.IdCliente
                    }
                );
            }

            var model =
                new AbonoExtraordinarioViewModel
                {
                    IdCredito =
                        credito.IdCredito,

                    CodigoCredito =
                        credito.CodigoCredito,

                    Cliente =
                        credito.Cliente.Usuario.Nombres +
                        " " +
                        credito.Cliente.Usuario.Apellidos,

                    SaldoCapital =
                        credito.SaldoCapital,

                    CuotaActual =
                        credito.CuotaOriginal,

                    PlazoOriginal =
                        credito.PlazoMeses
                };

            await CargarFormasPagoAbono(model);

            return View(model);
        }


        // ABONO EXTRAORDINARIO - POST
 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AbonoExtraordinario(
            AbonoExtraordinarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarDatosAbono(model);
                await CargarFormasPagoAbono(model);

                return View(model);
            }

            // USUARIO QUE REGISTRA EL ABONO


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

            // TRANSACCION


            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {

                // BUSCAR CREDITO


                var credito = await _context.Creditos
                    .Include(c => c.Cliente)
                        .ThenInclude(c => c.Usuario)
                    .FirstOrDefaultAsync(c =>
                        c.IdCredito ==
                            model.IdCredito);

                if (credito == null)
                {
                    await transaction.RollbackAsync();

                    return NotFound();
                }


                // VALIDAR CRÉDITO


                if (
                    credito.Estado != "ACTIVO" &&
                    credito.Estado != "ATRASADO"
                )
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        string.Empty,
                        "El crédito no está disponible para recibir abonos."
                    );

                    await CargarDatosAbono(model);
                    await CargarFormasPagoAbono(model);

                    return View(model);
                }

                // VALIDAR CUOTAS PARCIALES O ATRASADAS


                bool cuotaIrregular =
                    await _context.Cuotas
                        .AnyAsync(c =>
                            c.IdCredito ==
                                credito.IdCredito &&
                            (
                                c.Estado == "PARCIAL" ||
                                c.Estado == "ATRASADA"
                            ));

                if (cuotaIrregular)
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        string.Empty,
                        "Primero debe completar las cuotas parciales o atrasadas."
                    );

                    await CargarDatosAbono(model);
                    await CargarFormasPagoAbono(model);

                    return View(model);
                }

                // VALIDAR FORMA DE PAGO


                var formaPago =
                    await _context.FormasPago
                        .FirstOrDefaultAsync(f =>
                            f.IdFormaPago ==
                                model.IdFormaPago &&
                            f.Estado == "ACTIVO");

                if (formaPago == null)
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        nameof(model.IdFormaPago),
                        "La forma de pago seleccionada no es válida."
                    );

                    await CargarDatosAbono(model);
                    await CargarFormasPagoAbono(model);

                    return View(model);
                }

                // VALIDAR MONTO DEL ABONO


                decimal montoAbono =
                    Math.Round(
                        model.MontoAbono,
                        2
                    );

                if (montoAbono <= 0)
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        nameof(model.MontoAbono),
                        "El monto debe ser mayor que cero."
                    );

                    await CargarDatosAbono(model);
                    await CargarFormasPagoAbono(model);

                    return View(model);
                }

                if (montoAbono > credito.SaldoCapital)
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        nameof(model.MontoAbono),
                        $"El abono no puede superar el saldo de capital: Q {credito.SaldoCapital:N2}."
                    );

                    await CargarDatosAbono(model);
                    await CargarFormasPagoAbono(model);

                    return View(model);
                }

                // REGISTRAR ABONO EN PAGOS


                var pago =
                    new Pago
                    {
                        IdCredito =
                            credito.IdCredito,

                        // Un abono extraordinario no pertenece a una cuota específica
                        IdCuota =
                            null,

                        IdUsuario =
                            idUsuario,

                        IdFormaPago =
                            model.IdFormaPago,

                        NumeroRecibo =
                            $"TMP-{Guid.NewGuid().ToString("N")[..20]}",

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
                            model.Observacion
                    };

                _context.Pagos.Add(pago);

                await _context.SaveChangesAsync();

                pago.NumeroRecibo =
                    $"REC-{pago.IdPago:D6}";


                // REDUCIR SALDO DE CAPITAL


                credito.SaldoCapital =
                    Math.Round(
                        credito.SaldoCapital -
                        montoAbono,
                        2
                    );

                // SI EL ABONO CANCELA TODO EL CAPITAL
      

                if (credito.SaldoCapital <= 0.01m)
                {
                    credito.SaldoCapital =
                        0m;

                    credito.Estado =
                        "CANCELADO";

                    credito.FechaCancelacion =
                        DateTime.Today;

                    // Solo quitamos cuotas futuras pendientes
                    // Las cuotas pagadas conservan su historial
                    var cuotasPendientes =
                        await _context.Cuotas
                            .Where(c =>
                                c.IdCredito ==
                                    credito.IdCredito &&
                                c.Estado ==
                                    "PENDIENTE")
                            .ToListAsync();

                    _context.Cuotas.RemoveRange(
                        cuotasPendientes
                    );

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    TempData["MensajeExito"] =
                        $"Abono registrado correctamente. " +
                        $"Recibo: {pago.NumeroRecibo}. " +
                        $"El crédito {credito.CodigoCredito} fue cancelado.";

                    return RedirectToAction(
                        nameof(Index)
                    );
                }


                // CALCULAR TASA MENSUAL
   
                decimal tasaMensual;

                if (credito.TipoTasaAplicada == "ANUAL")
                {
                    tasaMensual =
                        credito.PorcentajeInteresAplicado /
                        12m /
                        100m;
                }
                else
                {
                    tasaMensual =
                        credito.PorcentajeInteresAplicado /
                        100m;
                }

                // OBTENER CUOTAS FUTURAS


                var cuotasFuturas =
                    await _context.Cuotas
                        .Where(c =>
                            c.IdCredito ==
                                credito.IdCredito &&
                            c.Estado ==
                                "PENDIENTE")
                        .OrderBy(c =>
                            c.NumeroCuota)
                        .ToListAsync();

                if (!cuotasFuturas.Any())
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        string.Empty,
                        "No existen cuotas futuras para recalcular."
                    );

                    await CargarDatosAbono(model);
                    await CargarFormasPagoAbono(model);

                    return View(model);
                }


                // MANTENER APROXIMADAMENTE LA MISMA CUOTA Y REDUCIR EL PLAZO


                decimal cuotaObjetivo =
                    credito.CuotaOriginal;

                decimal saldoTemporal =
                    credito.SaldoCapital;

                int cuotasNecesarias =
                    0;


                // CALCULAR CUÁNTAS CUOTAS SE NECESITAN

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
                        throw new Exception(
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

                    // Protección ante un cálculo inesperado.
                    if (cuotasNecesarias > 600)
                    {
                        throw new Exception(
                            "No fue posible recalcular el nuevo plazo."
                        );
                    }
                }

                if (
                    cuotasNecesarias >
                    cuotasFuturas.Count
                )
                {
                    throw new Exception(
                        "El nuevo calendario requiere más cuotas que las disponibles."
                    );
                }

                // RECALCULAR CUOTAS RESTANTES
        

                decimal saldoRestante =
                    credito.SaldoCapital;

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

                    // En la última cuota ajustamos centavos.
                    if (
                        i == cuotasNecesarias - 1 ||
                        capital > saldoRestante
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

                    cuota.SaldoCuota = totalCuota;

                    cuota.Estado =  "PENDIENTE";

                    saldoRestante = Math.Round(saldoRestante - capital, 2);
                }

                // GUARDAR LA NUEVA FECHA FINAL

                var ultimaCuotaNecesaria =
                    cuotasFuturas[
                        cuotasNecesarias - 1
                    ];

                credito.FechaFinalizacionEstimada = ultimaCuotaNecesaria.FechaVencimiento;

                // ELIMINAR CUOTAS FUTURAS SOBRANTES


                if (
                    cuotasNecesarias <cuotasFuturas.Count
                )
                {
                    var cuotasSobrantes =
                        cuotasFuturas
                            .Skip(cuotasNecesarias)
                            .ToList();

                    _context.Cuotas.RemoveRange(
                        cuotasSobrantes
                    );
                }

                // GUARDAR TODO


                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["MensajeExito"] =
                    $"Abono extraordinario registrado. " +
                    $"Recibo: {pago.NumeroRecibo} - " +
                    $"Abono: Q {montoAbono:N2} - " +
                    $"Nuevo saldo: Q {credito.SaldoCapital:N2} - " +
                    $"Cuotas restantes: {cuotasNecesarias}.";

                return RedirectToAction(
                    nameof(CuotasCliente),
                    new
                    {
                        id = credito.IdCliente
                    }
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

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

        // CARGAR FORMAS DE PAGO PARA CUOTAS


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
                            Value =  f.IdFormaPago.ToString(),

                            Text = f.Nombre
                        })
                    .ToListAsync();
        }

        // RECARGAR DATOS DE UNA CUOTA

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
            {
                return;
            }

            model.IdCredito = cuota.IdCredito;

            model.CodigoCredito = cuota.Credito.CodigoCredito;

            model.Cliente = cuota.Credito.Cliente.Usuario.Nombres + " " +
                cuota.Credito.Cliente.Usuario.Apellidos;

            model.NumeroCuota = cuota.NumeroCuota;

            model.FechaVencimiento = cuota.FechaVencimiento;

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

            model.MoraPendiente = 0m;

            model.TotalPagar =
                Math.Round(
                    model.CapitalPendiente +
                    model.InteresPendiente +
                    model.MoraPendiente,
                    2
                );
        }

        // FORMAS DE PAGO PARA ABONO EXTRAORDINARIO

        private async Task CargarFormasPagoAbono(
            AbonoExtraordinarioViewModel model)
        {
            model.FormasPago = await _context.FormasPago.Where(f =>f.Estado == "ACTIVO").OrderBy(f => f.Nombre)
                    .Select(f =>
                        new SelectListItem
                        {
                            Value = f.IdFormaPago.ToString(),

                            Text = f.Nombre
                        })
                    .ToListAsync();
        }

        // RECARGAR DATOS DEL CRÉDITO PARA ABONO

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
            {
                return;
            }

            model.CodigoCredito = credito.CodigoCredito;

            model.Cliente = credito.Cliente.Usuario.Nombres + " " + credito.Cliente.Usuario.Apellidos;

            model.SaldoCapital = credito.SaldoCapital;

            model.CuotaActual = credito.CuotaOriginal;

            model.PlazoOriginal = credito.PlazoMeses;
        }

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
                    p.Credito.Cliente.Usuario.Nombres.Contains(textoCliente) ||
                    p.Credito.Cliente.Usuario.Apellidos.Contains(textoCliente) ||
                    p.Credito.Cliente.CodigoCliente.Contains(textoCliente)
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
                    p.Credito.CodigoCredito.Contains(textoCredito)
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



            ViewBag.Cliente =
                cliente;

            ViewBag.Recibo =
                recibo;

            ViewBag.Credito =
                credito;

            ViewBag.TipoOperacion =
                tipoOperacion;

            return View(pagos);
        }
    }
}
