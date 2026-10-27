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
    public class CreditosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditoriaService _auditoriaService;
        private readonly ILogger<CreditosController> _logger;

        public CreditosController(
            ApplicationDbContext context,
            AuditoriaService auditoriaService,
            ILogger<CreditosController> logger)
        {
            _context = context;
            _auditoriaService = auditoriaService;
            _logger = logger;
        }

        [PermisoAuthorize("CREDITOS_VER")]
        public async Task<IActionResult> Index()
        {
            var creditos = await _context.Creditos
                .Include(c => c.Cliente)
                    .ThenInclude(c => c.Usuario)
                .Include(c => c.PlanCredito)
                .OrderByDescending(c => c.FechaOtorgamiento)
                .ThenByDescending(c => c.IdCredito)
                .ToListAsync();

            return View(creditos);
        }

        [HttpGet]
        [PermisoAuthorize("CREDITOS_DETALLE")]
        public async Task<IActionResult> Details(ulong id)
        {
            var credito = await _context.Creditos
                .Include(c => c.Cliente)
                    .ThenInclude(c => c.Usuario)
                .Include(c => c.PlanCredito)
                .Include(c => c.Cuotas)
                .FirstOrDefaultAsync(c =>
                    c.IdCredito == id
                );

            if (credito == null)
            {
                return NotFound();
            }

            return View(credito);
        }

        [HttpGet]
        [PermisoAuthorize("CREDITOS_CREAR")]
        public async Task<IActionResult> Create()
        {
            var model =
                new CrearCreditoViewModel();

            await CargarCombos(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermisoAuthorize("CREDITOS_CREAR")]
        public async Task<IActionResult> Create(
            CrearCreditoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarCombos(model);

                return View(model);
            }

            // ==========================================
            // VALIDAR CLIENTE
            // ==========================================

            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c =>
                    c.IdCliente == model.IdCliente &&
                    c.Estado == "ACTIVO"
                );

            if (cliente == null)
            {
                ModelState.AddModelError(
                    nameof(model.IdCliente),
                    "El cliente seleccionado no existe o está inactivo."
                );

                await CargarCombos(model);

                return View(model);
            }

            // ==========================================
            // VALIDAR PLAN
            // ==========================================

            var plan = await _context.PlanesCredito
                .FirstOrDefaultAsync(p =>
                    p.IdPlan == model.IdPlan &&
                    p.Estado == "ACTIVO"
                );

            if (plan == null)
            {
                ModelState.AddModelError(
                    nameof(model.IdPlan),
                    "El plan seleccionado no existe o está inactivo."
                );

                await CargarCombos(model);

                return View(model);
            }

            // ==========================================
            // VALIDAR PLAZO
            // ==========================================

            uint[] plazosPermitidos =
            {
                6,
                12,
                18,
                24,
                36,
                48,
                60
            };

            if (
                !plazosPermitidos.Contains(
                    model.PlazoMeses
                )
            )
            {
                ModelState.AddModelError(
                    nameof(model.PlazoMeses),
                    "El plazo seleccionado no es válido."
                );

                await CargarCombos(model);

                return View(model);
            }

            // ==========================================
            // CALCULAR TASA MENSUAL
            // ==========================================

            decimal tasaMensual;

            if (plan.TipoTasa == "ANUAL")
            {
                tasaMensual =
                    plan.PorcentajeInteres /
                    12m /
                    100m;
            }
            else
            {
                tasaMensual =
                    plan.PorcentajeInteres /
                    100m;
            }

            double monto =
                Convert.ToDouble(
                    model.MontoOriginal
                );

            double tasa =
                Convert.ToDouble(
                    tasaMensual
                );

            int meses =
                Convert.ToInt32(
                    model.PlazoMeses
                );

            double cuotaCalculada;

            // ==========================================
            // MÉTODO FRANCÉS
            // ==========================================

            if (tasa == 0)
            {
                cuotaCalculada =
                    monto / meses;
            }
            else
            {
                double factor =
                    Math.Pow(
                        1 + tasa,
                        meses
                    );

                cuotaCalculada =
                    monto *
                    (
                        tasa * factor /
                        (factor - 1)
                    );
            }

            decimal cuota =
                Math.Round(
                    Convert.ToDecimal(
                        cuotaCalculada
                    ),
                    2
                );

            // ==========================================
            // TRANSACCIÓN
            // ==========================================

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                // ======================================
                // CREAR CRÉDITO
                // ======================================

                var credito =
                    new Credito
                    {
                        CodigoCredito =
                            $"TMP-{Guid.NewGuid().ToString("N")[..20]}",

                        IdCliente =
                            model.IdCliente,

                        IdPlan =
                            model.IdPlan,

                        MontoOriginal =
                            model.MontoOriginal,

                        TipoTasaAplicada =
                            plan.TipoTasa,

                        PorcentajeInteresAplicado =
                            plan.PorcentajeInteres,

                        PlazoMeses =
                            model.PlazoMeses,

                        CuotaOriginal =
                            cuota,

                        SaldoCapital =
                            model.MontoOriginal,

                        Estado =
                            "ACTIVO",

                        FechaOtorgamiento =
                            DateTime.Today,

                        FechaInicio =
                            DateTime.Today,

                        FechaFinalizacionEstimada =
                            DateTime.Today.AddMonths(
                                (int)model.PlazoMeses
                            ),

                        FechaCancelacion =
                            null
                    };

                _context.Creditos.Add(
                    credito
                );

                await _context.SaveChangesAsync();

                // ======================================
                // GENERAR CÓDIGO DEL CRÉDITO
                // ======================================

                credito.CodigoCredito =
                    $"CR-{credito.IdCredito:D6}";

                await _context.SaveChangesAsync();

                // ======================================
                // GENERAR TABLA DE AMORTIZACIÓN
                // ======================================

                decimal saldoRestante =
                    credito.MontoOriginal;

                for (
                    int numero = 1;
                    numero <= meses;
                    numero++
                )
                {
                    decimal interesMes =
                        Math.Round(
                            saldoRestante *
                            tasaMensual,
                            2
                        );

                    decimal capitalMes =
                        Math.Round(
                            credito.CuotaOriginal -
                            interesMes,
                            2
                        );

                    // ==================================
                    // AJUSTAR ÚLTIMA CUOTA
                    // ==================================

                    if (numero == meses)
                    {
                        capitalMes =
                            saldoRestante;
                    }

                    decimal cuotaMes =
                        Math.Round(
                            capitalMes +
                            interesMes,
                            2
                        );

                    var nuevaCuota =
                        new Cuota
                        {
                            IdCredito =
                                credito.IdCredito,

                            NumeroCuota =
                                (uint)numero,

                            FechaVencimiento =
                                credito.FechaInicio
                                    .AddMonths(numero),

                            CapitalProgramado =
                                capitalMes,

                            InteresProgramado =
                                interesMes,

                            CuotaProgramada =
                                cuotaMes,

                            CapitalPagado =
                                0m,

                            InteresPagado =
                                0m,

                            MoraPagada =
                                0m,

                            SaldoCuota =
                                cuotaMes,

                            Estado =
                                "PENDIENTE"
                        };

                    _context.Cuotas.Add(
                        nuevaCuota
                    );

                    saldoRestante =
                        Math.Round(
                            saldoRestante -
                            capitalMes,
                            2
                        );
                }

                await _context.SaveChangesAsync();

                // ======================================
                // CONFIRMAR OPERACIÓN
                // ======================================

                await transaction.CommitAsync();

                // ======================================
                // AUDITORÍA
                // Se ejecuta después del COMMIT.
                // ======================================

                string nombreCliente =
                    $"{cliente.Usuario.Nombres} " +
                    $"{cliente.Usuario.Apellidos}";

                string descripcion =
                    $"Crédito {credito.CodigoCredito} creado. " +
                    $"Cliente: {cliente.CodigoCliente} - {nombreCliente}; " +
                    $"Plan: {plan.Nombre}; " +
                    $"Monto: Q{credito.MontoOriginal:N2}; " +
                    $"Tasa: {credito.PorcentajeInteresAplicado:N4}% " +
                    $"{credito.TipoTasaAplicada}; " +
                    $"Plazo: {credito.PlazoMeses} meses; " +
                    $"Cuota inicial: Q{credito.CuotaOriginal:N2}; " +
                    $"Saldo inicial: Q{credito.SaldoCapital:N2}; " +
                    $"Estado: {credito.Estado}.";

                await RegistrarAuditoriaSeguroAsync(
                    "CREDITO_CREADO",
                    "creditos",
                    credito.IdCredito.ToString(),
                    descripcion
                );

                // ======================================
                // MENSAJE
                // ======================================

                TempData["MensajeExito"] =
                    $"Crédito {credito.CodigoCredito} " +
                    $"otorgado correctamente. " +
                    $"Cuota mensual: Q {credito.CuotaOriginal:N2}";

                return RedirectToAction(
                    nameof(Index)
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error al crear un crédito."
                );

                ModelState.AddModelError(
                    string.Empty,
                    $"No fue posible registrar el crédito: {ex.Message}"
                );

                await CargarCombos(model);

                return View(model);
            }
        }

        // ==============================================
        // CARGAR COMBOS
        // ==============================================

        private async Task CargarCombos(
            CrearCreditoViewModel model)
        {
            model.Clientes =
                await _context.Clientes
                    .Include(c => c.Usuario)
                    .Where(c =>
                        c.Estado == "ACTIVO"
                    )
                    .OrderBy(c =>
                        c.Usuario.Nombres
                    )
                    .ThenBy(c =>
                        c.Usuario.Apellidos
                    )
                    .Select(c =>
                        new SelectListItem
                        {
                            Value =
                                c.IdCliente.ToString(),

                            Text =
                                c.CodigoCliente +
                                " - " +
                                c.Usuario.Nombres +
                                " " +
                                c.Usuario.Apellidos
                        }
                    )
                    .ToListAsync();

            model.Planes =
                await _context.PlanesCredito
                    .Where(p =>
                        p.Estado == "ACTIVO"
                    )
                    .OrderBy(p =>
                        p.Nombre
                    )
                    .Select(p =>
                        new SelectListItem
                        {
                            Value =
                                p.IdPlan.ToString(),

                            Text =
                                p.Nombre +
                                " - " +
                                p.PorcentajeInteres +
                                "% " +
                                p.TipoTasa
                        }
                    )
                    .ToListAsync();
        }

        // ==============================================
        // AUDITORÍA SEGURA
        // ==============================================

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
                    "No fue posible registrar la auditoría de la acción {Accion}.",
                    accion
                );
            }
        }
    }
}