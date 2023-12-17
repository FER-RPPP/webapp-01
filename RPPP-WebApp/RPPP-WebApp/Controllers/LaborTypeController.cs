using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Model;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers {
  public class LaborTypeController : Controller {
    private readonly Rppp01Context ctx;
    private readonly ILogger<LaborTypeController> logger;
    private readonly AppSettings appData;

    public LaborTypeController(Rppp01Context ctx, IOptionsSnapshot<AppSettings> options, ILogger<LaborTypeController> logger) {
      this.ctx = ctx;
      this.logger = logger;
      appData = options.Value;
    }

    public IActionResult Index(int page = 1, int sort = 1, bool ascending = true) {
      int pagesize = appData.PageSize;
      var query = ctx.LaborType
                     .AsNoTracking();

      int count = query.Count();
      if (count == 0) {
        logger.LogInformation("Ne postoji niti jedna vrsta posla.");
        TempData[Constants.Message] = "Ne postoji niti jedna vrsta posla.";
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
        return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
      }

      query = query.ApplySort(sort, ascending);

      var type = query
                  .Skip((page - 1) * pagesize)
                  .Take(pagesize)
                  .ToList();

      var model = new LaborTypeViewModel {
        LaborType = type,
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
    public IActionResult Create(LaborType laborType) {
      logger.LogTrace(JsonSerializer.Serialize(laborType));
      if (ModelState.IsValid) {
        try {
          ctx.Add(laborType);
          ctx.SaveChanges();
          logger.LogInformation(new EventId(1000), $"Vrsta {laborType.Type} je dodana.");

          TempData[Constants.Message] = $"Vrsta {laborType.Type} je dodana";
          TempData[Constants.ErrorOccurred] = false;
          return RedirectToAction(nameof(Index));
        }
        catch (Exception exc) {
          logger.LogError("Pogreška prilikom dodavanja nove vrste: " + exc.CompleteExceptionMessage());
          ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
          return View(laborType);
        }
      }
      else {
        return View(laborType);
      }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Guid id, int page = 1, int sort = 1, bool ascending = true) {
      var laborType = ctx.LaborType.Find(id);
      if (laborType != null) {
        try {
          ctx.Remove(laborType);
          ctx.SaveChanges();
          logger.LogInformation($"Vrsta {laborType.Type} uspješno obrisana.");
          TempData[Constants.Message] = $"Vrsta {laborType.Type} uspješno obrisana.";
          TempData[Constants.ErrorOccurred] = false;
        }
        catch (Exception exc) {
          TempData[Constants.Message] = "Pogreška prilikom brisanja vrste: " + exc.CompleteExceptionMessage();
          TempData[Constants.ErrorOccurred] = true;
          logger.LogError("Pogreška prilikom brisanja vrste: " + exc.CompleteExceptionMessage());
        }
      }
      else {
        logger.LogWarning("Ne postoji vrsta: " + id);
        TempData[Constants.Message] = "Ne postoji vrsta: " + id;
        TempData[Constants.ErrorOccurred] = true;
      }
      return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
    }


    [HttpGet]
    public IActionResult Edit(Guid id, int page = 1, int sort = 1, bool ascending = true) {
      var laborType = ctx.LaborType.AsNoTracking().Where(o => o.Id == id).SingleOrDefault();
      if (laborType == null) {
        logger.LogWarning("Ne postoji vrsta: " + id);
        return NotFound("Ne postoji vrsta: " + id);
      }
      else {
        ViewBag.Page = page;
        ViewBag.Sort = sort;
        ViewBag.Ascending = ascending;
        return View(laborType);
      }
    }

    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, int page = 1, int sort = 1, bool ascending = true) {
      try {
        LaborType laborType = await ctx.LaborType
                          .Where(o => o.Id == id)
                          .FirstOrDefaultAsync();
        if (laborType == null) {
          return NotFound("Neispravan id vrste: " + id);
        }

        if (await TryUpdateModelAsync<LaborType>(laborType, "",
            o => o.Type, o => o.Id
        )) {
          ViewBag.Page = page;
          ViewBag.Sort = sort;
          ViewBag.Ascending = ascending;
          try {
            await ctx.SaveChangesAsync();
            TempData[Constants.Message] = $"Vrsta {laborType.Type} ažurirana.";
            TempData[Constants.ErrorOccurred] = false;
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
          }
          catch (Exception exc) {
            ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
            return View(laborType);
          }
        }
        else {
          ModelState.AddModelError(string.Empty, "Podatke o vrsti nije moguće povezati s forme");
          return View(laborType);
        }
      }
      catch (Exception exc) {
        TempData[Constants.Message] = exc.CompleteExceptionMessage();
        TempData[Constants.ErrorOccurred] = true;
        return RedirectToAction(nameof(Edit), id);
      }
    }
  }
}
