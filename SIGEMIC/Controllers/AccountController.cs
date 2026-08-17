using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

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

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u =>u.Username == model.Username &&u.Estado == "ACTIVO");

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty,"Usuario o contraseña incorrectos."
                );

                return View(model);
            }

            var hasher = new PasswordHasher<Usuario>();

            var resultado = hasher.VerifyHashedPassword(usuario,usuario.PasswordHash,model.Password
            );

            if (resultado == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty,"Usuario o contraseña incorrectos."
                );

                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name,usuario.Username),
                new Claim(ClaimTypes.Role,usuario.Rol)
            };

            var identity = new ClaimsIdentity(claims,"SIGEMICCookie");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("SIGEMICCookie",principal);

            if (usuario.Rol == "ADMINISTRADOR")
            {
                return RedirectToAction("Index", "Admin");
            }

            if (usuario.Rol == "CAJERO")
            {
                return RedirectToAction("Index", "Cajero");
            }

            if (usuario.Rol == "CLIENTE")
            {
                return RedirectToAction("Index", "Cliente");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("SIGEMICCookie");

            return RedirectToAction("Login", "Account");
        }
    }
}