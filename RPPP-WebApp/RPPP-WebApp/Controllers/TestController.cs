using Microsoft.AspNetCore.Mvc;
using RPPP_WebApp.Model;
using System.Linq;

namespace RPPP_WebApp.Controllers {
  public class TestController : Controller {
    private readonly Rppp01Context _context;

    public TestController(Rppp01Context context) {
      _context = context;
    }

    public IActionResult TestDatabaseConnection() {
      var sampleData = _context.Owner.ToList();

      return View(sampleData);
    }
  }
}
