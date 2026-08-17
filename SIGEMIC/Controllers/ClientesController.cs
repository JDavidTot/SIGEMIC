using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using Microsoft.AspNetCore.Identity;
using SIGEMIC.Models;
using System.Security.Cryptography;
using System.Text;

namespace SIGEMIC.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR,CAJERO")]
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var clientes = await _context.Clientes.Include(c => c.Usuario).OrderBy(c => c.CodigoCliente).ToListAsync();

            return View(clientes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrearClienteViewModel model)
{
        if (!ModelState.IsValid)
        {
        return View(model);
        }

        if (await _context.Usuarios.AnyAsync(u => u.Username == model.Username))
        {
        ModelState.AddModelError(nameof(model.Username),"El nombre de usuario ya existe.");

        return View(model);
    }

    byte[] dpiHash = SHA256.HashData(Encoding.UTF8.GetBytes(model.Dpi.Trim())
    );

    if (await _context.Clientes.AnyAsync(c => c.DpiHash == dpiHash))
    {
        ModelState.AddModelError(nameof(model.Dpi),"Ya existe un cliente registrado con ese DPI."
        );

        return View(model);
    }

    await using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
        var usuario = new Usuario
        {
            Nombres = model.Nombres.Trim(),
            Apellidos = model.Apellidos.Trim(),
            Username = model.Username.Trim(),
            Rol = "CLIENTE",
            Estado = "ACTIVO",
            IntentosFallidos = 0,
            FechaCreacion = DateTime.Now
        };

        var hasher = new PasswordHasher<Usuario>();

        usuario.PasswordHash = hasher.HashPassword(usuario, model.Password);

        _context.Usuarios.Add(usuario);

        await _context.SaveChangesAsync();

        string codigoCliente =
            $"CLI-{usuario.IdUsuario:D6}";

        var cliente = new Cliente
        {
            IdUsuario = usuario.IdUsuario,

            CodigoCliente = codigoCliente,

            DpiCifrado = Encoding.UTF8.GetBytes(model.Dpi.Trim()), DpiHash = dpiHash,FechaNacimiento = model.FechaNacimiento,DireccionCifrada =
                string.IsNullOrWhiteSpace(model.Direccion)? null: Encoding.UTF8.GetBytes(model.Direccion.Trim()),

            TelefonoCifrado = string.IsNullOrWhiteSpace(model.Telefono)? null: Encoding.UTF8.GetBytes(model.Telefono.Trim()),
            Profesion = model.Profesion?.Trim(), LugarTrabajo = model.LugarTrabajo?.Trim(),IngresoMensual = model.IngresoMensual,
            FechaRegistro = DateTime.Now,Estado = "ACTIVO"};

        _context.Clientes.Add(cliente);

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        TempData["MensajeExito"] = "Cliente registrado correctamente.";

        return RedirectToAction(nameof(Index));
    }
    catch
    {
        await transaction.RollbackAsync();

        ModelState.AddModelError(string.Empty,"No fue posible registrar el cliente.");

        return View(model);
    }
}
    }
}