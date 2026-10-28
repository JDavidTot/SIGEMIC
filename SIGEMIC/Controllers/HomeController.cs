using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SIGEMIC.Models;

namespace SIGEMIC.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // =============================================
        // SI NO HAY SESIÓN -> LOGIN
        // =============================================

        if (
            User.Identity?.IsAuthenticated
            != true
        )
        {
            return RedirectToAction(
                "Login",
                "Account"
            );
        }

        // =============================================
        // ADMINISTRADOR -> DASHBOARD
        // =============================================

        if (
            User.IsInRole(
                "ADMINISTRADOR"
            )
        )
        {
            return RedirectToAction(
                "Index",
                "Admin"
            );
        }

        // =============================================
        // CAJERO -> DASHBOARD
        // =============================================

        if (
            User.IsInRole(
                "CAJERO"
            )
        )
        {
            return RedirectToAction(
                "Index",
                "Cajero"
            );
        }

        // =============================================
        // CLIENTE -> MI CUENTA
        // =============================================

        if (
            User.IsInRole(
                "CLIENTE"
            )
        )
        {
            return RedirectToAction(
                "Index",
                "Cliente"
            );
        }

        // =============================================
        // ROL NO RECONOCIDO
        // =============================================

        return RedirectToAction(
            "AccessDenied",
            "Account"
        );
    }

    public IActionResult Privacy()
    {
        return RedirectToAction(
            nameof(Index)
        );
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true
    )]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel
            {
                RequestId =
                    Activity.Current?.Id ??
                    HttpContext.TraceIdentifier
            }
        );
    }
}