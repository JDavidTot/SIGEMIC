using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Models;

namespace SIGEMIC.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class PlanesCreditoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlanesCreditoController(ApplicationDbContext context)
        {
            _context = context;
        }


        // LISTAR PLANES

        public async Task<IActionResult> Index()
        {
            var planes = await _context.PlanesCredito
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return View(planes);
        }

        // CREAR PLAN - GET


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // CREAR PLAN - POST


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlanCredito model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool existeNombre = await _context.PlanesCredito
                .AnyAsync(p => p.Nombre == model.Nombre);

            if (existeNombre)
            {
                ModelState.AddModelError(
                    nameof(model.Nombre),
                    "Ya existe un plan con ese nombre."
                );

                return View(model);
            }

            model.Nombre = model.Nombre.Trim();
            model.Descripcion = model.Descripcion?.Trim();
            model.Estado = "ACTIVO";
            model.FechaCreacion = DateTime.Now;

            _context.PlanesCredito.Add(model);

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                "Plan de crédito creado correctamente.";

            return RedirectToAction(nameof(Index));
        }


        // EDITAR PLAN - GET


        [HttpGet]
        public async Task<IActionResult> Edit(uint id)
        {
            var plan = await _context.PlanesCredito
                .FirstOrDefaultAsync(p => p.IdPlan == id);

            if (plan == null)
            {
                return NotFound();
            }

            return View(plan);
        }

        // EDITAR PLAN - POST
   

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            uint id,
            PlanCredito model)
        {
            if (id != model.IdPlan)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool nombreDuplicado = await _context.PlanesCredito
                .AnyAsync(p =>
                    p.Nombre == model.Nombre &&
                    p.IdPlan != model.IdPlan);

            if (nombreDuplicado)
            {
                ModelState.AddModelError(
                    nameof(model.Nombre),
                    "Ya existe otro plan con ese nombre."
                );

                return View(model);
            }

            var plan = await _context.PlanesCredito
                .FirstOrDefaultAsync(p => p.IdPlan == id);

            if (plan == null)
            {
                return NotFound();
            }

            plan.Nombre = model.Nombre.Trim();
            plan.Descripcion = model.Descripcion?.Trim();
            plan.TipoTasa = model.TipoTasa;
            plan.PorcentajeInteres = model.PorcentajeInteres;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                "Plan de crédito actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }


        // ACTIVAR / INACTIVAR PLAN
  

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(uint id)
        {
            var plan = await _context.PlanesCredito
                .FirstOrDefaultAsync(p => p.IdPlan == id);

            if (plan == null)
            {
                return NotFound();
            }

            if (plan.Estado == "ACTIVO")
            {
                plan.Estado = "INACTIVO";
            }
            else
            {
                plan.Estado = "ACTIVO";
            }

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                $"El plan ahora está {plan.Estado}.";

            return RedirectToAction(nameof(Index));
        }
    }
}