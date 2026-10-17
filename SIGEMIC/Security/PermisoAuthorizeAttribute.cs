using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SIGEMIC.Services;

namespace SIGEMIC.Security;

public class PermisoAuthorizeAttribute : TypeFilterAttribute
{
    public PermisoAuthorizeAttribute(string codigoPermiso)
        : base(typeof(PermisoAuthorizationFilter))
    {
        Arguments = new object[] { codigoPermiso };
    }
}

public class PermisoAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly string _codigoPermiso;
    private readonly PermisoService _permisoService;

    public PermisoAuthorizationFilter(
        string codigoPermiso,
        PermisoService permisoService)
    {
        _codigoPermiso = codigoPermiso;
        _permisoService = permisoService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return;
        }

        // El administrador tiene acceso completo.
        if (user.IsInRole("ADMINISTRADOR"))
            return;

        // Estos permisos personalizados son para CAJEROS.
        if (!user.IsInRole("CAJERO"))
        {
            Denegar(context);
            return;
        }

        var idTexto = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!uint.TryParse(idTexto, out uint idUsuario))
        {
            Denegar(context);
            return;
        }

        var autorizado = await _permisoService
            .TienePermisoAsync(idUsuario, _codigoPermiso);

        if (!autorizado)
            Denegar(context);
    }

    private static void Denegar(AuthorizationFilterContext context)
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

        context.Result = new ViewResult
        {
            ViewName = "~/Views/Shared/AccessDenied.cshtml"
        };
    }
}