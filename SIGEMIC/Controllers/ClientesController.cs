using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Models;
using SIGEMIC.Security;
using SIGEMIC.Services;

namespace SIGEMIC.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR,CAJERO")]
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditoriaService _auditoriaService;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(
            ApplicationDbContext context,
            AuditoriaService auditoriaService,
            ILogger<ClientesController> logger)
        {
            _context = context;
            _auditoriaService = auditoriaService;
            _logger = logger;
        }

        [PermisoAuthorize("CLIENTES_VER")]
        public async Task<IActionResult> Index()
        {
            var clientes = await _context.Clientes
                .Include(c => c.Usuario)
                .OrderBy(c => c.CodigoCliente)
                .ToListAsync();

            return View(clientes);
        }

        [HttpGet]
        [PermisoAuthorize("CLIENTES_VER")]
        public async Task<IActionResult> Details(uint id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .Include(c => c.Creditos)
                    .ThenInclude(cr => cr.PlanCredito)
                .FirstOrDefaultAsync(c =>
                    c.IdCliente == id
                );

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // =====================================================
        // CREAR - GET
        // =====================================================

        [HttpGet]
        [PermisoAuthorize("CLIENTES_CREAR")]
        public IActionResult Create()
        {
            return View();
        }

        // =====================================================
        // CREAR - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermisoAuthorize("CLIENTES_CREAR")]
        public async Task<IActionResult> Create(
            CrearClienteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Username =
                model.Username.Trim();

            model.Correo =
                CorreoHelper.Normalizar(
                    model.Correo
                );

            // =================================================
            // VALIDAR USERNAME
            // =================================================

            bool existeUsuario =
                await _context.Usuarios
                    .AnyAsync(u =>
                        u.Username == model.Username
                    );

            if (existeUsuario)
            {
                ModelState.AddModelError(
                    nameof(model.Username),
                    "El nombre de usuario ya existe."
                );

                return View(model);
            }

            // =================================================
            // VALIDAR CORREO
            // =================================================

            byte[] correoHash =
                CorreoHelper.CalcularHash(
                    model.Correo
                );

            if (
                await ExisteCorreoAsync(
                    correoHash
                )
            )
            {
                ModelState.AddModelError(
                    nameof(model.Correo),
                    "El correo electrónico ya está asociado a otra cuenta."
                );

                return View(model);
            }

            // =================================================
            // VALIDAR DPI
            // =================================================

            byte[] dpiHash =
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(
                        model.Dpi.Trim()
                    )
                );

            bool existeDpi =
                await _context.Clientes
                    .AnyAsync(c =>
                        c.DpiHash == dpiHash
                    );

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
                // =============================================
                // USUARIO
                // =============================================

                var usuario = new Usuario
                {
                    Nombres =
                        model.Nombres.Trim(),

                    Apellidos =
                        model.Apellidos.Trim(),

                    Username =
                        model.Username,

                    CorreoCifrado =
                        CorreoHelper.ConvertirABytes(
                            model.Correo
                        ),

                    CorreoHash =
                        correoHash,

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

                _context.Usuarios.Add(
                    usuario
                );

                await _context.SaveChangesAsync();

                // =============================================
                // CLIENTE
                // =============================================

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
                            model.Direccion
                        )
                            ? null
                            : Encoding.UTF8.GetBytes(
                                model.Direccion.Trim()
                            ),

                    TelefonoCifrado =
                        string.IsNullOrWhiteSpace(
                            model.Telefono
                        )
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

                _context.Clientes.Add(
                    cliente
                );

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // =============================================
                // AUDITORÍA
                // =============================================

                await RegistrarAuditoriaSeguroAsync(
                    "CLIENTE_CREADO",
                    "clientes",
                    cliente.IdCliente.ToString(),

                    $"Cliente {cliente.CodigoCliente} creado; " +
                    $"Usuario: {usuario.Username}; " +
                    $"Correo: Registrado; " +
                    $"Estado: {cliente.Estado}."
                );

                TempData["MensajeExito"] =
                    $"Cliente {cliente.CodigoCliente} creado correctamente.";

                return RedirectToAction(
                    nameof(Index)
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error al crear el cliente."
                );

                ModelState.AddModelError(
                    string.Empty,
                    $"No fue posible registrar el cliente: {ex.Message}"
                );

                return View(model);
            }
        }

        // =====================================================
        // EDITAR - GET
        // =====================================================

        [HttpGet]
        [PermisoAuthorize("CLIENTES_EDITAR")]
        public async Task<IActionResult> Edit(
            uint id)
        {
            var cliente =
                await _context.Clientes
                    .Include(c => c.Usuario)
                    .FirstOrDefaultAsync(c =>
                        c.IdCliente == id
                    );

            if (cliente == null)
            {
                return NotFound();
            }

            var model =
                new EditarClienteViewModel
                {
                    IdCliente =
                        cliente.IdCliente,

                    Nombres =
                        cliente.Usuario.Nombres,

                    Apellidos =
                        cliente.Usuario.Apellidos,

                    Username =
                        cliente.Usuario.Username,

                    Correo =
                        CorreoHelper.LeerCorreo(
                            cliente.Usuario.CorreoCifrado
                        )
                        ?? string.Empty,

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

            if (
                cliente.DireccionCifrada != null
            )
            {
                model.Direccion =
                    Encoding.UTF8.GetString(
                        cliente.DireccionCifrada
                    );
            }

            if (
                cliente.TelefonoCifrado != null
            )
            {
                model.Telefono =
                    Encoding.UTF8.GetString(
                        cliente.TelefonoCifrado
                    );
            }

            return View(model);
        }

        // =====================================================
        // EDITAR - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermisoAuthorize("CLIENTES_EDITAR")]
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

            var cliente =
                await _context.Clientes
                    .Include(c => c.Usuario)
                    .FirstOrDefaultAsync(c =>
                        c.IdCliente == id
                    );

            if (cliente == null)
            {
                return NotFound();
            }

            model.Username =
                model.Username.Trim();

            model.Correo =
                CorreoHelper.Normalizar(
                    model.Correo
                );

            // =================================================
            // VALIDAR USERNAME
            // =================================================

            bool usuarioDuplicado =
                await _context.Usuarios
                    .AnyAsync(u =>
                        u.Username ==
                            model.Username &&
                        u.IdUsuario !=
                            cliente.IdUsuario
                    );

            if (usuarioDuplicado)
            {
                ModelState.AddModelError(
                    nameof(model.Username),
                    "El nombre de usuario ya está siendo utilizado."
                );

                return View(model);
            }

            // =================================================
            // VALIDAR CORREO
            // =================================================

            byte[] correoHashNuevo =
                CorreoHelper.CalcularHash(
                    model.Correo
                );

            if (
                await ExisteCorreoAsync(
                    correoHashNuevo,
                    cliente.IdUsuario
                )
            )
            {
                ModelState.AddModelError(
                    nameof(model.Correo),
                    "El correo electrónico ya está asociado a otra cuenta."
                );

                return View(model);
            }

            // =================================================
            // VALORES ANTERIORES
            // =================================================

            string? correoAnterior =
                CorreoHelper.LeerCorreo(
                    cliente.Usuario.CorreoCifrado
                );

            string? direccionAnterior =
                cliente.DireccionCifrada == null
                    ? null
                    : Encoding.UTF8.GetString(
                        cliente.DireccionCifrada
                    );

            string? telefonoAnterior =
                cliente.TelefonoCifrado == null
                    ? null
                    : Encoding.UTF8.GetString(
                        cliente.TelefonoCifrado
                    );

            // =================================================
            // VALORES NUEVOS
            // =================================================

            string nombresNuevo =
                model.Nombres.Trim();

            string apellidosNuevo =
                model.Apellidos.Trim();

            string usernameNuevo =
                model.Username;

            string correoNuevo =
                model.Correo;

            string? direccionNueva =
                NormalizarTexto(
                    model.Direccion
                );

            string? telefonoNuevo =
                NormalizarTexto(
                    model.Telefono
                );

            string? profesionNueva =
                NormalizarTexto(
                    model.Profesion
                );

            string? lugarTrabajoNuevo =
                NormalizarTexto(
                    model.LugarTrabajo
                );

            // =================================================
            // AUDITAR CAMBIOS
            // =================================================

            var cambios =
                new List<string>();

            AgregarCambio(
                cambios,
                "Nombres",
                cliente.Usuario.Nombres,
                nombresNuevo
            );

            AgregarCambio(
                cambios,
                "Apellidos",
                cliente.Usuario.Apellidos,
                apellidosNuevo
            );

            AgregarCambio(
                cambios,
                "Usuario",
                cliente.Usuario.Username,
                usernameNuevo
            );

            // No exponemos el correo real en auditoría.
            AgregarCambioProtegido(
                cambios,
                "Correo",
                correoAnterior,
                correoNuevo
            );

            AgregarCambio(
                cambios,
                "Fecha de nacimiento",
                FormatearFecha(
                    cliente.FechaNacimiento
                ),
                FormatearFecha(
                    model.FechaNacimiento
                )
            );

            AgregarCambioProtegido(
                cambios,
                "Dirección",
                direccionAnterior,
                direccionNueva
            );

            AgregarCambioProtegido(
                cambios,
                "Teléfono",
                telefonoAnterior,
                telefonoNuevo
            );

            AgregarCambio(
                cambios,
                "Profesión",
                cliente.Profesion,
                profesionNueva
            );

            AgregarCambio(
                cambios,
                "Lugar de trabajo",
                cliente.LugarTrabajo,
                lugarTrabajoNuevo
            );

            AgregarCambio(
                cambios,
                "Ingreso mensual",
                FormatearMonto(
                    cliente.IngresoMensual
                ),
                FormatearMonto(
                    model.IngresoMensual
                )
            );

            try
            {
                // =============================================
                // ACTUALIZAR USUARIO
                // =============================================

                cliente.Usuario.Nombres =
                    nombresNuevo;

                cliente.Usuario.Apellidos =
                    apellidosNuevo;

                cliente.Usuario.Username =
                    usernameNuevo;

                cliente.Usuario.CorreoCifrado =
                    CorreoHelper.ConvertirABytes(
                        correoNuevo
                    );

                cliente.Usuario.CorreoHash =
                    correoHashNuevo;

                // =============================================
                // ACTUALIZAR CLIENTE
                // =============================================

                cliente.FechaNacimiento =
                    model.FechaNacimiento;

                cliente.DireccionCifrada =
                    direccionNueva == null
                        ? null
                        : Encoding.UTF8.GetBytes(
                            direccionNueva
                        );

                cliente.TelefonoCifrado =
                    telefonoNuevo == null
                        ? null
                        : Encoding.UTF8.GetBytes(
                            telefonoNuevo
                        );

                cliente.Profesion =
                    profesionNueva;

                cliente.LugarTrabajo =
                    lugarTrabajoNuevo;

                cliente.IngresoMensual =
                    model.IngresoMensual;

                await _context.SaveChangesAsync();

                string descripcion;

                if (cambios.Count == 0)
                {
                    descripcion =
                        $"Cliente {cliente.CodigoCliente} actualizado. " +
                        "No se detectaron cambios en los datos editables.";
                }
                else
                {
                    descripcion =
                        $"Cliente {cliente.CodigoCliente} actualizado. " +
                        $"Cambios: {string.Join("; ", cambios)}.";
                }

                await RegistrarAuditoriaSeguroAsync(
                    "CLIENTE_EDITADO",
                    "clientes",
                    cliente.IdCliente.ToString(),
                    descripcion
                );

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
                _logger.LogError(
                    ex,
                    "Error al actualizar el cliente {IdCliente}.",
                    id
                );

                ModelState.AddModelError(
                    string.Empty,
                    $"No fue posible actualizar el cliente: {ex.Message}"
                );

                return View(model);
            }
        }

        // =====================================================
        // CAMBIAR ESTADO
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermisoAuthorize(
            "CLIENTES_CAMBIAR_ESTADO"
        )]
        public async Task<IActionResult> CambiarEstado(
            uint id)
        {
            var cliente =
                await _context.Clientes
                    .Include(c => c.Usuario)
                    .FirstOrDefaultAsync(c =>
                        c.IdCliente == id
                    );

            if (cliente == null)
            {
                return NotFound();
            }

            string estadoAnterior =
                cliente.Estado;

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

            await RegistrarAuditoriaSeguroAsync(
                "CLIENTE_ESTADO_CAMBIADO",
                "clientes",
                cliente.IdCliente.ToString(),

                $"Cliente {cliente.CodigoCliente} actualizado. " +
                $"Cambios: Estado: " +
                $"{MostrarValor(estadoAnterior)} → " +
                $"{MostrarValor(cliente.Estado)}."
            );

            return RedirectToAction(
                nameof(Index)
            );
        }

        // =====================================================
        // VALIDAR CORREO DUPLICADO
        // =====================================================

        private async Task<bool> ExisteCorreoAsync(
            byte[] correoHash,
            uint? excluirIdUsuario = null)
        {
            var hashes =
                await _context.Usuarios
                    .AsNoTracking()
                    .Where(u =>
                        u.CorreoHash != null &&
                        (
                            !excluirIdUsuario.HasValue ||
                            u.IdUsuario !=
                                excluirIdUsuario.Value
                        )
                    )
                    .Select(u =>
                        u.CorreoHash!
                    )
                    .ToListAsync();

            return hashes.Any(hash =>
                hash.Length ==
                    correoHash.Length &&
                CryptographicOperations
                    .FixedTimeEquals(
                        hash,
                        correoHash
                    )
            );
        }

        // =====================================================
        // AUDITORÍA SEGURA
        // =====================================================

        private async Task RegistrarAuditoriaSeguroAsync(
            string accion,
            string tablaAfectada,
            string registroAfectado,
            string descripcion)
        {
            try
            {
                await _auditoriaService
                    .RegistrarUsuarioActualAsync(
                        accion,
                        tablaAfectada,
                        registroAfectado,
                        descripcion,
                        "EXITOSO"
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "No fue posible registrar la auditoría de la acción {Accion}.",
                    accion
                );
            }
        }

        // =====================================================
        // UTILIDADES DE AUDITORÍA
        // =====================================================

        private static void AgregarCambio(
            ICollection<string> cambios,
            string campo,
            string? valorAnterior,
            string? valorNuevo)
        {
            string anterior =
                NormalizarParaComparar(
                    valorAnterior
                );

            string nuevo =
                NormalizarParaComparar(
                    valorNuevo
                );

            if (
                string.Equals(
                    anterior,
                    nuevo,
                    StringComparison.Ordinal
                )
            )
            {
                return;
            }

            cambios.Add(
                $"{campo}: " +
                $"{MostrarValor(valorAnterior)} → " +
                $"{MostrarValor(valorNuevo)}"
            );
        }

        private static void AgregarCambioProtegido(
            ICollection<string> cambios,
            string campo,
            string? valorAnterior,
            string? valorNuevo)
        {
            string anterior =
                NormalizarParaComparar(
                    valorAnterior
                );

            string nuevo =
                NormalizarParaComparar(
                    valorNuevo
                );

            if (
                string.Equals(
                    anterior,
                    nuevo,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return;
            }

            string estadoAnterior =
                string.IsNullOrWhiteSpace(
                    valorAnterior
                )
                    ? "Sin dato"
                    : "Dato registrado";

            string estadoNuevo =
                string.IsNullOrWhiteSpace(
                    valorNuevo
                )
                    ? "Sin dato"
                    : "Dato actualizado";

            cambios.Add(
                $"{campo}: " +
                $"{estadoAnterior} → " +
                $"{estadoNuevo}"
            );
        }

        private static string? NormalizarTexto(
            string? valor)
        {
            return string.IsNullOrWhiteSpace(
                valor
            )
                ? null
                : valor.Trim();
        }

        private static string NormalizarParaComparar(
            string? valor)
        {
            return string.IsNullOrWhiteSpace(
                valor
            )
                ? string.Empty
                : valor.Trim();
        }

        private static string MostrarValor(
            string? valor)
        {
            return string.IsNullOrWhiteSpace(
                valor
            )
                ? "Sin dato"
                : valor.Trim();
        }

        private static string FormatearFecha(
            DateTime? fecha)
        {
            return fecha.HasValue
                ? fecha.Value.ToString(
                    "dd/MM/yyyy"
                )
                : "Sin dato";
        }

        private static string FormatearMonto(
            decimal? monto)
        {
            return monto.HasValue
                ? $"Q{monto.Value:N2}"
                : "Sin dato";
        }
    }
}