using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Models;

namespace SIGEMIC.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR,CAJERO")]
    public class CreditosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CreditosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LISTADO DE CRÉDITOS


        public async Task<IActionResult> Index()
        {
            var creditos = await _context.Creditos.Include(c => c.Cliente).ThenInclude(c => c.Usuario).Include(c => c.PlanCredito)
                .OrderByDescending(c => c.FechaOtorgamiento)
                .ToListAsync();

            return View(creditos);
        }

        // NUEVO CRÉDITO GET

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CrearCreditoViewModel();

            await CargarCombos(model);

            return View(model);
        }

        // NUEVO CRÉDITO POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearCreditoViewModel model)
        {

            // VALIDACIONES DEL FORMULARIO

            if (!ModelState.IsValid)
            {
                await CargarCombos(model);

                return View(model);
            }

            // VALIDAR CLIENTE

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c =>c.IdCliente == model.IdCliente &&
                    c.Estado == "ACTIVO");

            if (cliente == null)
            {
                ModelState.AddModelError(nameof(model.IdCliente),"El cliente seleccionado no existe o está inactivo.");
                await CargarCombos(model);
                return View(model);
            }

            // VALIDAR PLAN

            var plan = await _context.PlanesCredito.FirstOrDefaultAsync(p =>p.IdPlan == model.IdPlan &&
                p.Estado == "ACTIVO");

            if (plan == null)
            {
                ModelState.AddModelError(nameof(model.IdPlan),"El plan seleccionado no existe o está inactivo.");
                await CargarCombos(model);
                return View(model);
            }

            // VALIDAR PLAZO

            uint[] plazosPermitidos =
            {
                6, 12, 18, 24, 36, 48, 60
            };

            if (!plazosPermitidos.Contains(model.PlazoMeses))
            {
                ModelState.AddModelError(nameof(model.PlazoMeses),"El plazo seleccionado no es válido.");
                await CargarCombos(model);
                return View(model);
            }

            // CALCULAR TASA MENSUAL

            decimal tasaMensual;

            if (plan.TipoTasa == "ANUAL")
            {
                tasaMensual = plan.PorcentajeInteres / 12m / 100m;
            }
            else
            {
                tasaMensual = plan.PorcentajeInteres / 100m;
            }


            // CALCULAR CUOTA FIJA SISTEMA FRANCES

            double monto = Convert.ToDouble(model.MontoOriginal);

            double tasa = Convert.ToDouble(tasaMensual);

            int meses = Convert.ToInt32(model.PlazoMeses);

            double cuotaCalculada;

            if (tasa == 0)
            {
                cuotaCalculada = monto / meses;
            }
            else
            {
                double factor = Math.Pow(1 + tasa, meses);

                cuotaCalculada = monto *
                    (
                        tasa * factor / (factor - 1)
                    );
            }

            decimal cuota = Math.Round( Convert.ToDecimal(cuotaCalculada), 2);

            // TRANSACCIÓN


            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                // CREAR CRÉDITO

                var credito = new Credito
                {
                    CodigoCredito = $"TMP-{Guid.NewGuid().ToString("N")[..20]}",

                    IdCliente = model.IdCliente,

                    IdPlan = model.IdPlan,

                    MontoOriginal = model.MontoOriginal,

                    TipoTasaAplicada = plan.TipoTasa,

                    PorcentajeInteresAplicado = plan.PorcentajeInteres,

                    PlazoMeses = model.PlazoMeses,

                    CuotaOriginal = cuota,

                    SaldoCapital = model.MontoOriginal,

                    Estado = "ACTIVO",

                    FechaOtorgamiento = DateTime.Today,

                    FechaInicio = DateTime.Today,

                    FechaFinalizacionEstimada = DateTime.Today.AddMonths( (int)model.PlazoMeses),

                    FechaCancelacion = null
                };

                _context.Creditos.Add(credito);

                await _context.SaveChangesAsync();

                credito.CodigoCredito = $"CR-{credito.IdCredito:D6}";

                await _context.SaveChangesAsync();

                // GENERAR CUOTAS

                decimal saldoRestante =  credito.MontoOriginal;

                for (
                    int numero = 1;
                    numero <= meses;
                    numero++
                )
                {

                    // INTERÉS DEL MES

                    decimal interesMes = Math.Round(saldoRestante * tasaMensual, 2);

                    // CAPITAL DEL MES

                    decimal capitalMes = Math.Round(credito.CuotaOriginal - interesMes, 2);

                    // AJUSTE DE ÚLTIMA CUOTA

                    if (numero == meses)
                    {
                        capitalMes = saldoRestante;
                    }

                    // CUOTA DEL MES

                    decimal cuotaMes = Math.Round(capitalMes + interesMes, 2);

                    // CREAR REGISTRO DE CUOTA

                    var nuevaCuota = new Cuota
                    {
                        IdCredito = credito.IdCredito,

                        NumeroCuota = (uint)numero, FechaVencimiento = credito.FechaInicio.AddMonths(numero),
                        CapitalProgramado = capitalMes, InteresProgramado = interesMes, CuotaProgramada = cuotaMes,
                        CapitalPagado = 0, InteresPagado = 0, MoraPagada = 0, SaldoCuota = cuotaMes, Estado = "PENDIENTE"
                    };

                    _context.Cuotas.Add(nuevaCuota);

                    // ACTUALIZAR SALDO PARA EL SIGUIENTE MES

                    saldoRestante =Math.Round(saldoRestante - capitalMes, 2);
                }

                // Guardar todas las cuotas.
                await _context.SaveChangesAsync();

                // Confirmar toda la operación.
                await transaction.CommitAsync();

                TempData["MensajeExito"] =
                    $"Crédito {credito.CodigoCredito} " +
                    $"otorgado correctamente. " +
                    $"Cuota mensual: Q {credito.CuotaOriginal:N2}";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                Console.WriteLine(ex.ToString());

                ModelState.AddModelError(string.Empty,$"No fue posible registrar el crédito: {ex.Message}");
                await CargarCombos(model);
                return View(model);
            }
        }

        // CARGAR CLIENTES Y PLANES

        private async Task CargarCombos(CrearCreditoViewModel model)
        {

            // CLIENTES ACTIVOS

            model.Clientes = await _context.Clientes.Include(c => c.Usuario).Where(c =>c.Estado == "ACTIVO").OrderBy(c =>c.Usuario.Nombres)
                .Select(c =>new SelectListItem
                    {
                        Value = c.IdCliente.ToString(), Text = c.CodigoCliente + " - " + c.Usuario.Nombres + " " + c.Usuario.Apellidos
                    })
                .ToListAsync();

            // PLANES ACTIVOS

            model.Planes = await _context.PlanesCredito.Where(p =>p.Estado == "ACTIVO").OrderBy(p =>p.Nombre).Select(p =>
                    new SelectListItem
                    {
                        Value = p.IdPlan.ToString(),Text = p.Nombre + " - " + p.PorcentajeInteres + "% " + p.TipoTasa
                    }).ToListAsync();
        }
    }
}