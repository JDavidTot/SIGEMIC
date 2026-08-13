using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;

namespace SIGEMIC.Controllers
{
    public class PruebaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PruebaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var formasPago = await _context.FormasPago.ToListAsync();

            return Json(formasPago);
        }
    }
}