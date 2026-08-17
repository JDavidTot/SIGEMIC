using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIGEMIC.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}