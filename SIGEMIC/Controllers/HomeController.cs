using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SIGEMIC.Models;

namespace SIGEMIC.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("ADMINISTRADOR"))
            {
                return RedirectToAction(
                    "Index",
                    "Admin"
                );
            }

            if (User.IsInRole("CAJERO"))
            {
                return RedirectToAction(
                    "Index",
                    "Cajero"
                );
            }

            if (User.IsInRole("CLIENTE"))
            {
                return RedirectToAction(
                    "Index",
                    "Cliente"
                );
            }
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
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
