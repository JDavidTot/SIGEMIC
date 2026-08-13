using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Models;
using Microsoft.AspNetCore.Identity;

namespace SIGEMIC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

     [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    var usuario = await _context.Usuarios
        .FirstOrDefaultAsync(u =>
            u.Username == model.Username &&
            u.Estado == "ACTIVO");

    if (usuario == null)
    {
        ModelState.AddModelError(
            string.Empty,
            "Usuario o contraseña incorrectos."
        );

        return View(model);
    }

    var hasher = new PasswordHasher<Usuario>();

    var resultado = hasher.VerifyHashedPassword(
        usuario,
        usuario.PasswordHash,
        model.Password
    );

    if (resultado == PasswordVerificationResult.Failed)
    {
        ModelState.AddModelError(
            string.Empty,
            "Usuario o contraseña incorrectos."
        );

        return View(model);
    }

    return RedirectToAction("Index", "Home");
}
    }
}