using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            string? accion,
            string? resultado,
            string? usuario,
            DateTime? desde,
            DateTime? hasta)
        {
            IQueryable<Auditoria> consulta =
                _context.Auditorias
                    .AsNoTracking()
                    .Include(a => a.Usuario);

            // =============================================
            // FILTRO POR ACCIÓN
            // =============================================

            if (!string.IsNullOrWhiteSpace(accion))
            {
                accion = accion.Trim();

                consulta = consulta.Where(a =>
                    a.Accion.Contains(accion)
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
            // FILTRO POR USUARIO
            // =============================================

            if (!string.IsNullOrWhiteSpace(usuario))
            {
                usuario = usuario.Trim();

                consulta = consulta.Where(a =>
                    a.Usuario != null &&
                    (
                        a.Usuario.Username.Contains(usuario) ||
                        a.Usuario.Nombres.Contains(usuario) ||
                        a.Usuario.Apellidos.Contains(usuario)
                    )
                );
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
            // Se suma un día para incluir todo el día.
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
            // CONSERVAR FILTROS EN LA VISTA
            // =============================================

            ViewBag.Accion = accion;

            ViewBag.Resultado = resultado;

            ViewBag.Usuario = usuario;

            ViewBag.Desde =
                desde?.ToString(
                    "yyyy-MM-dd"
                );

            ViewBag.Hasta =
                hasta?.ToString(
                    "yyyy-MM-dd"
                );

            // =============================================
            // OBTENER REGISTROS
            // Máximo 500 para no cargar demasiado.
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