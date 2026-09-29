using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;

namespace SIGEMIC.Controllers
{
    [Authorize(Roles = "CAJERO")]
    public class CajeroController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CajeroController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // DASHBOARD CAJERO
        // =====================================================

        public async Task<IActionResult> Index()
        {
            // =================================================
            // CLIENTES
            // =================================================

            ViewBag.TotalClientes =
                await _context.Clientes
                    .CountAsync();


            // =================================================
            // CRÉDITOS ACTIVOS
            // =================================================

            ViewBag.CreditosActivos =
                await _context.Creditos
                    .CountAsync(c =>
                        c.Estado == "ACTIVO"
                    );


            // =================================================
            // CUOTAS PENDIENTES O ATRASADAS
            // =================================================

            ViewBag.CuotasPendientes =
                await _context.Cuotas
                    .CountAsync(c =>
                        c.Estado == "PENDIENTE" ||
                        c.Estado == "ATRASADA"
                    );


            // =================================================
            // PAGOS DE HOY
            // =================================================

            ViewBag.PagosHoy =
                await _context.Pagos
                    .Where(p =>
                        p.FechaPago.Date ==
                        DateTime.Today
                    )
                    .SumAsync(p =>
                        (decimal?)p.MontoTotal
                    )
                ?? 0m;


            // =================================================
            // SOLICITUDES DE PAGO PENDIENTES
            // =================================================

            ViewBag.SolicitudesPendientes =
                await _context.SolicitudesPago
                    .CountAsync(s =>
                        s.Estado == "PENDIENTE"
                    );


            return View();
        }
    }
}