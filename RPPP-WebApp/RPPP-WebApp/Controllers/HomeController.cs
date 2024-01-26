using Microsoft.AspNetCore.Mvc;

namespace RPPP_WebApp.Controllers
{
  /// <summary>
  /// Controller for handling home-related actions.
  /// </summary>
  public class HomeController : Controller
  {
    /// <summary>
    /// Displays the index view.
    /// </summary>
    /// <returns>The result of the action.</returns>
    public IActionResult Index()
    {
      return View();
    }
  }
}
