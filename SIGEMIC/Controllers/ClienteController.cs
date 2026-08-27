using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Models;

namespace SIGEMIC.Controllers
{
    [Authorize(Roles = "CLIENTE")]
    public class ClienteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClienteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DASHBOARD DEL CLIENTE

        public async Task<IActionResult> Index()
        {
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

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.IdUsuario == idUsuario);

            if (cliente == null)
            {
                return NotFound();
            }

            var credito = await _context.Creditos
                .Where(c =>
                    c.IdCliente == cliente.IdCliente &&
                    (
                        c.Estado == "ACTIVO" ||
                        c.Estado == "ATRASADO"
                    ))
                .OrderByDescending(c =>
                    c.FechaOtorgamiento)
                .FirstOrDefaultAsync();

            ViewBag.CodigoCliente = cliente.CodigoCliente;

            if (credito != null)
            {
                ViewBag.TieneCredito = true;

                ViewBag.CodigoCredito = credito.CodigoCredito;

                ViewBag.MontoOriginal = credito.MontoOriginal;

                ViewBag.SaldoCapital = credito.SaldoCapital;

                ViewBag.CuotaOriginal = credito.CuotaOriginal;

                ViewBag.EstadoCredito = credito.Estado;

                ViewBag.CuotasPendientes =
                    await _context.Cuotas
                        .CountAsync(c =>
                            c.IdCredito == credito.IdCredito &&
                            c.Estado != "PAGADA");

                ViewBag.TotalPagado =
                    await _context.Pagos
                        .Where(p =>
                            p.IdCredito == credito.IdCredito)
                        .SumAsync(p =>
                            (decimal?)p.MontoTotal)
                        ?? 0m;
            }
            else
            {
                ViewBag.TieneCredito = false;
                ViewBag.CodigoCredito = null;
                ViewBag.MontoOriginal = 0m;
                ViewBag.SaldoCapital = 0m;
                ViewBag.CuotaOriginal = 0m;
                ViewBag.EstadoCredito = "SIN CRÉDITO";
                ViewBag.CuotasPendientes = 0;
                ViewBag.TotalPagado = 0m;
            }

            return View();
        }

        // MIS CUOTAS

        public async Task<IActionResult> Cuotas()
        {
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

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.IdUsuario == idUsuario);

            if (cliente == null)
            {
                return NotFound();
            }

            var credito = await _context.Creditos
                .Where(c =>
                    c.IdCliente == cliente.IdCliente &&
                    (
                        c.Estado == "ACTIVO" ||
                        c.Estado == "ATRASADO"
                    ))
                .OrderByDescending(c =>
                    c.FechaOtorgamiento)
                .FirstOrDefaultAsync();

            if (credito == null)
            {
                TempData["MensajeInfo"] =
                    "Actualmente no tienes un crédito activo.";

                return RedirectToAction(
                    nameof(Index)
                );
            }

            var cuotas = await _context.Cuotas
                .Where(c =>
                    c.IdCredito == credito.IdCredito)
                .OrderBy(c =>
                    c.NumeroCuota)
                .ToListAsync();

            ViewBag.CodigoCredito = credito.CodigoCredito;

            ViewBag.CuotaOriginal = credito.CuotaOriginal;

            ViewBag.MontoOriginal = credito.MontoOriginal;

            ViewBag.SaldoCapital = credito.SaldoCapital;

            ViewBag.EstadoCredito = credito.Estado;

            return View(cuotas);
        }

        // ESTADO DE CUENTA / HISTORIAL DE PAGOS
  
        public async Task<IActionResult> EstadoCuenta()
        {
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

            // BUSCAR CLIENTE

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.IdUsuario == idUsuario);

            if (cliente == null)
            {
                return NotFound();
            }


            // OBTENER EL CRÉDITO MÁS RECIENTE

            var credito = await _context.Creditos
                .Where(c =>
                    c.IdCliente == cliente.IdCliente)
                .OrderByDescending(c =>
                    c.FechaOtorgamiento)
                .ThenByDescending(c =>
                    c.IdCredito)
                .FirstOrDefaultAsync();

            if (credito == null)
            {
                TempData["MensajeInfo"] =
                    "No tienes créditos registrados.";

                return RedirectToAction(
                    nameof(Index)
                );
            }

            // OBTENER PAGOS DEL CRÉDITO

            var pagos = await _context.Pagos.Include(p => p.Cuota).Include(p => p.FormaPago)
                .Where(p =>
                    p.IdCredito == credito.IdCredito)
                .OrderByDescending(p =>
                    p.FechaPago)
                .ThenByDescending(p =>
                    p.IdPago)
                .Select(p =>
                    new PagoHistorialViewModel
                    {
                        IdPago = p.IdPago,

                        NumeroRecibo = p.NumeroRecibo,

                        FechaPago = p.FechaPago,

                        TipoOperacion = p.TipoOperacion,

                        NumeroCuota = p.Cuota != null? p.Cuota.NumeroCuota: null,

                        FormaPago = p.FormaPago.Nombre,

                        MontoTotal = p.MontoTotal,

                        CapitalAplicado = p.CapitalAplicado,

                        InteresAplicado = p.InteresAplicado,

                        MoraAplicada = p.MoraAplicada,

                        Observacion = p.Observacion
                    })
                .ToListAsync();

            // CREAR MODELO DEL ESTADO DE CUENTA

            var model =
                new EstadoCuentaViewModel
                {
                    CodigoCliente = cliente.CodigoCliente,

                    CodigoCredito = credito.CodigoCredito,

                    MontoOriginal = credito.MontoOriginal,

                    SaldoCapital = credito.SaldoCapital,

                    CuotaOriginal = credito.CuotaOriginal,

                    EstadoCredito = credito.Estado,

                    TotalPagado = pagos.Sum(p => p.MontoTotal),

                    TotalCapitalPagado = pagos.Sum(p => p.CapitalAplicado),

                    TotalInteresPagado = pagos.Sum(p => p.InteresAplicado),

                    TotalAbonosExtraordinarios = pagos.Where(p => p.TipoOperacion == "ABONO_EXTRAORDINARIO").Sum(p =>p.MontoTotal),Pagos = pagos
                };

            return View(model);
        }
    }
}