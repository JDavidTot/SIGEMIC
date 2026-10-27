using System.Net;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using SIGEMIC.Data;
using SIGEMIC.Models;

namespace SIGEMIC.Services;

public class AuditoriaService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditoriaService(
        IServiceScopeFactory scopeFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _scopeFactory = scopeFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task RegistrarAsync(
        uint? idUsuario,
        string accion,
        string? tablaAfectada = null,
        string? registroAfectado = null,
        string? descripcion = null,
        string resultado = "EXITOSO")
    {
        if (string.IsNullOrWhiteSpace(accion))
        {
            throw new ArgumentException(
                "La acción de auditoría es obligatoria.",
                nameof(accion)
            );
        }

        resultado =
            resultado.Equals(
                "FALLIDO",
                StringComparison.OrdinalIgnoreCase
            )
                ? "FALLIDO"
                : "EXITOSO";

        var auditoria = new Auditoria
        {
            IdUsuario = idUsuario,

            FechaHora = DateTime.Now,

            Accion = Limitar(
                accion.Trim(),
                100
            )!,

            TablaAfectada = Limitar(
                tablaAfectada,
                100
            ),

            RegistroAfectado = Limitar(
                registroAfectado,
                100
            ),

            DireccionIp = Limitar(
                ObtenerDireccionIp(),
                45
            ),

            Descripcion = Limitar(
                descripcion,
                1000
            ),

            Resultado = resultado
        };

        // ==============================================
        // CONTEXTO INDEPENDIENTE PARA AUDITORÍA
        //
        // Esto evita que una transacción de pagos,
        // créditos o solicitudes interfiera con el
        // registro de auditoría.
        // ==============================================

        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        context.Auditorias.Add(
            auditoria
        );

        await context.SaveChangesAsync();
    }

    public async Task RegistrarUsuarioActualAsync(
        string accion,
        string? tablaAfectada = null,
        string? registroAfectado = null,
        string? descripcion = null,
        string resultado = "EXITOSO")
    {
        uint? idUsuario =
            ObtenerIdUsuarioActual();

        await RegistrarAsync(
            idUsuario,
            accion,
            tablaAfectada,
            registroAfectado,
            descripcion,
            resultado
        );
    }

    public uint? ObtenerIdUsuarioActual()
    {
        var usuario =
            _httpContextAccessor
                .HttpContext?
                .User;

        if (
            usuario?.Identity?.IsAuthenticated
            != true
        )
        {
            return null;
        }

        string? idTexto =
            usuario.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        if (
            uint.TryParse(
                idTexto,
                out uint idUsuario
            )
        )
        {
            return idUsuario;
        }

        return null;
    }

    private string? ObtenerDireccionIp()
    {
        var httpContext =
            _httpContextAccessor
                .HttpContext;

        if (httpContext == null)
        {
            return null;
        }

        // ==============================================
        // X-FORWARDED-FOR
        // Para cuando SIGEMIC esté detrás de proxy.
        // ==============================================

        string? forwardedFor =
            httpContext
                .Request
                .Headers["X-Forwarded-For"]
                .FirstOrDefault();

        if (
            !string.IsNullOrWhiteSpace(
                forwardedFor
            )
        )
        {
            string? primeraIp =
                forwardedFor
                    .Split(',')
                    .FirstOrDefault()?
                    .Trim();

            if (
                !string.IsNullOrWhiteSpace(
                    primeraIp
                ) &&
                IPAddress.TryParse(
                    primeraIp,
                    out IPAddress? ipForwarded
                )
            )
            {
                return NormalizarIp(
                    ipForwarded
                );
            }
        }

        // ==============================================
        // IP DIRECTA
        // ==============================================

        IPAddress? ip =
            httpContext
                .Connection
                .RemoteIpAddress;

        if (ip == null)
        {
            return null;
        }

        return NormalizarIp(ip);
    }

    private static string NormalizarIp(
        IPAddress ip)
    {
        // ::1 equivale a localhost.
        if (IPAddress.IsLoopback(ip))
        {
            return "127.0.0.1";
        }

        // ::ffff:192.168.0.25
        // se convierte a 192.168.0.25.
        if (ip.IsIPv4MappedToIPv6)
        {
            ip = ip.MapToIPv4();
        }

        return ip.ToString();
    }

    private static string? Limitar(
        string? valor,
        int longitudMaxima)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        valor = valor.Trim();

        if (valor.Length <= longitudMaxima)
        {
            return valor;
        }

        return valor[..longitudMaxima];
    }
}