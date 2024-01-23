using Microsoft.AspNetCore.Mvc;

namespace RPPP_WebApp.Controllers
{
    public class JTableProductController : Controller
    {

        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
