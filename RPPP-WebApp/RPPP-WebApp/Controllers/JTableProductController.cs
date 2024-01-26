using Microsoft.AspNetCore.Mvc;

namespace RPPP_WebApp.Controllers
{

    /// <summary>
    /// Controller for handling JTable related product operations.
    /// </summary>
    public class JTableProductController : Controller
    {

        /// <summary>
        /// Displays the main view associated with JTable products.
        /// </summary>
        /// <returns>The index view of JTable products.</returns>
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
