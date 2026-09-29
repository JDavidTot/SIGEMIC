using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;

namespace SIGEMIC.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // DASHBOARD ADMINISTRADOR
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
            // CRÉDITOS ATRASADOS
            // =================================================

            ViewBag.CreditosAtrasados =
                await _context.Creditos
                    .CountAsync(c =>
                        c.Estado == "ATRASADO"
                    );


            // =================================================
            // TOTAL PRESTADO
            // =================================================

            ViewBag.TotalPrestado =
                await _context.Creditos
                    .SumAsync(c =>
                        (decimal?)c.MontoOriginal
                    )
                ?? 0m;


            // =================================================
            // SALDO PENDIENTE
            // =================================================

            ViewBag.SaldoPendiente =
                await _context.Creditos
                    .Where(c =>
                        c.Estado == "ACTIVO" ||
                        c.Estado == "ATRASADO"
                    )
                    .SumAsync(c =>
                        (decimal?)c.SaldoCapital
                    )
                ?? 0m;


            // =================================================
            // TOTAL RECIBIDO
            // =================================================

            ViewBag.TotalPagos =
                await _context.Pagos
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