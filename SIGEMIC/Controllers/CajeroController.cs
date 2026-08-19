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
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalClientes = await _context.Clientes.CountAsync();
            ViewBag.CreditosActivos = await _context.Creditos.CountAsync(c => c.Estado == "ACTIVO");
            ViewBag.CuotasPendientes = await _context.Cuotas.CountAsync(c =>c.Estado == "PENDIENTE" || c.Estado == "ATRASADA");
            ViewBag.PagosHoy = await _context.Pagos.Where(p =>p.FechaPago.Date == DateTime.Today).SumAsync(p => (decimal?)p.MontoTotal) ?? 0;

            return View();
        }
    }
}