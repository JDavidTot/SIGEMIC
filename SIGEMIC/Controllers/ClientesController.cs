using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Models;

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
            var clientes = await _context.Clientes
                .Include(c => c.Usuario)
                .OrderBy(c => c.CodigoCliente)
                .ToListAsync();

            return View(clientes);
        }


        [HttpGet]
        public async Task<IActionResult> Details(uint id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .Include(c => c.Creditos)
                    .ThenInclude(cr => cr.PlanCredito)
                .FirstOrDefaultAsync(c =>
                    c.IdCliente == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CrearClienteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }



            bool existeUsuario =
                await _context.Usuarios.AnyAsync(u =>
                    u.Username == model.Username.Trim());

            if (existeUsuario)
            {
                ModelState.AddModelError(
                    nameof(model.Username),
                    "El nombre de usuario ya existe."
                );

                return View(model);
            }


            byte[] dpiHash =
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(
                        model.Dpi.Trim()
                    )
                );

            bool existeDpi =
                await _context.Clientes.AnyAsync(c =>
                    c.DpiHash == dpiHash);

            if (existeDpi)
            {
                ModelState.AddModelError(
                    nameof(model.Dpi),
                    "Ya existe un cliente registrado con este DPI."
                );

                return View(model);
            }

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {


                var usuario = new Usuario
                {
                    Nombres =
                        model.Nombres.Trim(),

                    Apellidos =
                        model.Apellidos.Trim(),

                    Username =
                        model.Username.Trim(),

                    Rol =
                        "CLIENTE",

                    Estado =
                        "ACTIVO",

                    IntentosFallidos =
                        0,

                    FechaCreacion =
                        DateTime.Now
                };

                var hasher =
                    new PasswordHasher<Usuario>();

                usuario.PasswordHash =
                    hasher.HashPassword(
                        usuario,
                        model.Password
                    );

                _context.Usuarios.Add(usuario);

                await _context.SaveChangesAsync();


                string codigoCliente =
                    $"CLI-{usuario.IdUsuario:D6}";


                var cliente = new Cliente
                {
                    IdUsuario =
                        usuario.IdUsuario,

                    CodigoCliente =
                        codigoCliente,

                    DpiCifrado =
                        Encoding.UTF8.GetBytes(
                            model.Dpi.Trim()
                        ),

                    DpiHash =
                        dpiHash,

                    FechaNacimiento =
                        model.FechaNacimiento,

                    DireccionCifrada =
                        string.IsNullOrWhiteSpace(
                            model.Direccion)
                            ? null
                            : Encoding.UTF8.GetBytes(
                                model.Direccion.Trim()
                            ),

                    TelefonoCifrado =
                        string.IsNullOrWhiteSpace(
                            model.Telefono)
                            ? null
                            : Encoding.UTF8.GetBytes(
                                model.Telefono.Trim()
                            ),

                    Profesion =
                        model.Profesion?.Trim(),

                    LugarTrabajo =
                        model.LugarTrabajo?.Trim(),

                    IngresoMensual =
                        model.IngresoMensual,

                    FechaRegistro =
                        DateTime.Now,

                    Estado =
                        "ACTIVO"
                };

                _context.Clientes.Add(cliente);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["MensajeExito"] =
                    $"Cliente {cliente.CodigoCliente} creado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                Console.WriteLine(
                    ex.ToString()
                );

                ModelState.AddModelError(
                    string.Empty,
                    $"No fue posible registrar el cliente: {ex.Message}"
                );

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(uint id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c =>
                    c.IdCliente == id);

            if (cliente == null)
            {
                return NotFound();
            }

            var model = new EditarClienteViewModel
            {
                IdCliente =
                    cliente.IdCliente,

                Nombres =
                    cliente.Usuario.Nombres,

                Apellidos =
                    cliente.Usuario.Apellidos,

                Username =
                    cliente.Usuario.Username,

                FechaNacimiento =
                    cliente.FechaNacimiento,

                Profesion =
                    cliente.Profesion,

                LugarTrabajo =
                    cliente.LugarTrabajo,

                IngresoMensual =
                    cliente.IngresoMensual,

                Estado =
                    cliente.Estado
            };

            if (cliente.DireccionCifrada != null)
            {
                model.Direccion =
                    Encoding.UTF8.GetString(
                        cliente.DireccionCifrada
                    );
            }

            if (cliente.TelefonoCifrado != null)
            {
                model.Telefono =
                    Encoding.UTF8.GetString(
                        cliente.TelefonoCifrado
                    );
            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            uint id,
            EditarClienteViewModel model)
        {
            if (id != model.IdCliente)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c =>
                    c.IdCliente == id);

            if (cliente == null)
            {
                return NotFound();
            }


            bool usuarioDuplicado =
                await _context.Usuarios.AnyAsync(u =>
                    u.Username == model.Username.Trim() &&
                    u.IdUsuario != cliente.IdUsuario);

            if (usuarioDuplicado)
            {
                ModelState.AddModelError(
                    nameof(model.Username),
                    "El nombre de usuario ya está siendo utilizado."
                );

                return View(model);
            }

            try
            {

                cliente.Usuario.Nombres = model.Nombres.Trim();

                cliente.Usuario.Apellidos = model.Apellidos.Trim();

                cliente.Usuario.Username = model.Username.Trim();


                cliente.FechaNacimiento = model.FechaNacimiento;

                cliente.DireccionCifrada =
                    string.IsNullOrWhiteSpace(
                        model.Direccion)
                        ? null
                        : Encoding.UTF8.GetBytes(
                            model.Direccion.Trim()
                        );

                cliente.TelefonoCifrado =
                    string.IsNullOrWhiteSpace(
                        model.Telefono)
                        ? null
                        : Encoding.UTF8.GetBytes(
                            model.Telefono.Trim()
                        );

                cliente.Profesion =
                    model.Profesion?.Trim();

                cliente.LugarTrabajo =
                    model.LugarTrabajo?.Trim();

                cliente.IngresoMensual =
                    model.IngresoMensual;

                await _context.SaveChangesAsync();

                TempData["MensajeExito"] =
                    "Cliente actualizado correctamente.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = cliente.IdCliente
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    ex.ToString()
                );

                ModelState.AddModelError(
                    string.Empty,
                    $"No fue posible actualizar el cliente: {ex.Message}"
                );

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(
            uint id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c =>
                    c.IdCliente == id);

            if (cliente == null)
            {
                return NotFound();
            }

            if (cliente.Estado == "ACTIVO")
            {
                cliente.Estado =
                    "INACTIVO";

                cliente.Usuario.Estado =
                    "INACTIVO";

                TempData["MensajeExito"] =
                    "Cliente inactivado correctamente.";
            }
            else
            {
                cliente.Estado =
                    "ACTIVO";

                cliente.Usuario.Estado =
                    "ACTIVO";

                TempData["MensajeExito"] =
                    "Cliente activado correctamente.";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index)
            );
        }
    }
}