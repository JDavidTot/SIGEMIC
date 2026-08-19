using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;

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

        public async Task<IActionResult> Index()
        {
            var idUsuarioTexto = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!uint.TryParse(idUsuarioTexto, out uint idUsuario))
            {
                return Unauthorized();
            }

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c =>c.IdUsuario == idUsuario);

            if (cliente == null)
            {
                return NotFound();
            }

            var credito = await _context.Creditos.Where(c =>c.IdCliente == cliente.IdCliente && (c.Estado == "ACTIVO" ||
                c.Estado == "ATRASADO")).OrderByDescending(c => c.FechaOtorgamiento).FirstOrDefaultAsync();

            ViewBag.CodigoCliente = cliente.CodigoCliente;

            if (credito != null)
            {
                ViewBag.TieneCredito = true;
                ViewBag.CodigoCredito = credito.CodigoCredito;
                ViewBag.MontoOriginal = credito.MontoOriginal;
                ViewBag.SaldoCapital = credito.SaldoCapital;
                ViewBag.CuotaOriginal = credito.CuotaOriginal;
                ViewBag.EstadoCredito = credito.Estado;
                ViewBag.CuotasPendientes = await _context.Cuotas.CountAsync(c =>c.IdCredito == credito.IdCredito && c.Estado != "PAGADA");
                ViewBag.TotalPagado = await _context.Pagos.Where(p =>p.IdCredito == credito.IdCredito).SumAsync(p =>(decimal?)p.MontoTotal) ?? 0;
            }
            else
            {
                ViewBag.TieneCredito = false;
            }

            return View();
        }
    }
}