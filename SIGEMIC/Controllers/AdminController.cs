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

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalClientes = await _context.Clientes.CountAsync();
            ViewBag.CreditosActivos = await _context.Creditos.CountAsync(c => c.Estado == "ACTIVO");
            ViewBag.CreditosAtrasados = await _context.Creditos.CountAsync(c => c.Estado == "ATRASADO");
            ViewBag.TotalPrestado = await _context.Creditos.SumAsync(c => (decimal?)c.MontoOriginal) ?? 0;
            ViewBag.SaldoPendiente = await _context.Creditos.Where(c =>c.Estado == "ACTIVO" || c.Estado == "ATRASADO").SumAsync(c => (decimal?)c.SaldoCapital) ?? 0;
            ViewBag.TotalPagos = await _context.Pagos.SumAsync(p => (decimal?)p.MontoTotal) ?? 0;

            return View();
        }
    }
}