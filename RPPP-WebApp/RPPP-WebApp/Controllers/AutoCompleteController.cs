using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers {
  public class AutoCompleteController : Controller {
    private readonly Rppp01Context ctx;
    private readonly AppSettings appData;

    public AutoCompleteController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options) {
      this.ctx = ctx;
      appData = options.Value;
    }

    public async Task<IEnumerable<IdLabel>> Owner(string term) {
      var query = ctx.Owner
                      .Select(o => new IdLabel {
                        Id = o.Oib,
                        Label = o.Name + " " + o.Surname + " (" + o.Oib + ")"
                      })
                      .Where(l => l.Label.Contains(term));

      var list = await query.OrderBy(l => l.Label)
                            .Take(appData.AutoCompleteCount)
                            .ToListAsync();
      return list;
    }
  }
}
