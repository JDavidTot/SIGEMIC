using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Services;

namespace SIGEMIC.Controllers
{
    [Authorize(Roles = "CAJERO")]
    public class CajeroController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PermisoService _permisoService;

        public CajeroController(
            ApplicationDbContext context,
            PermisoService permisoService)
        {
            _context = context;
            _permisoService = permisoService;
        }

        public async Task<IActionResult> Index()
        {
            var idTexto = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!uint.TryParse(idTexto, out uint idUsuario))
                return Unauthorized();

            var permisos = (await _permisoService
                .ObtenerPermisosAsync(idUsuario))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            ViewData["PermisosCajero"] = permisos.ToList();

            ViewBag.TotalClientes = 0;
            ViewBag.CreditosActivos = 0;
            ViewBag.CuotasPendientes = 0;
            ViewBag.PagosHoy = 0m;
            ViewBag.SolicitudesPendientes = 0;

            if (permisos.Contains("CLIENTES_VER"))
            {
                ViewBag.TotalClientes =
                    await _context.Clientes.CountAsync();
            }

            if (permisos.Contains("CREDITOS_VER"))
            {
                ViewBag.CreditosActivos =
                    await _context.Creditos
                        .CountAsync(c =>
                            c.Estado == "ACTIVO");
            }

            if (permisos.Contains("PAGOS_REGISTRAR"))
            {
                ViewBag.CuotasPendientes =
                    await _context.Cuotas
                        .CountAsync(c =>
                            c.Estado == "PENDIENTE" ||
                            c.Estado == "ATRASADA");
            }

            if (permisos.Contains("PAGOS_HISTORIAL"))
            {
                ViewBag.PagosHoy =
                    await _context.Pagos
                        .Where(p =>
                            p.FechaPago.Date == DateTime.Today)
                        .SumAsync(p =>
                            (decimal?)p.MontoTotal)
                    ?? 0m;
            }

            if (permisos.Contains("SOLICITUDES_VER"))
            {
                ViewBag.SolicitudesPendientes =
                    await _context.SolicitudesPago
                        .CountAsync(s =>
                            s.Estado == "PENDIENTE");
            }

            return View();
        }
    }
}