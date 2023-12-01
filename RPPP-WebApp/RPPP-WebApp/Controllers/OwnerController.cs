using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers {
	public class OwnerController : Controller {
		private readonly Rppp01Context ctx;
    private readonly ILogger<OwnerController> logger;
    private readonly AppSettings appData;

		public OwnerController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<OwnerController> logger) {
			this.ctx = ctx;
      this.logger = logger;
      appData = options.Value;
		}

		public IActionResult Index(int page = 1, int sort = 1, bool ascending = true) {
      int pagesize = appData.PageSize;
      var query = ctx.Owner
                     .AsNoTracking();

      int count = query.Count();
      if (count == 0) {
        logger.LogInformation("Ne postoji niti jedan vlasnik.");
        TempData[Constants.Message] = "Ne postoji niti jedan vlasnik.";
        TempData[Constants.ErrorOccurred] = false;
        return RedirectToAction(nameof(Create));
      }

      var pagingInfo = new PagingInfo {
        CurrentPage = page,
        Sort = sort,
        Ascending = ascending,
        ItemsPerPage = pagesize,
        TotalItems = count
      };
      if (page < 1 || page > pagingInfo.TotalPages) {
        return RedirectToAction(nameof(Index), new { page = pagingInfo.TotalPages, sort, ascending });
      }

      query = query.ApplySort(sort, ascending);

      var owner = query
                  .Skip((page - 1) * pagesize)
                  .Take(pagesize)
                  .ToList();

      var model = new OwnerViewModel {
        Owner = owner,
        PagingInfo = pagingInfo
      };

      return View(model);
		}

    [HttpGet]
    public IActionResult Create() {
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Owner owner) {
      logger.LogTrace(JsonSerializer.Serialize(owner));
      if (ModelState.IsValid) {
        try {
          ctx.Add(owner);
          ctx.SaveChanges();
          logger.LogInformation(new EventId(1000), $" Vlasnik {owner.Name} {owner.Surname} je dodan.");

          TempData[Constants.Message] = $"Vlasnik {owner.Name} {owner.Surname} je dodan.";
          TempData[Constants.ErrorOccurred] = false;
          return RedirectToAction(nameof(Index));
        }
        catch (Exception exc) {
          logger.LogError("Pogreška prilikom dodavanja novog vlasnika: {0}", exc.CompleteExceptionMessage());
          ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
          return View(owner);
        }
      }
      else {
        return View(owner);
      }
    }
  }
}


