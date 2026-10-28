using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Models;

namespace SIGEMIC.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class AuditoriaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditoriaController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? usuario,
            string? accion,
            string? resultado,
            DateTime? desde,
            DateTime? hasta)
        {
            IQueryable<Auditoria> consulta =
                _context.Auditorias
                    .AsNoTracking()
                    .Include(a => a.Usuario);

            // =============================================
            // FILTRO POR USUARIO
            // =============================================

            if (!string.IsNullOrWhiteSpace(usuario))
            {
                usuario = usuario.Trim();

                consulta = consulta.Where(a =>
                    a.Usuario != null &&
                    a.Usuario.Username == usuario
                );
            }

            // =============================================
            // FILTRO POR ACCIÓN
            // =============================================

            if (!string.IsNullOrWhiteSpace(accion))
            {
                accion = accion.Trim();

                consulta = consulta.Where(a =>
                    a.Accion == accion
                );
            }

            // =============================================
            // FILTRO POR RESULTADO
            // =============================================

            if (!string.IsNullOrWhiteSpace(resultado))
            {
                resultado =
                    resultado
                        .Trim()
                        .ToUpper();

                if (
                    resultado == "EXITOSO" ||
                    resultado == "FALLIDO"
                )
                {
                    consulta = consulta.Where(a =>
                        a.Resultado == resultado
                    );
                }
            }

            // =============================================
            // FILTRO DESDE
            // =============================================

            if (desde.HasValue)
            {
                DateTime fechaDesde =
                    desde.Value.Date;

                consulta = consulta.Where(a =>
                    a.FechaHora >= fechaDesde
                );
            }

            // =============================================
            // FILTRO HASTA
            // =============================================

            if (hasta.HasValue)
            {
                DateTime fechaHasta =
                    hasta.Value
                        .Date
                        .AddDays(1);

                consulta = consulta.Where(a =>
                    a.FechaHora < fechaHasta
                );
            }

            // =============================================
            // LISTA DE USUARIOS PARA FILTRO
            // =============================================

            var usuariosBase =
                await _context.Usuarios
                    .AsNoTracking()
                    .OrderBy(u => u.Rol)
                    .ThenBy(u => u.Username)
                    .Select(u => new
                    {
                        u.Username,
                        u.Rol
                    })
                    .ToListAsync();

            ViewBag.UsuariosFiltro =
                usuariosBase
                    .Select(u =>
                        new SelectListItem
                        {
                            Value =
                                u.Username,

                            Text =
                                $"{u.Username} — {u.Rol}",

                            Selected =
                                u.Username == usuario
                        }
                    )
                    .ToList();

            // =============================================
            // LISTA DE ACCIONES PARA FILTRO
            // Se obtiene automáticamente de Auditoría.
            // =============================================

            var acciones =
                await _context.Auditorias
                    .AsNoTracking()
                    .Select(a => a.Accion)
                    .Distinct()
                    .OrderBy(a => a)
                    .ToListAsync();

            ViewBag.AccionesFiltro =
                acciones
                    .Select(a =>
                        new SelectListItem
                        {
                            Value = a,

                            Text =
                                a.Replace(
                                    "_",
                                    " "
                                ),

                            Selected =
                                a == accion
                        }
                    )
                    .ToList();

            // =============================================
            // CONSERVAR FILTROS
            // =============================================

            ViewBag.Usuario =
                usuario;

            ViewBag.Accion =
                accion;

            ViewBag.Resultado =
                resultado;

            ViewBag.Desde =
                desde?.ToString(
                    "yyyy-MM-dd"
                );

            ViewBag.Hasta =
                hasta?.ToString(
                    "yyyy-MM-dd"
                );

            // =============================================
            // OBTENER AUDITORÍA
            // =============================================

            var auditorias =
                await consulta
                    .OrderByDescending(a =>
                        a.FechaHora
                    )
                    .ThenByDescending(a =>
                        a.IdAuditoria
                    )
                    .Take(500)
                    .ToListAsync();

            return View(auditorias);
        }
    }
}